using UnityEngine;
using UnityEngine.Serialization;

public class HarvestableResource : BaseItem, IInteractable
{
    [FormerlySerializedAs("_requiredTool")]
    [SerializeField] private ItemObject requiredTool;

    [FormerlySerializedAs("_resourceCount")]
    [SerializeField, Min(1)] private int resourceCount = 1;

    [SerializeField] private bool collectAllAtOnce;

    [SerializeField, Min(1)] private int minCountPerInteraction = 1;
    [SerializeField, Min(1)] private int maxCountPerInteraction = 1;

    public bool Interact(Character character)
    {
        if (character == null || itemObject == null || resourceCount <= 0)
            return false;

        if (requiredTool != null && !character.HasSelectedItem(requiredTool))
            return false;

        int countToCollect = GetCountToCollect();
        if (!character.TryAddItemsToInventory(itemObject, countToCollect))
            return false;

        resourceCount -= countToCollect;
        if (resourceCount <= 0)
            Destroy(gameObject);

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
