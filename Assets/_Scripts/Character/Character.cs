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

    private CharacterController _characterController;
    private InventorySystem _inventorySystem;
    private Transform _transform;

    private IInteractable _currentInteractable;
    private Transform _currentInteractableTransform;

    private List<Transform> _interactableQueue = new List<Transform>();

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _inventorySystem = GetComponent<InventorySystem>();
        _transform = transform;
    }

    public void Action()
    {
        Debug.Log("Action");
        animator.SetTrigger("Attack");
    }

    public void Interact()
    {
        if (_currentInteractable == null)
            return;

        if (!_currentInteractable.Interact(this))
            return;

        GetInteractableFromQueue();
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

    public void Move(Vector2 direction)
    {
        Vector3 scaledMovement = new Vector3(direction.x, 0f, direction.y) * speed * Time.fixedDeltaTime;

        _transform.LookAt(_transform.position + scaledMovement, Vector3.up);
        _characterController.Move(scaledMovement);
        animator.SetFloat("Velocity", _characterController.velocity.magnitude);
    }

    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable == null)
            return;

        if (_currentInteractable == null)
        {
            _currentInteractable = interactable;
            _currentInteractableTransform = other.transform;
            Debug.Log("Can interact: " + other.name);
            return;
        }

        _interactableQueue.Add(other.transform);
        Debug.Log("Added to Queue: " + other.name);
    }

    private void OnTriggerExit(Collider other)
    {
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

    private void GetInteractableFromQueue()
    {
        _currentInteractable = null;
        _currentInteractableTransform = null;

        if (_interactableQueue.Count == 0)
            return;

        _currentInteractableTransform = _interactableQueue[0];
        _currentInteractable = _currentInteractableTransform.GetComponent<IInteractable>();
        if (_currentInteractable == null)
        {
            _interactableQueue.RemoveAt(0);
            GetInteractableFromQueue();
            return;
        }

        _interactableQueue.RemoveAt(0);
    }
}
