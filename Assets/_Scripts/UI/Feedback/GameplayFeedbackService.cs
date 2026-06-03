using System;

public class GameplayFeedbackService : IGameplayFeedback
{
    public event Action<string> OnMessageRaised;

    public void ShowMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        OnMessageRaised?.Invoke(message);
    }

    public void ShowPickedUp(ItemObject itemObject, int count)
    {
        ShowMessage($"Picked up {GetItemName(itemObject)} x{count}");
    }

    public void ShowInventoryFull(ItemObject itemObject, int count)
    {
        ShowMessage($"Inventory full: cannot pick up {GetItemName(itemObject)} x{count}");
    }

    public void ShowCraftSucceeded(ItemObject itemObject, int count)
    {
        ShowMessage($"Crafted {GetItemName(itemObject)} x{count}");
    }

    public void ShowCraftUnavailable(string reason)
    {
        ShowMessage(string.IsNullOrWhiteSpace(reason) ? "Cannot craft selected item" : reason);
    }

    public void ShowItemDiscovered(ItemObject itemObject, int discoveredCount, int totalCount)
    {
        ShowMessage($"New discovery: {GetItemName(itemObject)} ({discoveredCount}/{totalCount})");
    }

    public void ShowCollectionMilestone(string rankName, int discoveredCount, int totalCount)
    {
        ShowMessage($"{rankName}: collection progress {discoveredCount}/{totalCount}");
    }

    private string GetItemName(ItemObject itemObject)
    {
        if (itemObject == null)
            return "item";

        return string.IsNullOrWhiteSpace(itemObject.Name) ? itemObject.ItemId : itemObject.Name;
    }
}
