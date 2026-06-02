using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform _target;   
    [SerializeField] private float _smoothTime = 0.15f;

    private Vector3 _cameraOffest;
    private Vector3 _velocity = Vector3.zero;
    private Vector3 _targetPosition;

    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
        _cameraOffest = new Vector3(0f, _transform.position.y, -5f); 
    }

    private void FixedUpdate()
    {
        _targetPosition = _target.position + _cameraOffest;
        _transform.position = Vector3.SmoothDamp(_transform.position, _targetPosition, ref _velocity, _smoothTime);
    }
}
