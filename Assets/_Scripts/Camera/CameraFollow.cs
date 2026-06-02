using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [FormerlySerializedAs("_target")]
    [SerializeField] private Transform target;   
    [FormerlySerializedAs("_smoothTime")]
    [SerializeField] private float smoothTime = 0.15f;

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
        if (target == null)
            return;

        _targetPosition = target.position + _cameraOffest;
        _transform.position = Vector3.SmoothDamp(_transform.position, _targetPosition, ref _velocity, smoothTime);
    }
}
