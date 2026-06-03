public interface IGameplayFeedback
{
    void ShowMessage(string message);
    void ShowPickedUp(ItemObject itemObject, int count);
    void ShowInventoryFull(ItemObject itemObject, int count);
    void ShowCraftSucceeded(ItemObject itemObject, int count);
    void ShowCraftUnavailable(string reason);
    void ShowItemDiscovered(ItemObject itemObject, int discoveredCount, int totalCount);
    void ShowCollectionMilestone(string rankName, int discoveredCount, int totalCount);
}
