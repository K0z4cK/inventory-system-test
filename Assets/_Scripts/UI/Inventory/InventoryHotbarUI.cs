using System.Collections.Generic;
using Infrastructure;
using UnityEngine;
using UnityEngine.Pool;

public class InventoryHotbarUI : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private DraggableItemUI itemPrefab;

    [Header("Cells")]
    [SerializeField] private Transform cellsRoot;

    private readonly List<InventoryCellUI> _cells = new List<InventoryCellUI>();

    private ObjectPool<DraggableItemUI> _itemsPool;
    private ILocalPlayerProvider _localPlayerProvider;
    private IPlayerContext _boundPlayer;
    private IInventory _inventory;
    private IInventorySlotSelector _slotSelector;

    private void Awake()
    {
        _itemsPool = new ObjectPool<DraggableItemUI>(CreateItem, ShowItem, HideItem);
        RegisterCells();
        ResolveProvider();
        BindCurrentPlayer();
    }

    private void OnEnable()
    {
        ResolveProvider();
        _localPlayerProvider.OnLocalPlayerChanged += HandleLocalPlayerChanged;
        BindCurrentPlayer();
    }

    private void OnDisable()
    {
        if (_localPlayerProvider != null)
            _localPlayerProvider.OnLocalPlayerChanged -= HandleLocalPlayerChanged;

        UnbindPlayer();
    }

    private void RegisterCells()
    {
        if (cellsRoot == null)
            return;

        foreach (Transform cellTransform in cellsRoot)
        {
            InventoryCellUI cell = cellTransform.GetComponent<InventoryCellUI>();
            if (cell == null)
                continue;

            cell.OnItemClick += HandleCellClicked;
            cell.SetDraggerActive(false);
            _cells.Add(cell);
        }
    }

    private void ResolveProvider()
    {
        if (_localPlayerProvider == null)
            _localPlayerProvider = ProjectContext.Get<ILocalPlayerProvider>();
    }

    private void BindCurrentPlayer()
    {
        IPlayerContext localPlayer = _localPlayerProvider.LocalPlayer;
        if (ReferenceEquals(_boundPlayer, localPlayer))
            return;

        UnbindPlayer();
        _boundPlayer = localPlayer;
        if (_boundPlayer == null)
            return;

        _inventory = _boundPlayer.Get<IInventory>();
        _slotSelector = _boundPlayer.Get<IInventorySlotSelector>();
        _inventory.OnSlotChanged += HandleSlotChanged;
        _slotSelector.OnSelectedItemChanged += HandleSelectedItemChanged;
        RefreshAll();
    }

    private void UnbindPlayer()
    {
        if (_inventory != null)
            _inventory.OnSlotChanged -= HandleSlotChanged;
        if (_slotSelector != null)
            _slotSelector.OnSelectedItemChanged -= HandleSelectedItemChanged;

        _boundPlayer = null;
        _inventory = null;
        _slotSelector = null;
    }

    private void HandleLocalPlayerChanged(IPlayerContext playerContext)
    {
        BindCurrentPlayer();
    }

    private void HandleCellClicked(InventoryCellUI cell)
    {
        int index = _cells.IndexOf(cell);
        if (index >= 0)
            _slotSelector?.SelectSlot(index);
    }

    private void HandleSlotChanged(int index, InventoryItem item)
    {
        if (index < 0 || index >= _cells.Count)
            return;

        if (item.IsEmpty)
            _cells[index].ClearCell(_itemsPool);
        else
            _cells[index].SetItem(_itemsPool, item);
    }

    private void HandleSelectedItemChanged(ItemObject itemObject)
    {
        RefreshSelection();
    }

    private void RefreshAll()
    {
        if (_inventory == null)
            return;

        for (int i = 0; i < _cells.Count; i++)
        {
            InventoryItem item = i < _inventory.InventoryItems.Count
                ? _inventory.InventoryItems[i]
                : default;
            HandleSlotChanged(i, item);
        }

        RefreshSelection();
    }

    private void RefreshSelection()
    {
        int selectedIndex = _slotSelector != null ? _slotSelector.SelectedSlotIndex : -1;
        for (int i = 0; i < _cells.Count; i++)
            _cells[i].SetSelected(i == selectedIndex);
    }

    private DraggableItemUI CreateItem()
    {
        if (itemPrefab == null)
        {
            Debug.LogError("InventoryHotbarUI requires DraggableItemUI prefab.", this);
            return null;
        }

        DraggableItemUI item = Instantiate(itemPrefab);
        item.gameObject.SetActive(false);
        item.SetDraggerActive(false);
        return item;
    }

    private void ShowItem(DraggableItemUI item)
    {
        if (item != null)
        {
            item.gameObject.SetActive(true);
            item.SetDraggerActive(false);
        }
    }

    private void HideItem(DraggableItemUI item)
    {
        if (item != null)
            item.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        foreach (InventoryCellUI cell in _cells)
            cell.OnItemClick -= HandleCellClicked;
    }
}
