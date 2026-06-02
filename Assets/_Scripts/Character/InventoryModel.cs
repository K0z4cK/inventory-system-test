using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryModel : IInventory
{
    public event Action<int, InventoryItem> OnSlotChanged;
    public event Action OnInventoryChanged;

    private readonly InventoryItem[] _inventoryItems;
    private readonly int _maxItemsInCell;

    public InventoryItem[] InventoryItems => _inventoryItems;
    public int Capacity => _inventoryItems.Length;

    public InventoryModel(int maxCells, int maxItemsInCell)
    {
        _inventoryItems = new InventoryItem[Mathf.Max(0, maxCells)];
        _maxItemsInCell = Mathf.Max(1, maxItemsInCell);
    }

    public bool CanAddItems(ItemObject itemObject, int count = 1)
    {
        if (!IsValidItemRequest(itemObject, count))
            return false;

        return GetAvailableSpace(itemObject) >= count;
    }

    public bool CanAddItemsAfterRemoving(ItemObject itemObject, int count, IEnumerable<InventoryItem> itemsToRemove)
    {
        if (!IsValidItemRequest(itemObject, count))
            return false;

        InventoryModel simulation = Clone();
        if (itemsToRemove != null)
        {
            foreach (InventoryItem item in itemsToRemove)
            {
                if (!item.IsEmpty && !simulation.TryRemoveItems(item.ItemObject, item.Count))
                    return false;
            }
        }

        return simulation.CanAddItems(itemObject, count);
    }

    public bool TryAddItems(ItemObject itemObject, int count = 1)
    {
        if (!CanAddItems(itemObject, count))
            return false;

        int remainingCount = count;
        remainingCount = AddToExistingStacks(itemObject, remainingCount);
        AddToEmptySlots(itemObject, remainingCount);

        RaiseInventoryChanged();
        return true;
    }

    public bool HasItems(ItemObject itemObject, int count = 1)
    {
        if (!IsValidItemRequest(itemObject, count))
            return false;

        return CountItems(itemObject) >= count;
    }

    public bool TryRemoveItems(ItemObject itemObject, int count = 1)
    {
        if (!HasItems(itemObject, count))
            return false;

        int remainingCount = count;

        for (int i = 0; i < _inventoryItems.Length && remainingCount > 0; i++)
        {
            if (!_inventoryItems[i].Matches(itemObject))
                continue;

            int countToRemove = Mathf.Min(_inventoryItems[i].Count, remainingCount);
            _inventoryItems[i].Count -= countToRemove;
            remainingCount -= countToRemove;

            if (_inventoryItems[i].Count <= 0)
                _inventoryItems[i].Clear();

            RaiseSlotChanged(i);
        }

        RaiseInventoryChanged();
        return true;
    }

    public int CountItems(ItemObject itemObject)
    {
        if (itemObject == null)
            return 0;

        int count = 0;
        foreach (InventoryItem item in _inventoryItems)
        {
            if (item.Matches(itemObject))
                count += item.Count;
        }

        return count;
    }

    public void SwapItems(int firstIndex, int secondIndex)
    {
        if (!IsValidIndex(firstIndex) || !IsValidIndex(secondIndex) || firstIndex == secondIndex)
            return;

        (_inventoryItems[firstIndex], _inventoryItems[secondIndex]) = (_inventoryItems[secondIndex], _inventoryItems[firstIndex]);

        RaiseSlotChanged(firstIndex);
        RaiseSlotChanged(secondIndex);
        RaiseInventoryChanged();
    }

    private int AddToExistingStacks(ItemObject itemObject, int count)
    {
        int remainingCount = count;

        for (int i = 0; i < _inventoryItems.Length && remainingCount > 0; i++)
        {
            if (!_inventoryItems[i].Matches(itemObject) || _inventoryItems[i].Count >= _maxItemsInCell)
                continue;

            int countToAdd = Mathf.Min(_maxItemsInCell - _inventoryItems[i].Count, remainingCount);
            _inventoryItems[i].Count += countToAdd;
            remainingCount -= countToAdd;
            RaiseSlotChanged(i);
        }

        return remainingCount;
    }

    private void AddToEmptySlots(ItemObject itemObject, int count)
    {
        int remainingCount = count;

        for (int i = 0; i < _inventoryItems.Length && remainingCount > 0; i++)
        {
            if (!_inventoryItems[i].IsEmpty)
                continue;

            int countToAdd = Mathf.Min(_maxItemsInCell, remainingCount);
            _inventoryItems[i] = new InventoryItem(itemObject, countToAdd);
            remainingCount -= countToAdd;
            RaiseSlotChanged(i);
        }
    }

    private int GetAvailableSpace(ItemObject itemObject)
    {
        int space = 0;

        foreach (InventoryItem item in _inventoryItems)
        {
            if (item.IsEmpty)
            {
                space += _maxItemsInCell;
                continue;
            }

            if (item.Matches(itemObject))
                space += _maxItemsInCell - item.Count;
        }

        return space;
    }

    private bool IsValidItemRequest(ItemObject itemObject, int count) => itemObject != null && count > 0;

    private bool IsValidIndex(int index) => index >= 0 && index < _inventoryItems.Length;

    private void RaiseSlotChanged(int index) => OnSlotChanged?.Invoke(index, _inventoryItems[index]);

    private void RaiseInventoryChanged() => OnInventoryChanged?.Invoke();

    private InventoryModel Clone()
    {
        InventoryModel clone = new InventoryModel(_inventoryItems.Length, _maxItemsInCell);
        Array.Copy(_inventoryItems, clone._inventoryItems, _inventoryItems.Length);
        return clone;
    }
}

[Serializable]
public struct InventoryItem
{
    public ItemObject ItemObject;
    public int Count;

    public InventoryItem(ItemObject itemObject, int count)
    {
        ItemObject = itemObject;
        Count = count;
    }

    public bool IsEmpty => ItemObject == null || Count <= 0;

    public bool Matches(ItemObject itemObject)
    {
        if (ItemObject == null || itemObject == null)
            return false;

        return !string.IsNullOrWhiteSpace(ItemObject.ItemId)
            ? ItemObject.ItemId == itemObject.ItemId
            : ItemObject == itemObject;
    }

    public void Clear()
    {
        ItemObject = null;
        Count = 0;
    }
}
