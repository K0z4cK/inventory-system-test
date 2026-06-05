using System;
using System.Collections.Generic;

public sealed class InventoryService : IInventoryService, IDisposable
{
    private readonly InventoryModel _model;
    private readonly IGameplayFeedback _feedback;

    private int _selectedSlotIndex = -1;
    private ItemObject _lastSelectedItem;

    public event Action<int, InventoryItem> OnSlotChanged
    {
        add => _model.OnSlotChanged += value;
        remove => _model.OnSlotChanged -= value;
    }

    public event Action OnInventoryChanged
    {
        add => _model.OnInventoryChanged += value;
        remove => _model.OnInventoryChanged -= value;
    }

    public event Action<ItemObject> OnSelectedItemChanged;

    public IReadOnlyList<InventoryItem> InventoryItems => _model.InventoryItems;
    public int Capacity => _model.Capacity;
    public int SelectedSlotIndex => _selectedSlotIndex;

    public ItemObject SelectedItem
    {
        get
        {
            if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _model.InventoryItems.Count)
                return null;

            InventoryItem selectedItem = _model.InventoryItems[_selectedSlotIndex];
            return selectedItem.IsEmpty ? null : selectedItem.ItemObject;
        }
    }

    public InventoryService(int maxCells, int maxItemsInCell, IGameplayFeedback feedback = null)
        : this(new InventoryModel(maxCells, maxItemsInCell), feedback)
    {
    }

    public InventoryService(InventoryModel model, IGameplayFeedback feedback = null)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _feedback = feedback;
        _model.OnInventoryChanged += RefreshSelectedItem;
    }

    public bool CanAddItems(ItemObject itemObject, int count = 1)
    {
        return _model.CanAddItems(itemObject, count);
    }

    public bool CanAddItemsAfterRemoving(
        ItemObject itemObject,
        int count,
        IEnumerable<InventoryItem> itemsToRemove)
    {
        return _model.CanAddItemsAfterRemoving(itemObject, count, itemsToRemove);
    }

    public bool TryAddItems(ItemObject itemObject, int count = 1)
    {
        return _model.TryAddItems(itemObject, count);
    }

    public bool HasItems(ItemObject itemObject, int count = 1)
    {
        return _model.HasItems(itemObject, count);
    }

    public bool TryRemoveItems(ItemObject itemObject, int count = 1)
    {
        return _model.TryRemoveItems(itemObject, count);
    }

    public bool TryRemoveSlot(int slotIndex, out InventoryItem removedItem)
    {
        return _model.TryRemoveSlot(slotIndex, out removedItem);
    }

    public int CountItems(ItemObject itemObject)
    {
        return _model.CountItems(itemObject);
    }

    public void SwapItems(int firstIndex, int secondIndex)
    {
        _model.SwapItems(firstIndex, secondIndex);
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= InventoryItems.Count || InventoryItems[index].IsEmpty)
        {
            _feedback?.ShowInvalidInventorySelection();
            return;
        }

        _selectedSlotIndex = index;
        RefreshSelectedItem();
        _feedback?.ShowItemSelected(SelectedItem);
    }

    public void Dispose()
    {
        _model.OnInventoryChanged -= RefreshSelectedItem;
    }

    private void RefreshSelectedItem()
    {
        ItemObject selectedItem = SelectedItem;
        if (ReferenceEquals(_lastSelectedItem, selectedItem))
            return;

        _lastSelectedItem = selectedItem;
        OnSelectedItemChanged?.Invoke(selectedItem);
    }
}
