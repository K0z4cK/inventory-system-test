using System;

public enum GameplayFeedbackKind
{
    Information,
    Success,
    Warning,
    Failure,
    Progress
}

[Serializable]
public readonly struct GameplayFeedbackMessage
{
    public string Text { get; }
    public GameplayFeedbackKind Kind { get; }

    public GameplayFeedbackMessage(string text, GameplayFeedbackKind kind)
    {
        Text = text;
        Kind = kind;
    }
}

public interface IGameplayFeedback
{
    void ShowMessage(string message, GameplayFeedbackKind kind = GameplayFeedbackKind.Information);
    void ShowPickedUp(ItemObject itemObject, int count);
    void ShowHarvested(ItemObject itemObject, int count, int remainingCount);
    void ShowInventoryFull(ItemObject itemObject, int count);
    void ShowItemSelected(ItemObject itemObject);
    void ShowInvalidInventorySelection();
    void ShowCraftSucceeded(ItemObject itemObject, int count);
    void ShowCraftUnavailable(string reason);
    void ShowNoCraftsAvailable();
    void ShowCraftSelectionRequired();
    void ShowItemDiscovered(ItemObject itemObject, int discoveredCount, int totalCount);
    void ShowCollectionMilestone(string rankName, int discoveredCount, int totalCount);
    void ShowNoInteractable();
    void ShowRequiredTool(ItemObject requiredTool);
    void ShowResourceDepleted(ItemObject itemObject);
    void ShowNoAttackTarget();
    void ShowAttackTargetOutOfRange(string targetName);
    void ShowDamageDealt(string targetName, int damage, int remainingHealth, int maxHealth);
    void ShowTargetDefeated(string targetName);
    void ShowLootDropped(int instanceCount);
}
