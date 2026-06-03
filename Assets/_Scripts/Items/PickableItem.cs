using System;
using UnityEngine;
using UnityEngine.Serialization;

public class PickableItem : BaseItem, IPickable
{
    [Range(1, 10)]
    [FormerlySerializedAs("_pickUpCount")]
    [SerializeField] private int pickUpCount = 1;

    public event Func<IPickable, ItemObject, int, bool> OnItemPickUp;

    public void SubscribeOnItemPickUp(Func<IPickable, ItemObject, int, bool> onItemPickUp) => OnItemPickUp += onItemPickUp;

    public void UnsubscribeOnItemPickUp(Func<IPickable, ItemObject, int, bool> onItemPickUp) => OnItemPickUp -= onItemPickUp;

    public bool PickUp()
    {
        if (OnItemPickUp == null)
            return false;

        bool pickedUp = false;
        foreach (Func<IPickable, ItemObject, int, bool> listener in OnItemPickUp.GetInvocationList())
        {
            pickedUp |= listener.Invoke(this, _itemObject, pickUpCount);
        }

        if (pickedUp)
            Debug.Log("Picked up: " + name);

        return pickedUp;
    }

    public void DestroyObject() => Destroy(gameObject);

    private void OnDestroy()
    {
        OnItemPickUp = null;
    }
}
