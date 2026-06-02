using System;
using UnityEngine;
using UnityEngine.EventSystems;
public class Dragger : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public event Action<Vector3> OnReleasedObject;
    public event Action OnGrabbedObject;
    public event Action OnClickObject;

    private Vector3 _offset;

    private bool _isActivate = false;
    private bool _isDragged = false;

    public void SetDraggerActive(bool isActive) => _isActivate = isActive;

    public void OnPointerDown(PointerEventData eventData)
    {
        _offset = transform.position - Input.mousePosition;
        _isDragged = false;
        if(_isActivate)
            OnGrabbedObject?.Invoke();
        else
            OnClickObject?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isActivate)
            return;

        if (_isDragged)
            OnReleasedObject?.Invoke(transform.position);
        else
            OnClickObject?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isActivate)
            return;
        _isDragged = true;
        transform.position = Input.mousePosition + _offset;
    }

    private void OnDestroy()
    {
        OnReleasedObject = null;
        OnGrabbedObject = null;
        OnClickObject = null;
    }
}
