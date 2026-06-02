using System;

public interface IPickable
{
    public event Action<IPickable, ItemObject, int> OnItemPickUp;
    public void SubscribeOnItemPickUp(Action<IPickable, ItemObject, int> onItemPickUp);
    public void UnsubscribeOnItemPickUp(Action<IPickable, ItemObject, int> onItemPickUp);
    public void PickUp();
    public void DestroyObject();
}