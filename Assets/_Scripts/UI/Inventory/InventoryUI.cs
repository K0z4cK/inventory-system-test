using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class InventoryUI : BasePanelUI
{
    public event Action<int, int> OnSwapCellItems;
    public event Action<int> OnCellItemClick;

    [Header("Prefabs")]
    [SerializeField] private DraggableItemUI _itemPrefab;

    [Header("Transforms of cells")]
    [SerializeField] private Transform _cellsGrid;
    [SerializeField] private Transform _cellsHotbar;

    [Header("Hotbar")]
    [SerializeField] private GameObject _hudHotbar;
    [SerializeField] private Transform _hudHotbarPosition;
    [SerializeField] private Transform _inventoryHotbarPosition;

    private List<InventoryCellUI> _inventoryCells = new List<InventoryCellUI>();
    private InventoryCellUI _selectedCell;

    private ObjectPool<DraggableItemUI> _itemsPool;

    private void Awake()
    {
        _itemsPool = new ObjectPool<DraggableItemUI>(Create, Get, Release);

        foreach (Transform cell in _cellsHotbar)
        {
            SetCell(cell);
        }
        foreach (Transform cell in _cellsGrid)
        {
            SetCell(cell);
        }
        HidePanel();
    }

    private void SetCell(Transform cell)
    {
        var cellUI = cell.GetComponent<InventoryCellUI>();
        _inventoryCells.Add(cellUI);
        cellUI.OnItemPositionChanged += OnItemPositionChanged;
        cellUI.OnItemClick += OnItemClick;
    }

    private void OnItemPositionChanged(Vector3 position, InventoryCellUI cellUI)
    {
        int firstIndex = _inventoryCells.IndexOf(cellUI);
        InventoryCellUI secondCellUI = GetClosestCell(position);
        int secondIndex = _inventoryCells.IndexOf(secondCellUI);
        OnSwapCellItems?.Invoke(firstIndex, secondIndex);
    }

    private void OnItemClick(InventoryCellUI cellUI)
    {
        int index = _inventoryCells.IndexOf(cellUI);

        if(_selectedCell != null)
            _selectedCell.GetComponent<Image>().enabled = false;
        _selectedCell = cellUI;
        _selectedCell.GetComponent<Image>().enabled = true;

        OnCellItemClick?.Invoke(index);
    }

    public override void ShowPanel()
    {
        base.ShowPanel();
        _hudHotbar.gameObject.SetActive(false);
        _cellsHotbar.position = _inventoryHotbarPosition.transform.position;
        _inventoryCells.ForEach(cell => cell.SetDraggerActive(true));
    }

    public override void HidePanel()
    {
        base.HidePanel();
        _hudHotbar.gameObject.SetActive(true);
        _cellsHotbar.position = _hudHotbarPosition.position;
        _inventoryCells.ForEach(cell => cell.SetDraggerActive(false));
    }

    public void SetItemToCell(int index, InventoryItem item) => _inventoryCells[index].SetItem(_itemsPool, item);

    public void ClearCell(int index) => _inventoryCells[index].ClearCell(_itemsPool);

    private InventoryCellUI GetClosestCell(Vector3 position)
    {
        InventoryCellUI closestCell = _inventoryCells[0];

        foreach (InventoryCellUI cell in _inventoryCells)
        {
            if(cell.gameObject.activeInHierarchy && Vector3.Distance(cell.transform.position, position) < Vector3.Distance(closestCell.transform.position, position))
                closestCell = cell;
        }

        return closestCell;
    }

    private DraggableItemUI Create()
    {
        DraggableItemUI newItem = Instantiate(_itemPrefab);
        newItem.gameObject.SetActive(false);

        return newItem;
    }

    private void Get(DraggableItemUI item)
    {
        item.gameObject.SetActive(true);
    }

    private void Release(DraggableItemUI item)
    {
        item.gameObject.SetActive(false);
    }
}
