using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemCrafts", menuName = "ScriptableObjects/ItemCraftsScriptableObject")]
public class ItemCrafts : ScriptableObject
{
    public List<ItemCraftStruct> itemCrafts;
}

[Serializable]
public struct ItemCraftStruct
{
    public List<InventoryItem> CraftRecipe;
    public InventoryItem ItemResult;
}
