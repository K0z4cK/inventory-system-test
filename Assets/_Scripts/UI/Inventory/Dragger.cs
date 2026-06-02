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

    public void SetDraggerActive(bool isActive) => _isActivate = isActive;

    public void OnPointerDown(PointerEventData eventData)
    {
        _offset = transform.position - Input.mousePosition;
        if(_isActivate)
            OnGrabbedObject?.Invoke();
        else
            OnClickObject?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isActivate)
            return;

        OnReleasedObject?.Invoke(transform.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isActivate)
            return;
        transform.position = Input.mousePosition + _offset;
    }

    private void OnDestroy()
    {
        OnReleasedObject = null;
        OnGrabbedObject = null;
    }
}

