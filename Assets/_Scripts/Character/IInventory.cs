using System;
using System.Collections.Generic;

public interface IInventory
{
    event Action<int, InventoryItem> OnSlotChanged;
    event Action OnInventoryChanged;

    IReadOnlyList<InventoryItem> InventoryItems { get; }
    int Capacity { get; }

    bool CanAddItems(ItemObject itemObject, int count = 1);
    bool CanAddItemsAfterRemoving(ItemObject itemObject, int count, IEnumerable<InventoryItem> itemsToRemove);
    bool TryAddItems(ItemObject itemObject, int count = 1);
    bool HasItems(ItemObject itemObject, int count = 1);
    bool TryRemoveItems(ItemObject itemObject, int count = 1);
    int CountItems(ItemObject itemObject);
    void SwapItems(int firstIndex, int secondIndex);
}
