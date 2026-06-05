using System;

public interface IInventorySlotSelector
{
    event Action<ItemObject> OnSelectedItemChanged;

    int SelectedSlotIndex { get; }
    ItemObject SelectedItem { get; }
    void SelectSlot(int index);
}
