using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CraftManager : SingletonComponent<CraftManager>
{
    public event Action<ItemCraftStruct> OnItemCrafted;

    [SerializeField] private ItemCrafts _itemCrafts;
    [SerializeField] private InventorySystem _inventorySystem;

    public void SubscribeOnItemCrafted(Action<ItemCraftStruct> onItemCrafted) => OnItemCrafted += onItemCrafted;
    public void UnsubscribeOnItemCrafted(Action<ItemCraftStruct> onItemCrafted) => OnItemCrafted -= onItemCrafted;

    public List<ItemCraftStruct> GetAllCrafts() => _itemCrafts.itemCrafts;

    public List<ItemCraftStruct> GetAvalibleCrafts()
    {
        List<ItemCraftStruct> avalibleCrafts = new List<ItemCraftStruct>();

        foreach (ItemCraftStruct craft in _itemCrafts.itemCrafts)
        {
            List<InventoryItem> itemsForCraft = GetCraftItems(craft.CraftRecipe);
            if (itemsForCraft.Count == craft.CraftRecipe.Count)
            {
                avalibleCrafts.Add(craft);
                continue;
            }
        }

        return avalibleCrafts;
    }

    public List<InventoryItem> GetCraftItems(List<InventoryItem> craftRecipe)
    {
        List<InventoryItem> itemsForCraft = new List<InventoryItem>();

        foreach (InventoryItem item in craftRecipe)
        {
            InventoryItem avalibleItem = _inventorySystem.InventoryItems.FirstOrDefault(i => i.ItemObject == item.ItemObject && i.Count >= item.Count);
            if (avalibleItem.ItemObject != null && !itemsForCraft.Contains(avalibleItem))
            {
                itemsForCraft.Add(avalibleItem);
                continue;
            }
        }

        return itemsForCraft;
    }


    public void CraftItem(ItemCraftStruct craft)
    {
        OnItemCrafted?.Invoke(craft);
    }
}
