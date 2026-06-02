using System;
using UnityEngine;

[RequireComponent(typeof(Dragger))]
public class DraggableItemUI : ItemUI
{
    private Dragger _dragger;

    private void Awake()
    {
        _dragger = GetComponent<Dragger>();
    }

    public void SetDraggerActive(bool isActive) => _dragger.SetDraggerActive(isActive);

    public void SubscribeOnDragger(Action<Vector3> onChangePosition) => _dragger.OnReleasedObject += onChangePosition;
    public void UnsubscribeOnDragger(Action<Vector3> onChangePosition) => _dragger.OnReleasedObject -= onChangePosition;

    public void SubscribeOnClick(Action onClick) => _dragger.OnClickObject += onClick;
    public void UnsubscribeOnClick(Action onClick) => _dragger.OnClickObject -= onClick;
}
