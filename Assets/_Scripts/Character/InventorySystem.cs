using System;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    [SerializeField] private ItemsHolder _itemsHolder;

    [SerializeField] private int _maxCells;
    [SerializeField] private int _maxItemsInCell;

    private InventoryItem[] _inventoryItems;
    public InventoryItem[] InventoryItems => _inventoryItems;

    private void Awake()
    {
        _inventoryItems = new InventoryItem[_maxCells];

        CraftManager.Instance.SubscribeOnItemCrafted(OnItemCrafted);
        UIManager.Instance.InventoryPanel.OnSwapCellItems += SwapItems;
        UIManager.Instance.InventoryPanel.OnCellItemClick += SelectItemInHolder;
    }

    public void AddItems(IPickable pickable, ItemObject itemObject, int count = 1)
    {
        int index = -1;
        bool isCountChanged = false;

        if(CheckSameItem(itemObject, ref index))
        {
            if (IsCellHaveSpace(index, count))
            {
                AddItemsToCell(index, count);
                pickable.DestroyObject();
                return;
            }
            else
            {
                AddItemsToCell(index, _maxItemsInCell - _inventoryItems[index].Count);
                count = GetCountRest(index, count);
                isCountChanged = true; 
            }
        }

        index = GetFirstEmptyCellIndex();
        if (index > -1)
        {
            SetItemToCell(index, itemObject, count);
            pickable.DestroyObject();
            return;
        }

        if(isCountChanged)
        {
            pickable.DestroyObject();
        }

        Debug.Log("Inventory Full");
    }

    public bool TryAddItems(ItemObject itemObject, int count = 1)
    {
        int index = -1;
        if (CheckSameItem(itemObject, ref index))
        {
            if (IsCellHaveSpace(index, count))
            {
                AddItemsToCell(index, count);
                return true;
            }
            else
            {
                AddItemsToCell(index, _maxItemsInCell - _inventoryItems[index].Count);
                count = GetCountRest(index, count);
            }
        }

        index = GetFirstEmptyCellIndex();
        if (index > -1)
        {
            SetItemToCell(index, itemObject, count);
            return true;
        }

        Debug.Log("Inventory Full");
        return false;
    }

    private void SetItemToCell(int index, ItemObject itemObject, int count)
    {
        _inventoryItems[index].ItemObject = itemObject;
        AddItemsToCell(index, count);
    }

    private void AddItemsToCell(int index, int count)
    {
        _inventoryItems[index].Count += count;
        UIManager.Instance.InventoryPanel.SetItemToCell(index, _inventoryItems[index]);
    }

    private int GetCountRest(int index, int count) => _inventoryItems[index].Count + count - _maxItemsInCell;

    private bool IsCellHaveSpace(int index, int count) => _inventoryItems[index].Count + count <= _maxItemsInCell;

    private bool CheckSameItem(ItemObject itemObject, ref int index)
    {
        for(int i = 0; i < _inventoryItems.Length; i++)
        {
            if ( _inventoryItems[i].ItemObject == itemObject && _inventoryItems[i].Count < _maxItemsInCell)
            {
                index = i;
                return true;
            }
        }
        return false;
    }

    private int GetFirstEmptyCellIndex()
    {
        for (int i = 0; i < _inventoryItems.Length; i++)
        {
            if (_inventoryItems[i].Count == 0)
                return i;
        }
        return -1;
    }

    public void OnItemCrafted(ItemCraftStruct itemCraft)
    {
        if (TryAddItems(itemCraft.ItemResult.ItemObject, itemCraft.ItemResult.Count))
        {
            foreach (InventoryItem item in itemCraft.CraftRecipe)
            {
                RemoveItems(item.ItemObject, item.Count);
            }

            Debug.Log("item crafted: " + itemCraft.ItemResult.ItemObject.Name);
        }
        else
            Debug.Log("item craft failed: " + itemCraft.ItemResult.ItemObject.Name);
    }

    public void RemoveItems(ItemObject item, int count = 1)
    {
        for(int i = 0; i < _inventoryItems.Length; i++)
        {
            if (_inventoryItems[i].ItemObject == item)
            {
                RemoveItems(i, ref count);
                if(count > 0)
                {
                    RemoveItems(item, count);
                }
                return;
            }
        }
    }

    private void RemoveItems(int index, ref int count)
    {
        if (_inventoryItems[index].Count > 0)
        {
            if (_inventoryItems[index].Count - count <= 0)
            {
                _inventoryItems[index].Count -= count;
                count = _inventoryItems[index].Count * -1;
                ClearCell(index);
                UIManager.Instance.InventoryPanel.ClearCell(index);
            }
            else
            {
                _inventoryItems[index].Count -= count;
                count = 0;
                UIManager.Instance.InventoryPanel.SetItemToCell(index, _inventoryItems[index]);
            }
            
        }
    }

    private void ClearCell(int index)
    {
        _inventoryItems[index].Count = 0;
        _inventoryItems[index].ItemObject = null;
    }

    public void SwapItems(int firstIndex, int secondIndex)
    {
        var temp = _inventoryItems[firstIndex];
        _inventoryItems[firstIndex] = _inventoryItems[secondIndex];
        _inventoryItems[secondIndex] = temp;

        UpdateCellUI(firstIndex);
        UpdateCellUI(secondIndex);
    }

    private void SelectItemInHolder(int index)
    {
        _itemsHolder.SetNewItem(_inventoryItems[index].ItemObject.Type);
    }

    private void UpdateCellUI(int index)
    {
        if (_inventoryItems[index].ItemObject != null)
            UIManager.Instance.InventoryPanel.SetItemToCell(index, _inventoryItems[index]);
        else
            UIManager.Instance.InventoryPanel.ClearCell(index);
    }
}

[Serializable]
public struct InventoryItem
{
    public ItemObject ItemObject;
    public int Count;
}