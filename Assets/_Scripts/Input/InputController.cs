using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    private IControllable _controllable;
    private InputSystem_Actions _inputSystem;

    private void Awake()
    {
        _inputSystem = new InputSystem_Actions();
        _inputSystem.Enable();

        _controllable = GetComponent<IControllable>();
        if (_controllable == null)
        {
            throw new System.Exception($"{gameObject.name} don't have IControllable");
        }
    }

    private void OnEnable()
    {
        _inputSystem.Player.Attack.performed += OnAttackPerformed;
        _inputSystem.Player.Interact.performed += OnInteractPerformed;
    }

    private void OnInteractPerformed(InputAction.CallbackContext context) => _controllable.Interact();

    private void OnAttackPerformed(InputAction.CallbackContext context) => _controllable.Action();

    private void PerformMove() => _controllable.Move(_inputSystem.Player.Move.ReadValue<Vector2>());

    private void FixedUpdate()
    {
        PerformMove();
    }

    private void OnDisable()
    {
        _inputSystem.Player.Attack.performed -= OnAttackPerformed;
        _inputSystem.Player.Interact.performed -= OnInteractPerformed;
    }
}
