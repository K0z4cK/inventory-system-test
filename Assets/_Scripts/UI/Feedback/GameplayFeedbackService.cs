using System;

public class GameplayFeedbackService : IGameplayFeedback, IGameplayFeedbackSource
{
    public event Action<GameplayFeedbackMessage> OnFeedbackRaised;
    public event Action<string> OnMessageRaised;

    public void ShowMessage(string message, GameplayFeedbackKind kind = GameplayFeedbackKind.Information)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        OnFeedbackRaised?.Invoke(new GameplayFeedbackMessage(message, kind));
        OnMessageRaised?.Invoke(message);
    }

    public void ShowPickedUp(ItemObject itemObject, int count)
    {
        ShowMessage($"Picked up {GetItemName(itemObject)} x{count}", GameplayFeedbackKind.Success);
    }

    public void ShowHarvested(ItemObject itemObject, int count, int remainingCount)
    {
        string remainingText = remainingCount > 0 ? $" ({remainingCount} remaining)" : string.Empty;
        ShowMessage($"Collected {GetItemName(itemObject)} x{count}{remainingText}", GameplayFeedbackKind.Success);
    }

    public void ShowInventoryFull(ItemObject itemObject, int count)
    {
        ShowMessage($"Inventory full: cannot collect {GetItemName(itemObject)} x{count}", GameplayFeedbackKind.Failure);
    }

    public void ShowItemDropped(ItemObject itemObject, int count)
    {
        ShowMessage($"Dropped {GetItemName(itemObject)} x{count}", GameplayFeedbackKind.Information);
    }

    public void ShowItemSelected(ItemObject itemObject)
    {
        ShowMessage($"Selected {GetItemName(itemObject)}", GameplayFeedbackKind.Information);
    }

    public void ShowInvalidInventorySelection()
    {
        ShowMessage("Cannot select an empty inventory slot", GameplayFeedbackKind.Warning);
    }

    public void ShowCraftSucceeded(ItemObject itemObject, int count)
    {
        ShowMessage($"Crafted {GetItemName(itemObject)} x{count}", GameplayFeedbackKind.Success);
    }

    public void ShowCraftUnavailable(string reason)
    {
        ShowMessage(
            string.IsNullOrWhiteSpace(reason) ? "Cannot craft selected item" : reason,
            GameplayFeedbackKind.Failure);
    }

    public void ShowNoCraftsAvailable()
    {
        ShowMessage("No crafting recipes are available", GameplayFeedbackKind.Warning);
    }

    public void ShowCraftSelectionRequired()
    {
        ShowMessage("Select a recipe to craft", GameplayFeedbackKind.Information);
    }

    public void ShowItemDiscovered(ItemObject itemObject, int discoveredCount, int totalCount)
    {
        ShowMessage(
            $"New discovery: {GetItemName(itemObject)} ({discoveredCount}/{totalCount})",
            GameplayFeedbackKind.Progress);
    }

    public void ShowCollectionMilestone(string rankName, int discoveredCount, int totalCount)
    {
        ShowMessage(
            $"{rankName}: collection progress {discoveredCount}/{totalCount}",
            GameplayFeedbackKind.Progress);
    }

    public void ShowNoInteractable()
    {
        ShowMessage("Nothing nearby to interact with", GameplayFeedbackKind.Warning);
    }

    public void ShowRequiredTool(ItemObject requiredTool)
    {
        ShowMessage($"Requires {GetItemName(requiredTool)}", GameplayFeedbackKind.Failure);
    }

    public void ShowResourceDepleted(ItemObject itemObject)
    {
        ShowMessage($"{GetItemName(itemObject)} depleted", GameplayFeedbackKind.Information);
    }

    public void ShowNoAttackTarget()
    {
        ShowMessage("No target to attack", GameplayFeedbackKind.Warning);
    }

    public void ShowAttackTargetOutOfRange(string targetName)
    {
        ShowMessage($"{GetTargetName(targetName)} is out of range", GameplayFeedbackKind.Warning);
    }

    public void ShowDamageDealt(string targetName, int damage, int remainingHealth, int maxHealth)
    {
        ShowMessage(
            $"Hit {GetTargetName(targetName)} for {Math.Max(1, damage)} damage ({Math.Max(0, remainingHealth)}/{Math.Max(1, maxHealth)})",
            GameplayFeedbackKind.Success);
    }

    public void ShowTargetDefeated(string targetName)
    {
        ShowMessage($"{GetTargetName(targetName)} defeated", GameplayFeedbackKind.Success);
    }

    public void ShowLootDropped(int instanceCount)
    {
        if (instanceCount <= 0)
            return;

        ShowMessage($"Dropped loot x{instanceCount}", GameplayFeedbackKind.Information);
    }

    private string GetItemName(ItemObject itemObject)
    {
        if (itemObject == null)
            return "item";

        return string.IsNullOrWhiteSpace(itemObject.Name) ? itemObject.ItemId : itemObject.Name;
    }

    private string GetTargetName(string targetName)
    {
        return string.IsNullOrWhiteSpace(targetName) ? "Target" : targetName;
    }
}
