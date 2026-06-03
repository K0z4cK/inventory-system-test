using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class InventoryUI : BasePanelUI
{
    [Header("Prefabs")]
    [FormerlySerializedAs("_itemPrefab")]
    [SerializeField] private DraggableItemUI itemPrefab;

    [Header("Transforms of cells")]
    [FormerlySerializedAs("_cellsGrid")]
    [SerializeField] private Transform cellsGrid;
    [FormerlySerializedAs("_cellsHotbar")]
    [SerializeField] private Transform cellsHotbar;

    [Header("Hotbar")]
    [FormerlySerializedAs("_hudHotbar")]
    [SerializeField] private GameObject hudHotbar;
    [FormerlySerializedAs("_hudHotbarPosition")]
    [SerializeField] private Transform hudHotbarPosition;
    [FormerlySerializedAs("_inventoryHotbarPosition")]
    [SerializeField] private Transform inventoryHotbarPosition;

    private List<InventoryCellUI> _inventoryCells = new List<InventoryCellUI>();
    private InventoryCellUI _selectedCell;

    private ObjectPool<DraggableItemUI> _itemsPool;
    private IInventory _inventory;
    private IInventorySlotSelector _slotSelector;
    private bool _isPanelVisible;

    private void Awake()
    {
        _itemsPool = new ObjectPool<DraggableItemUI>(Create, Get, Release);

        foreach (Transform cell in cellsHotbar)
        {
            SetCell(cell);
        }
        foreach (Transform cell in cellsGrid)
        {
            SetCell(cell);
        }

        HidePanel();
    }

    public void Initialize(IInventory inventorySource, IInventorySlotSelector slotSelector)
    {
        if (_inventory != null)
            _inventory.OnSlotChanged -= OnInventorySlotChanged;

        _inventory = inventorySource;
        _slotSelector = slotSelector;

        if (_inventory == null)
        {
            Debug.LogError("InventoryUI requires IInventory.");
            return;
        }

        _inventory.OnSlotChanged += OnInventorySlotChanged;
        RefreshAllCells();
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
        _inventory?.SwapItems(firstIndex, secondIndex);
    }

    private void OnItemClick(InventoryCellUI cellUI)
    {
        int index = _inventoryCells.IndexOf(cellUI);
        if (index < 0)
            return;

        if(_selectedCell != null)
            _selectedCell.GetComponent<Image>().enabled = false;
        _selectedCell = cellUI;
        _selectedCell.GetComponent<Image>().enabled = true;

        _slotSelector?.SelectSlot(index);
    }

    public override void ShowPanel()
    {
        _isPanelVisible = true;
        base.ShowPanel();
        ApplyHotbarState();
        RefreshAllCells();
    }

    public override void HidePanel()
    {
        _isPanelVisible = false;
        base.HidePanel();
        ApplyHotbarState();
        RefreshAllCells();
    }

    public void SetItemToCell(int index, InventoryItem item)
    {
        if (!IsValidCellIndex(index))
            return;

        _inventoryCells[index].SetItem(_itemsPool, item);
    }

    public void ClearCell(int index)
    {
        if (!IsValidCellIndex(index))
            return;

        _inventoryCells[index].ClearCell(_itemsPool);
    }

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
        DraggableItemUI newItem = Instantiate(itemPrefab);
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

    private void RefreshAllCells()
    {
        if (_inventory == null)
            return;

        IReadOnlyList<InventoryItem> items = _inventory.InventoryItems;
        for (int i = 0; i < _inventoryCells.Count && i < items.Count; i++)
        {
            OnInventorySlotChanged(i, items[i]);
        }
    }

    private void OnInventorySlotChanged(int index, InventoryItem item)
    {
        if (item.IsEmpty)
            ClearCell(index);
        else
            SetItemToCell(index, item);

        ApplyHotbarState();
    }

    private bool IsValidCellIndex(int index) => index >= 0 && index < _inventoryCells.Count;

    private void ApplyHotbarState()
    {
        if (hudHotbar == null || cellsHotbar == null)
            return;

        hudHotbar.gameObject.SetActive(!_isPanelVisible);
        cellsHotbar.gameObject.SetActive(true);

        if (_isPanelVisible && inventoryHotbarPosition != null)
            cellsHotbar.position = inventoryHotbarPosition.position;
        else if (!_isPanelVisible && hudHotbarPosition != null)
            cellsHotbar.position = hudHotbarPosition.position;

        _inventoryCells.ForEach(cell => cell.SetDraggerActive(_isPanelVisible));
    }

    private void OnDestroy()
    {
        if (_inventory != null)
            _inventory.OnSlotChanged -= OnInventorySlotChanged;
    }
}
