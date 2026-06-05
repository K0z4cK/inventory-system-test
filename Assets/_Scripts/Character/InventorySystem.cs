using System;
using System.Collections.Generic;

[Obsolete("InventorySystem is a scene compatibility adapter. Use PlayerContext and IInventoryService.")]
public class InventorySystem : PlayerContext, IInventory, IInventorySlotSelector
{
    private IInventoryService Inventory => Get<IInventoryService>();

    public event Action<int, InventoryItem> OnSlotChanged
    {
        add => Inventory.OnSlotChanged += value;
        remove => Inventory.OnSlotChanged -= value;
    }

    public event Action OnInventoryChanged
    {
        add => Inventory.OnInventoryChanged += value;
        remove => Inventory.OnInventoryChanged -= value;
    }

    public event Action<ItemObject> OnSelectedItemChanged
    {
        add => Inventory.OnSelectedItemChanged += value;
        remove => Inventory.OnSelectedItemChanged -= value;
    }

    public IReadOnlyList<InventoryItem> InventoryItems => Inventory.InventoryItems;
    public int Capacity => Inventory.Capacity;
    public int SelectedSlotIndex => Inventory.SelectedSlotIndex;
    public ItemObject SelectedItem => Inventory.SelectedItem;

    public bool CanAddItems(ItemObject itemObject, int count = 1) => Inventory.CanAddItems(itemObject, count);

    public bool CanAddItemsAfterRemoving(
        ItemObject itemObject,
        int count,
        IEnumerable<InventoryItem> itemsToRemove)
    {
        return Inventory.CanAddItemsAfterRemoving(itemObject, count, itemsToRemove);
    }

    public bool TryAddItems(ItemObject itemObject, int count = 1) => Inventory.TryAddItems(itemObject, count);
    public bool HasItems(ItemObject itemObject, int count = 1) => Inventory.HasItems(itemObject, count);
    public bool TryRemoveItems(ItemObject itemObject, int count = 1) => Inventory.TryRemoveItems(itemObject, count);
    public bool TryRemoveSlot(int slotIndex, out InventoryItem removedItem) => Inventory.TryRemoveSlot(slotIndex, out removedItem);
    public int CountItems(ItemObject itemObject) => Inventory.CountItems(itemObject);
    public void SwapItems(int firstIndex, int secondIndex) => Inventory.SwapItems(firstIndex, secondIndex);
    public void SelectSlot(int index) => Inventory.SelectSlot(index);

}
