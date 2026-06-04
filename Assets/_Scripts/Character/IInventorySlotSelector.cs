public interface IInventorySlotSelector
{
    ItemObject SelectedItem { get; }
    void SelectSlot(int index);
}
