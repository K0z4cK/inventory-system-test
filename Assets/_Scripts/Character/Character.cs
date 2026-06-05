using System.Collections.Generic;
using Infrastructure;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour, IControllable, IInventoryDropService
{
    [FormerlySerializedAs("_animator")]
    [SerializeField] private Animator animator;
    [FormerlySerializedAs("_speed")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private InteractableHighlightPresenter interactableHighlightPresenter;
    [FormerlySerializedAs("attackDamage")]
    [SerializeField, Min(1)] private int baseAttackDamage = 1;
    [SerializeField, Min(0.1f)] private float attackRange = 2.5f;
    [SerializeField] private Transform dropOrigin;
    [SerializeField, Min(0f)] private float dropForwardOffset = 0.75f;
    [SerializeField, Min(0f)] private float dropHeightOffset = 0.15f;

    private CharacterController _characterController;
    private PlayerContext _playerContext;
    private IInventoryService _inventory;
    private IGameplayFeedback _feedback;
    private Transform _transform;

    private IInteractable _currentInteractable;
    private Transform _currentInteractableTransform;
    private IAttackable _currentAttackable;
    private Transform _currentAttackableTransform;

    private List<Transform> _interactableQueue = new List<Transform>();
    private List<Transform> _attackableTargets = new List<Transform>();

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _playerContext = GetComponentInParent<PlayerContext>();
        if (_playerContext != null)
        {
            _playerContext.OnInitialized += HandlePlayerContextInitialized;
            ResolvePlayerServices();
        }
        else
            Debug.LogError("Character requires PlayerContext in its hierarchy.", this);

        if (interactableHighlightPresenter == null)
            interactableHighlightPresenter = GetComponentInChildren<InteractableHighlightPresenter>(true);

        _transform = transform;
    }

    private void Update()
    {
        RefreshAttackTarget();
    }

    public void Action()
    {
        RefreshAttackTarget();

        if (IsAttackableAvailable(_currentAttackable))
        {
            AttackResult result = _currentAttackable.ReceiveAttack(this, GetAttackDamage());
            if (result.Succeeded)
            {
                _feedback?.ShowDamageDealt(
                    result.TargetName,
                    result.DamageDealt,
                    result.RemainingHealth,
                    result.MaxHealth);

                if (result.Defeated)
                    _feedback?.ShowTargetDefeated(result.TargetName);

                _feedback?.ShowLootDropped(result.DroppedLootCount);
            }
            else
            {
                _feedback?.ShowNoAttackTarget();
            }

            RefreshAttackTarget();
        }
        else if (TryGetClosestAvailableAttackableName(out string targetName))
        {
            _feedback?.ShowAttackTargetOutOfRange(targetName);
        }
        else
        {
            _feedback?.ShowNoAttackTarget();
        }

        if (animator != null)
            animator.SetTrigger("Attack");
    }

    public void Interact()
    {
        if (!IsInteractableAvailable(_currentInteractable))
        {
            GetInteractableFromQueue();
            if (!IsInteractableAvailable(_currentInteractable))
                _feedback?.ShowNoInteractable();

            return;
        }

        if (!_currentInteractable.Interact(this))
            return;

        if (!IsInteractableAvailable(_currentInteractable))
            GetInteractableFromQueue();
        else if (interactableHighlightPresenter != null)
            interactableHighlightPresenter.Show(_currentInteractableTransform);

        if (animator != null)
            animator.SetTrigger("Gather");
    }

    public bool TryAddItemsToInventory(ItemObject itemObject, int count)
    {
        return TryCollectItems(itemObject, count, false, 0);
    }

    public bool TryHarvestItemsToInventory(ItemObject itemObject, int count, int remainingCount)
    {
        return TryCollectItems(itemObject, count, true, remainingCount);
    }

    public bool TryDropSlot(int slotIndex)
    {
        if (_inventory == null || !_inventory.TryRemoveSlot(slotIndex, out InventoryItem removedItem))
            return false;

        if (!TrySpawnPickable(removedItem))
        {
            _inventory.TryAddItems(removedItem.ItemObject, removedItem.Count);
            return false;
        }

        _feedback?.ShowItemDropped(removedItem.ItemObject, removedItem.Count);
        return true;
    }

    public void ShowRequiredToolFeedback(ItemObject requiredTool)
    {
        _feedback?.ShowRequiredTool(requiredTool);
    }

    public void ShowResourceDepletedFeedback(ItemObject itemObject)
    {
        _feedback?.ShowResourceDepleted(itemObject);
    }

    public bool HasSelectedItem(ItemObject itemObject)
    {
        if (_inventory == null || itemObject == null || _inventory.SelectedItem == null)
            return false;

        return new InventoryItem(_inventory.SelectedItem, 1).Matches(itemObject);
    }

    public int GetAttackDamage()
    {
        ItemObject selectedItem = _inventory != null ? _inventory.SelectedItem : null;
        return selectedItem != null ? selectedItem.AttackDamage : Mathf.Max(1, baseAttackDamage);
    }

    public void Move(Vector2 direction)
    {
        Vector3 scaledMovement = new Vector3(direction.x, 0f, direction.y) * speed * Time.fixedDeltaTime;

        _transform.LookAt(_transform.position + scaledMovement, Vector3.up);
        _characterController.Move(scaledMovement);
        if (animator != null)
            animator.SetFloat("Velocity", _characterController.velocity.magnitude);
    }

    private void OnTriggerEnter(Collider other)
    {
        IAttackable attackable = other.GetComponentInParent<IAttackable>();
        if (IsAttackableAvailable(attackable))
        {
            Transform attackableTransform = (attackable as Component)?.transform ?? other.transform;
            if (!_attackableTargets.Contains(attackableTransform))
                _attackableTargets.Add(attackableTransform);

            RefreshAttackTarget();
        }

        IInteractable interactable = other.GetComponent<IInteractable>();
        if (!IsInteractableAvailable(interactable))
            return;

        if (_currentInteractable == null)
        {
            SetCurrentInteractable(other.transform, interactable);
            return;
        }

        _interactableQueue.Add(other.transform);
    }

    private void OnTriggerExit(Collider other)
    {
        IAttackable attackable = other.GetComponentInParent<IAttackable>();
        SetAttackRangeState(attackable, false);

        Transform attackableTransform = (attackable as Component)?.transform ?? other.transform;
        _attackableTargets.Remove(attackableTransform);
        RefreshAttackTarget();

        if (other.transform == _currentInteractableTransform)
        {
            GetInteractableFromQueue();
        }
        else if(_interactableQueue.Contains(other.transform))
        {
            _interactableQueue.Remove(other.transform);
        }
    }

    private void RefreshAttackTarget()
    {
        _currentAttackable = null;
        _currentAttackableTransform = null;

        int bestPriority = int.MinValue;
        float bestDistance = float.MaxValue;

        for (int i = _attackableTargets.Count - 1; i >= 0; i--)
        {
            Transform attackableTransform = _attackableTargets[i];
            if (attackableTransform == null)
            {
                _attackableTargets.RemoveAt(i);
                continue;
            }

            IAttackable attackable = attackableTransform.GetComponent<IAttackable>();
            if (!IsAttackableAvailable(attackable))
            {
                _attackableTargets.RemoveAt(i);
                continue;
            }

            float distance = Vector3.Distance(_transform.position, attackableTransform.position);
            SetAttackRangeState(attackable, distance <= attackRange);

            if (distance > attackRange)
                continue;

            if (attackable.AttackPriority < bestPriority)
                continue;

            if (attackable.AttackPriority == bestPriority && distance >= bestDistance)
                continue;

            bestPriority = attackable.AttackPriority;
            bestDistance = distance;
            _currentAttackable = attackable;
            _currentAttackableTransform = attackableTransform;
        }
    }

    private bool TryGetClosestAvailableAttackableName(out string targetName)
    {
        targetName = null;
        float closestDistance = float.MaxValue;

        for (int i = _attackableTargets.Count - 1; i >= 0; i--)
        {
            Transform attackableTransform = _attackableTargets[i];
            if (attackableTransform == null)
            {
                _attackableTargets.RemoveAt(i);
                continue;
            }

            IAttackable attackable = attackableTransform.GetComponent<IAttackable>();
            if (!IsAttackableAvailable(attackable))
                continue;

            float distance = Vector3.Distance(_transform.position, attackableTransform.position);
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            targetName = attackable is AttackableObject attackableObject
                ? attackableObject.DisplayName
                : attackableTransform.gameObject.name;
        }

        return !string.IsNullOrWhiteSpace(targetName);
    }

    private bool TryCollectItems(ItemObject itemObject, int count, bool isHarvest, int remainingCount)
    {
        if (_inventory == null || !_inventory.TryAddItems(itemObject, count))
        {
            _feedback?.ShowInventoryFull(itemObject, count);
            return false;
        }

        if (isHarvest)
            _feedback?.ShowHarvested(itemObject, count, remainingCount);
        else
            _feedback?.ShowPickedUp(itemObject, count);

        return true;
    }

    private bool TrySpawnPickable(InventoryItem item)
    {
        if (item.IsEmpty || item.ItemObject.PickablePrefab == null)
        {
            Debug.LogWarning("Cannot drop item because it has no PickablePrefab assigned.", this);
            return false;
        }

        Transform origin = dropOrigin != null ? dropOrigin : _transform;
        Vector3 spawnPosition = origin.position
            + origin.forward * dropForwardOffset
            + Vector3.up * dropHeightOffset;

        PickableItem pickableItem = Instantiate(
            item.ItemObject.PickablePrefab,
            spawnPosition,
            item.ItemObject.PickablePrefab.transform.rotation);

        pickableItem.Initialize(item.ItemObject, item.Count);
        return true;
    }

    private void GetInteractableFromQueue()
    {
        ClearCurrentInteractable();

        if (_interactableQueue.Count == 0)
            return;

        Transform interactableTransform = _interactableQueue[0];
        if (interactableTransform == null)
        {
            _interactableQueue.RemoveAt(0);
            GetInteractableFromQueue();
            return;
        }

        IInteractable interactable = interactableTransform.GetComponent<IInteractable>();
        if (!IsInteractableAvailable(interactable))
        {
            _interactableQueue.RemoveAt(0);
            GetInteractableFromQueue();
            return;
        }

        SetCurrentInteractable(interactableTransform, interactable);
        _interactableQueue.RemoveAt(0);
    }

    private void SetCurrentInteractable(Transform interactableTransform, IInteractable interactable)
    {
        _currentInteractable = interactable;
        _currentInteractableTransform = interactableTransform;
        if (interactableHighlightPresenter != null)
            interactableHighlightPresenter.Show(interactableTransform);
    }

    private void ClearCurrentInteractable()
    {
        if (interactableHighlightPresenter != null)
            interactableHighlightPresenter.Hide();

        _currentInteractable = null;
        _currentInteractableTransform = null;
    }

    private bool IsInteractableAvailable(IInteractable interactable)
    {
        if (interactable == null)
            return false;

        Object interactableObject = interactable as Object;
        if (interactableObject == null)
            return false;

        return interactable.CanInteract;
    }

    private bool IsAttackableAvailable(IAttackable attackable)
    {
        if (attackable == null)
            return false;

        Object attackableObject = attackable as Object;
        if (attackableObject == null)
            return false;

        return attackable.CanBeAttacked;
    }

    private void SetAttackRangeState(IAttackable attackable, bool isInRange)
    {
        if (attackable is IAttackRangeAware rangeAware)
            rangeAware.SetInAttackRange(isInRange);
    }

    private void ResolvePlayerServices()
    {
        if (_playerContext == null || !_playerContext.TryGet(out _inventory))
            return;

        _feedback = _playerContext.IsLocalPlayer
            ? ProjectContext.Get<IGameplayFeedback>()
            : null;
    }

    private void HandlePlayerContextInitialized(PlayerContext playerContext)
    {
        ResolvePlayerServices();
    }

    private void OnDestroy()
    {
        if (_playerContext != null)
            _playerContext.OnInitialized -= HandlePlayerContextInitialized;
    }
}
