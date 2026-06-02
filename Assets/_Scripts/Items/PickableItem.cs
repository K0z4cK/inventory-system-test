using System;
using UnityEngine;

public class PickableItem : BaseItem, IPickable
{
    [Range(1, 10)][SerializeField] private int _pickUpCount = 1;

    public event Action<IPickable, ItemObject, int> OnItemPickUp;

    public void SubscribeOnItemPickUp(Action<IPickable, ItemObject, int> onItemPickUp) => OnItemPickUp += onItemPickUp;

    public void UnsubscribeOnItemPickUp(Action<IPickable, ItemObject, int> onItemPickUp) => OnItemPickUp -= onItemPickUp;

    public void PickUp()
    {
        Debug.Log("Picked up: " + name);
        OnItemPickUp?.Invoke(this, _itemObject, _pickUpCount);
    }

    public void DestroyObject() => Destroy(gameObject);

    private void OnDestroy()
    {
        OnItemPickUp = null;
    }
}


