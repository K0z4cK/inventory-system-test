using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

public class InventorySystem : MonoBehaviour, IInventory, IInventorySlotSelector
{
    [FormerlySerializedAs("_itemsHolder")]
    [SerializeField] private ItemsHolder itemsHolder;

    [FormerlySerializedAs("_maxCells")]
    [SerializeField] private int maxCells;
    [FormerlySerializedAs("_maxItemsInCell")]
    [SerializeField] private int maxItemsInCell;

    private InventoryModel _model;
    private IGameplayFeedback _feedback;
    private int _selectedSlotIndex = -1;

    public event System.Action<int, InventoryItem> OnSlotChanged
    {
        add
        {
            EnsureModel();
            _model.OnSlotChanged += value;
        }
        remove
        {
            EnsureModel();
            _model.OnSlotChanged -= value;
        }
    }

    public event System.Action OnInventoryChanged
    {
        add
        {
            EnsureModel();
            _model.OnInventoryChanged += value;
        }
        remove
        {
            EnsureModel();
            _model.OnInventoryChanged -= value;
        }
    }

    public IReadOnlyList<InventoryItem> InventoryItems
    {
        get
        {
            EnsureModel();
            return _model.InventoryItems;
        }
    }

    public int Capacity
    {
        get
        {
            EnsureModel();
            return _model.Capacity;
        }
    }

    public ItemObject SelectedItem
    {
        get
        {
            EnsureModel();
            if (_selectedSlotIndex < 0 || _selectedSlotIndex >= _model.InventoryItems.Count)
                return null;

            InventoryItem selectedItem = _model.InventoryItems[_selectedSlotIndex];
            return selectedItem.IsEmpty ? null : selectedItem.ItemObject;
        }
    }

    private void Awake()
    {
        ResolveItemsHolder();

        EnsureModel();
    }

    public void Initialize(IGameplayFeedback feedback)
    {
        _feedback = feedback;
    }

    public bool AddItems(ItemObject itemObject, int count = 1)
    {
        if (TryAddItems(itemObject, count))
        {
            _feedback?.ShowPickedUp(itemObject, count);
            return true;
        }

        _feedback?.ShowInventoryFull(itemObject, count);
        Debug.Log("Inventory Full");
        return false;
    }

    public bool CanAddItems(ItemObject itemObject, int count = 1)
    {
        EnsureModel();
        return _model.CanAddItems(itemObject, count);
    }

    public bool CanAddItemsAfterRemoving(ItemObject itemObject, int count, IEnumerable<InventoryItem> itemsToRemove)
    {
        EnsureModel();
        return _model.CanAddItemsAfterRemoving(itemObject, count, itemsToRemove);
    }

    public bool TryAddItems(ItemObject itemObject, int count = 1)
    {
        EnsureModel();
        return _model.TryAddItems(itemObject, count);
    }

    public bool HasItems(ItemObject itemObject, int count = 1)
    {
        EnsureModel();
        return _model.HasItems(itemObject, count);
    }

    public bool TryRemoveItems(ItemObject itemObject, int count = 1)
    {
        EnsureModel();
        return _model.TryRemoveItems(itemObject, count);
    }

    public int CountItems(ItemObject itemObject)
    {
        EnsureModel();
        return _model.CountItems(itemObject);
    }

    public void SwapItems(int firstIndex, int secondIndex)
    {
        EnsureModel();
        _model.SwapItems(firstIndex, secondIndex);
    }

    public void SelectSlot(int index)
    {
        IReadOnlyList<InventoryItem> inventoryItems = InventoryItems;
        if (index < 0 || index >= inventoryItems.Count || inventoryItems[index].IsEmpty)
            return;

        _selectedSlotIndex = index;

        ResolveItemsHolder();
        if (itemsHolder == null)
        {
            Debug.LogWarning("InventorySystem cannot equip selected item because ItemsHolder was not found.");
            return;
        }

        itemsHolder?.SetNewItem(inventoryItems[index].ItemObject);
    }

    private void ResolveItemsHolder()
    {
        if (itemsHolder == null)
            itemsHolder = GetComponentInChildren<ItemsHolder>(true);
    }

    private void EnsureModel()
    {
        if (_model != null)
            return;

        _model = new InventoryModel(maxCells, maxItemsInCell);
    }
}
