using System.Collections.Generic;
using Infrastructure;
using UnityEngine;
using UnityEngine.Pool;

public class InventoryUI : PlayerWindow
{
    [Header("Prefabs")]
    [SerializeField] private DraggableItemUI itemPrefab;

    [Header("Cells")]
    [SerializeField] private Transform cellsGrid;
    [SerializeField] private List<Transform> cellsBeforeGrid = new List<Transform>();

    [Header("Drop Area")]
    [SerializeField] private RectTransform dropArea;
    [SerializeField, Min(0f)] private float dropAreaPadding = 32f;

    [Header("Cell Magnet")]
    [SerializeField, Min(0f)] private float cellSnapDistance = 48f;

    private List<InventoryCellUI> _inventoryCells = new List<InventoryCellUI>();

    private ObjectPool<DraggableItemUI> _itemsPool;
    private IInventory _inventory;
    private IInventorySlotSelector _slotSelector;
    private IInventoryDropService _dropService;

    protected override void Awake()
    {
        _itemsPool = new ObjectPool<DraggableItemUI>(Create, Get, Release);

        if (dropArea == null)
            dropArea = transform as RectTransform;

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
            if (IsInsideDropArea(position) || _dropService == null || !_dropService.TryDropSlot(firstIndex))
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
        InventoryCellUI closestCell = null;
        float closestDistance = float.MaxValue;

        foreach (InventoryCellUI cell in _inventoryCells)
        {
            if (!cell.gameObject.activeInHierarchy)
                continue;

            RectTransform rectTransform = cell.transform as RectTransform;
            if (rectTransform == null)
                continue;

            if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, screenPosition))
                return cell;

            if (cellSnapDistance <= 0f)
                continue;

            float distance = GetDistanceToRect(rectTransform, screenPosition);
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestCell = cell;
        }

        return closestDistance <= cellSnapDistance ? closestCell : null;
    }

    private bool IsInsideDropArea(Vector3 screenPosition)
    {
        if (dropArea == null)
            return false;

        Camera eventCamera = GetCanvasCamera(dropArea);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                dropArea,
                screenPosition,
                eventCamera,
                out Vector2 localPoint))
        {
            return false;
        }

        Rect rect = dropArea.rect;
        rect.xMin -= dropAreaPadding;
        rect.xMax += dropAreaPadding;
        rect.yMin -= dropAreaPadding;
        rect.yMax += dropAreaPadding;

        return rect.Contains(localPoint);
    }

    private Camera GetCanvasCamera(RectTransform rectTransform)
    {
        Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            return null;

        return canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
    }

    private float GetDistanceToRect(RectTransform rectTransform, Vector3 screenPosition)
    {
        Camera eventCamera = GetCanvasCamera(rectTransform);
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rectTransform,
                screenPosition,
                eventCamera,
                out Vector2 localPoint))
        {
            return float.MaxValue;
        }

        Rect rect = rectTransform.rect;
        float xDistance = Mathf.Max(rect.xMin - localPoint.x, 0f, localPoint.x - rect.xMax);
        float yDistance = Mathf.Max(rect.yMin - localPoint.y, 0f, localPoint.y - rect.yMax);

        return new Vector2(xDistance, yDistance).magnitude;
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
