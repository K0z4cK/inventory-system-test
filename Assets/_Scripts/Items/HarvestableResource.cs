using UnityEngine;

public class HarvestableResource : BaseItem, IInteractable
{
    [SerializeField] private ItemObject requiredTool;

    [SerializeField, Min(1)] private int resourceCount = 1;

    [SerializeField] private bool collectAllAtOnce;

    [SerializeField, Min(1)] private int minCountPerInteraction = 1;
    [SerializeField, Min(1)] private int maxCountPerInteraction = 1;

    private bool _isDepleted;

    public bool CanInteract => !_isDepleted && itemObject != null && resourceCount > 0;

    public bool Interact(Character character)
    {
        if (character == null || !CanInteract)
            return false;

        if (requiredTool != null && !character.HasSelectedItem(requiredTool))
        {
            character.ShowRequiredToolFeedback(requiredTool);
            return false;
        }

        int countToCollect = GetCountToCollect();
        int remainingCount = Mathf.Max(0, resourceCount - countToCollect);
        if (!character.TryHarvestItemsToInventory(itemObject, countToCollect, remainingCount))
            return false;

        resourceCount -= countToCollect;
        if (resourceCount <= 0)
        {
            _isDepleted = true;
            character.ShowResourceDepletedFeedback(itemObject);
            Destroy(gameObject);
        }

        return true;
    }

    private int GetCountToCollect()
    {
        if (collectAllAtOnce)
            return resourceCount;

        int minCount = Mathf.Max(1, minCountPerInteraction);
        int maxCount = Mathf.Max(minCount, maxCountPerInteraction);
        return Mathf.Min(Random.Range(minCount, maxCount + 1), resourceCount);
    }

    private void OnValidate()
    {
        resourceCount = Mathf.Max(1, resourceCount);
        minCountPerInteraction = Mathf.Max(1, minCountPerInteraction);
        maxCountPerInteraction = Mathf.Max(minCountPerInteraction, maxCountPerInteraction);
    }
}
