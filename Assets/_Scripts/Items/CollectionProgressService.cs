using System;
using System.Collections.Generic;

public class CollectionProgressService : ICollectionProgressService, IDisposable
{
    private const string RookieRank = "Rookie Collector";
    private const string ExplorerRank = "Field Explorer";
    private const string ArchivistRank = "Item Archivist";
    private const string MasterRank = "Master Collector";

    private readonly ItemDatabase _itemDatabase;
    private readonly IInventory _inventory;
    private readonly HashSet<string> _discoveredItemIds = new HashSet<string>();

    private string _currentRank = RookieRank;

    public event Action<ItemObject, int, int> OnItemDiscovered;
    public event Action<string, int, int> OnMilestoneReached;
    public event Action OnProgressChanged;

    public int DiscoveredCount => _discoveredItemIds.Count;
    public int TotalCount => CountTrackableItems();
    public string CurrentRank => GetRank(DiscoveredCount, TotalCount);

    public CollectionProgressService(ItemDatabase itemDatabase, IInventory inventory)
    {
        _itemDatabase = itemDatabase;
        _inventory = inventory;

        if (_inventory != null)
        {
            _inventory.OnSlotChanged += HandleSlotChanged;
            DiscoverExistingInventoryItems();
        }
    }

    public bool IsDiscovered(ItemObject itemObject)
    {
        return itemObject != null
               && !string.IsNullOrWhiteSpace(itemObject.ItemId)
               && _discoveredItemIds.Contains(itemObject.ItemId);
    }

    public bool TryDiscover(ItemObject itemObject)
    {
        if (!CanTrack(itemObject))
            return false;

        if (!_discoveredItemIds.Add(itemObject.ItemId))
            return false;

        int discoveredCount = DiscoveredCount;
        int totalCount = TotalCount;
        OnItemDiscovered?.Invoke(itemObject, discoveredCount, totalCount);
        RaiseProgressChanged(discoveredCount, totalCount);
        return true;
    }

    public void Dispose()
    {
        if (_inventory != null)
            _inventory.OnSlotChanged -= HandleSlotChanged;
    }

    private void HandleSlotChanged(int index, InventoryItem item)
    {
        if (!item.IsEmpty)
            TryDiscover(item.ItemObject);
    }

    private void DiscoverExistingInventoryItems()
    {
        if (_inventory == null)
            return;

        foreach (InventoryItem item in _inventory.InventoryItems)
        {
            if (!item.IsEmpty)
                TryDiscover(item.ItemObject);
        }
    }

    private void RaiseProgressChanged(int discoveredCount, int totalCount)
    {
        string newRank = GetRank(discoveredCount, totalCount);
        if (newRank != _currentRank)
        {
            _currentRank = newRank;
            OnMilestoneReached?.Invoke(newRank, discoveredCount, totalCount);
        }

        OnProgressChanged?.Invoke();
    }

    private bool CanTrack(ItemObject itemObject)
    {
        if (itemObject == null || string.IsNullOrWhiteSpace(itemObject.ItemId) || _itemDatabase == null)
            return false;

        return _itemDatabase.GetById(itemObject.ItemId) != null;
    }

    private int CountTrackableItems()
    {
        if (_itemDatabase == null)
            return 0;

        int count = 0;
        foreach (ItemObject item in _itemDatabase.Items)
        {
            if (item != null && !string.IsNullOrWhiteSpace(item.ItemId))
                count++;
        }

        return count;
    }

    private string GetRank(int discoveredCount, int totalCount)
    {
        if (totalCount <= 0 || discoveredCount <= 0)
            return RookieRank;

        if (discoveredCount >= totalCount)
            return MasterRank;

        float progress = (float)discoveredCount / totalCount;
        if (progress >= 0.5f)
            return ArchivistRank;

        return ExplorerRank;
    }
}
