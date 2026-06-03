using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "ScriptableObjects/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    [SerializeField] private List<ItemObject> items = new List<ItemObject>();

    public IReadOnlyList<ItemObject> Items => items;

    public bool TryGetById(string itemId, out ItemObject itemObject)
    {
        itemObject = GetById(itemId);
        return itemObject != null;
    }

    public ItemObject GetById(string itemId)
    {
        if (string.IsNullOrWhiteSpace(itemId))
            return null;

        foreach (ItemObject item in items)
        {
            if (item != null && item.ItemId == itemId)
                return item;
        }

        return null;
    }

    public List<string> GetValidationErrors()
    {
        List<string> errors = new List<string>();
        HashSet<string> itemIds = new HashSet<string>();

        for (int i = 0; i < items.Count; i++)
        {
            ItemObject item = items[i];
            if (item == null)
            {
                errors.Add($"ItemDatabase has an empty item reference at index {i}.");
                continue;
            }

            if (string.IsNullOrWhiteSpace(item.ItemId))
                errors.Add($"{item.name} has an empty ItemId.");
            else if (!itemIds.Add(item.ItemId))
                errors.Add($"Duplicate ItemId '{item.ItemId}' in ItemDatabase.");

            if (string.IsNullOrWhiteSpace(item.Name))
                errors.Add($"{item.name} has an empty display Name.");

            if (item.Sprite == null)
                errors.Add($"{item.name} has no Sprite assigned.");
        }

        return errors;
    }

    private void OnValidate()
    {
        foreach (string error in GetValidationErrors())
        {
            Debug.LogWarning(error, this);
        }
    }
}
