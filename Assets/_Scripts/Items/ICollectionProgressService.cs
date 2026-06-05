using System;

public interface ICollectionProgressService
{
    event Action<ItemObject, int, int> OnItemDiscovered;
    event Action<string, int, int> OnMilestoneReached;
    event Action OnProgressChanged;

    int DiscoveredCount { get; }
    int TotalCount { get; }
    string CurrentRank { get; }

    bool IsDiscovered(ItemObject itemObject);
    bool TryDiscover(ItemObject itemObject);
}
