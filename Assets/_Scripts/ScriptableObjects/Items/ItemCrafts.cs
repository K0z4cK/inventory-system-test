using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemCrafts", menuName = "ScriptableObjects/ItemCraftsScriptableObject")]
public class ItemCrafts : ScriptableObject
{
    public List<ItemCraftStruct> itemCrafts;

    private void OnValidate()
    {
        if (itemCrafts == null)
            return;

        for (int i = 0; i < itemCrafts.Count; i++)
        {
            ItemCraftStruct craft = itemCrafts[i];
            if (craft.ItemResult.IsEmpty)
                Debug.LogWarning($"Craft recipe at index {i} has no result item.", this);

            if (craft.CraftRecipe == null || craft.CraftRecipe.Count == 0)
            {
                Debug.LogWarning($"Craft recipe at index {i} has no ingredients.", this);
                continue;
            }

            foreach (InventoryItem ingredient in craft.CraftRecipe)
            {
                if (ingredient.IsEmpty)
                    Debug.LogWarning($"Craft recipe at index {i} has an empty ingredient.", this);
            }
        }
    }
}

[Serializable]
public struct ItemCraftStruct
{
    public List<InventoryItem> CraftRecipe;
    public InventoryItem ItemResult;
}
