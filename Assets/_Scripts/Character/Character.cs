using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(CharacterController))]
public class Character : MonoBehaviour, IControllable
{
    [FormerlySerializedAs("_animator")]
    [SerializeField] private Animator animator;
    [FormerlySerializedAs("_speed")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private InteractableHighlightPresenter interactableHighlightPresenter;
    [FormerlySerializedAs("attackDamage")]
    [SerializeField, Min(1)] private int baseAttackDamage = 1;
    [SerializeField, Min(0.1f)] private float attackRange = 2.5f;

    private CharacterController _characterController;
    private InventorySystem _inventorySystem;
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
        _inventorySystem = GetComponent<InventorySystem>();
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
            _currentAttackable.ReceiveAttack(this, GetAttackDamage());
            RefreshAttackTarget();
        }

        animator.SetTrigger("Attack");
        Debug.Log("Action");
    }

    public void Interact()
    {
        if (!IsInteractableAvailable(_currentInteractable))
        {
            GetInteractableFromQueue();
            return;
        }

        if (!_currentInteractable.Interact(this))
            return;

        if (!IsInteractableAvailable(_currentInteractable))
            GetInteractableFromQueue();
        else if (interactableHighlightPresenter != null)
            interactableHighlightPresenter.Show(_currentInteractableTransform);

        animator.SetTrigger("Gather");
        Debug.Log("Interact");
    }

    public bool TryAddItemsToInventory(ItemObject itemObject, int count)
    {
        return _inventorySystem != null && _inventorySystem.AddItems(itemObject, count);
    }

    public bool HasSelectedItem(ItemObject itemObject)
    {
        if (_inventorySystem == null || itemObject == null || _inventorySystem.SelectedItem == null)
            return false;

        return new InventoryItem(_inventorySystem.SelectedItem, 1).Matches(itemObject);
    }

    public int GetAttackDamage()
    {
        ItemObject selectedItem = _inventorySystem != null ? _inventorySystem.SelectedItem : null;
        return selectedItem != null ? selectedItem.AttackDamage : Mathf.Max(1, baseAttackDamage);
    }

    public void Move(Vector2 direction)
    {
        Vector3 scaledMovement = new Vector3(direction.x, 0f, direction.y) * speed * Time.fixedDeltaTime;

        _transform.LookAt(_transform.position + scaledMovement, Vector3.up);
        _characterController.Move(scaledMovement);
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
            Debug.Log("Can interact: " + other.name);
            return;
        }

        _interactableQueue.Add(other.transform);
        Debug.Log("Added to Queue: " + other.name);
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
            Debug.Log("Get form Queue: " + _currentInteractableTransform.name);
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
}
