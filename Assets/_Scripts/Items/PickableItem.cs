using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PickableItem : BaseItem, IPickable
{
    [Range(1, 10)]
    [FormerlySerializedAs("_pickUpCount")]
    [SerializeField] private int pickUpCount = 1;

    public event Action<IPickable, ItemObject, int> OnItemPickUp;

    public void SubscribeOnItemPickUp(Action<IPickable, ItemObject, int> onItemPickUp) => OnItemPickUp += onItemPickUp;

    public void UnsubscribeOnItemPickUp(Action<IPickable, ItemObject, int> onItemPickUp) => OnItemPickUp -= onItemPickUp;

    public void PickUp()
    {
        Debug.Log("Picked up: " + name);
        OnItemPickUp?.Invoke(this, _itemObject, pickUpCount);
    }

    public void DestroyObject() => Destroy(gameObject);

    private void OnDestroy()
    {
        OnItemPickUp = null;
    }
}

