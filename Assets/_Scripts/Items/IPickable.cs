using System;

public interface IPickable
{
    public event Func<IPickable, ItemObject, int, bool> OnItemPickUp;
    public void SubscribeOnItemPickUp(Func<IPickable, ItemObject, int, bool> onItemPickUp);
    public void UnsubscribeOnItemPickUp(Func<IPickable, ItemObject, int, bool> onItemPickUp);
    public bool PickUp();
    public void DestroyObject();
}
