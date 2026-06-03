using System.Collections.Generic;
using System.Linq;

public class CraftingService
{
    private readonly ItemCrafts _itemCrafts;
    private readonly IInventory _inventory;

    public CraftingService(ItemCrafts itemCrafts, IInventory inventory)
    {
        _itemCrafts = itemCrafts;
        _inventory = inventory;
    }

    public List<ItemCraftStruct> GetAllCrafts()
    {
        return _itemCrafts != null && _itemCrafts.itemCrafts != null
            ? _itemCrafts.itemCrafts
            : new List<ItemCraftStruct>();
    }

    public List<ItemCraftStruct> GetAvailableCrafts()
    {
        return GetAllCrafts().Where(CanCraft).ToList();
    }

    public List<InventoryItem> GetCraftItems(List<InventoryItem> craftRecipe)
    {
        List<InventoryItem> itemsForCraft = new List<InventoryItem>();
        if (_inventory == null || craftRecipe == null)
            return itemsForCraft;

        foreach (InventoryItem item in craftRecipe)
        {
            if (!item.IsEmpty && _inventory.HasItems(item.ItemObject, item.Count))
                itemsForCraft.Add(item);
        }

        return itemsForCraft;
    }

    public List<InventoryItem> GetMissingItems(List<InventoryItem> craftRecipe)
    {
        List<InventoryItem> missingItems = new List<InventoryItem>();
        if (_inventory == null || craftRecipe == null)
            return missingItems;

        foreach (InventoryItem item in craftRecipe)
        {
            if (item.IsEmpty)
                continue;

            int missingCount = item.Count - _inventory.CountItems(item.ItemObject);
            if (missingCount > 0)
                missingItems.Add(new InventoryItem(item.ItemObject, missingCount));
        }

        return missingItems;
    }

    public bool CanCraft(ItemCraftStruct craft)
    {
        if (_inventory == null || craft.CraftRecipe == null || craft.ItemResult.IsEmpty)
            return false;

        foreach (InventoryItem item in craft.CraftRecipe)
        {
            if (item.IsEmpty || !_inventory.HasItems(item.ItemObject, item.Count))
                return false;
        }

        return _inventory.CanAddItemsAfterRemoving(
            craft.ItemResult.ItemObject,
            craft.ItemResult.Count,
            craft.CraftRecipe);
    }

    public bool TryCraft(ItemCraftStruct craft)
    {
        if (!CanCraft(craft))
            return false;

        foreach (InventoryItem item in craft.CraftRecipe)
        {
            if (!_inventory.TryRemoveItems(item.ItemObject, item.Count))
                return false;
        }

        return _inventory.TryAddItems(craft.ItemResult.ItemObject, craft.ItemResult.Count);
    }
}
