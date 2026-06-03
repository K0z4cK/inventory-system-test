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

    private IPickable _currentPickableItem;
    private Transform _currentPickableItemTransform;

    private List<Transform> _pickableQueue = new List<Transform>();

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
        if (_currentPickableItem == null)
            return;

        if (!_currentPickableItem.PickUp())
            return;

        GetPickableFromQueue();
        animator.SetTrigger("Gather");
        Debug.Log("Interact");
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
        if (other.CompareTag("Pickable")) 
        {
            if (_currentPickableItem == null)
            {
                _currentPickableItem = other.GetComponent<IPickable>();
                if (_currentPickableItem == null)
                    return;

                _currentPickableItemTransform = other.transform;
                if (_inventorySystem != null)
                    _currentPickableItem.SubscribeOnItemPickUp(_inventorySystem.AddItems);

                Debug.Log("Can pick up: " + other.name);
                return;
            }
            _pickableQueue.Add(other.transform);
            Debug.Log("Added to Queue: " + other.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform == _currentPickableItemTransform)
        {
            if (_currentPickableItem != null && _inventorySystem != null)
                _currentPickableItem.UnsubscribeOnItemPickUp(_inventorySystem.AddItems);

            Debug.Log("Get form Queue: " + _currentPickableItemTransform.name);
            GetPickableFromQueue();           
        }
        else if(_pickableQueue.Contains(other.transform))
        {
            _pickableQueue.Remove(other.transform);
        }
    }

    private void GetPickableFromQueue()
    {
        _currentPickableItem = null;
        _currentPickableItemTransform = null;

        if (_pickableQueue.Count == 0)
            return;

        _currentPickableItemTransform = _pickableQueue[0];
        _currentPickableItem = _currentPickableItemTransform.GetComponent<IPickable>();
        if (_currentPickableItem == null)
        {
            _pickableQueue.RemoveAt(0);
            GetPickableFromQueue();
            return;
        }

        if (_inventorySystem != null)
            _currentPickableItem.SubscribeOnItemPickUp(_inventorySystem.AddItems);

        _pickableQueue.RemoveAt(0);
    }
}
