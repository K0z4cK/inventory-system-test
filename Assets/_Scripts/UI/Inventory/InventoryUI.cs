using System.Collections.Generic;
using Infrastructure;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Serialization;

public class InventoryUI : PlayerWindow
{
    [Header("Prefabs")]
    [FormerlySerializedAs("_itemPrefab")]
    [SerializeField] private DraggableItemUI itemPrefab;

    [Header("Cells")]
    [FormerlySerializedAs("_cellsGrid")]
    [SerializeField] private Transform cellsGrid;
    [FormerlySerializedAs("additionalCellsRoots")]
    [SerializeField] private List<Transform> cellsBeforeGrid = new List<Transform>();

    private List<InventoryCellUI> _inventoryCells = new List<InventoryCellUI>();

    private ObjectPool<DraggableItemUI> _itemsPool;
    private IInventory _inventory;
    private IInventorySlotSelector _slotSelector;
    private IInventoryDropService _dropService;

    protected override void Awake()
    {
        _itemsPool = new ObjectPool<DraggableItemUI>(Create, Get, Release);

        RegisterAdditionalCells();
        RegisterCells(cellsGrid);

        base.Awake();
    }

    protected override void BindPlayer(IPlayerContext playerContext)
    {
        playerContext.TryGet(out _dropService);
        BindInventory(playerContext.Get<IInventory>(), playerContext.Get<IInventorySlotSelector>());
    }

    protected override void UnbindPlayer()
    {
        _dropService = null;
        BindInventory(null, null);
    }

    private void BindInventory(IInventory inventorySource, IInventorySlotSelector slotSelector)
    {
        if (_inventory != null)
            _inventory.OnSlotChanged -= OnInventorySlotChanged;
        if (_slotSelector != null)
            _slotSelector.OnSelectedItemChanged -= HandleSelectedItemChanged;

        _inventory = inventorySource;
        _slotSelector = slotSelector;

        if (_inventory == null)
            return;

        _inventory.OnSlotChanged += OnInventorySlotChanged;
        if (_slotSelector != null)
            _slotSelector.OnSelectedItemChanged += HandleSelectedItemChanged;

        RefreshAllCells();
        RefreshSelection();
    }

    private void RegisterCells(Transform cellsRoot)
    {
        if (cellsRoot == null)
            return;

        foreach (Transform cell in cellsRoot)
            SetCell(cell);
    }

    private void RegisterAdditionalCells()
    {
        if (cellsBeforeGrid == null)
            return;

        foreach (Transform cellsRoot in cellsBeforeGrid)
            RegisterCells(cellsRoot);
    }

    private void SetCell(Transform cell)
    {
        var cellUI = cell.GetComponent<InventoryCellUI>();
        if (cellUI == null)
            return;

        _inventoryCells.Add(cellUI);
        cellUI.OnItemPositionChanged += OnItemPositionChanged;
        cellUI.OnItemClick += OnItemClick;
    }

    private void OnItemPositionChanged(Vector3 position, InventoryCellUI cellUI)
    {
        int firstIndex = _inventoryCells.IndexOf(cellUI);
        InventoryCellUI secondCellUI = GetCellAtScreenPosition(position);
        if (firstIndex < 0)
        {
            RefreshAllCells();
            return;
        }

        if (secondCellUI == null)
        {
            if (_dropService == null || !_dropService.TryDropSlot(firstIndex))
                RefreshAllCells();

            return;
        }

        int secondIndex = _inventoryCells.IndexOf(secondCellUI);
        if (secondIndex < 0 || firstIndex == secondIndex)
        {
            RefreshAllCells();
            return;
        }

        _inventory?.SwapItems(firstIndex, secondIndex);
    }

    private void OnItemClick(InventoryCellUI cellUI)
    {
        int index = _inventoryCells.IndexOf(cellUI);
        if (index < 0)
            return;

        _slotSelector?.SelectSlot(index);
    }

    protected override void OnOpened()
    {
        SetCellsDraggerActive(true);
        RefreshAllCells();
    }

    protected override void OnClosed()
    {
        SetCellsDraggerActive(false);
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

    private InventoryCellUI GetCellAtScreenPosition(Vector3 screenPosition)
    {
        foreach (InventoryCellUI cell in _inventoryCells)
        {
            if (!cell.gameObject.activeInHierarchy)
                continue;

            RectTransform rectTransform = cell.transform as RectTransform;
            if (rectTransform == null)
                continue;

            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition))
                return cell;
        }

        return null;
    }

    private DraggableItemUI Create()
    {
        if (itemPrefab == null)
        {
            Debug.LogError("InventoryUI requires DraggableItemUI prefab.", this);
            return null;
        }

        DraggableItemUI newItem = Instantiate(itemPrefab);
        newItem.gameObject.SetActive(false);

        return newItem;
    }

    private void Get(DraggableItemUI item)
    {
        if (item != null)
            item.gameObject.SetActive(true);
    }

    private void Release(DraggableItemUI item)
    {
        if (item != null)
            item.gameObject.SetActive(false);
    }

    private void RefreshAllCells()
    {
        if (_inventory == null)
            return;

        IReadOnlyList<InventoryItem> items = _inventory.InventoryItems;
        for (int i = 0; i < _inventoryCells.Count; i++)
        {
            InventoryItem item = i < items.Count ? items[i] : default;
            OnInventorySlotChanged(i, item);
        }
    }

    private void OnInventorySlotChanged(int index, InventoryItem item)
    {
        if (item.IsEmpty)
            ClearCell(index);
        else
            SetItemToCell(index, item);
    }

    private void HandleSelectedItemChanged(ItemObject itemObject)
    {
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        int selectedIndex = _slotSelector != null ? _slotSelector.SelectedSlotIndex : -1;
        for (int i = 0; i < _inventoryCells.Count; i++)
            _inventoryCells[i].SetSelected(i == selectedIndex);

    }

    private bool IsValidCellIndex(int index) => index >= 0 && index < _inventoryCells.Count;

    private void SetCellsDraggerActive(bool isActive)
    {
        _inventoryCells.ForEach(cell => cell.SetDraggerActive(isActive));
    }

    protected override void OnDisable()
    {
        if (_inventory != null)
            _inventory.OnSlotChanged -= OnInventorySlotChanged;
        if (_slotSelector != null)
            _slotSelector.OnSelectedItemChanged -= HandleSelectedItemChanged;

        base.OnDisable();
    }
}
