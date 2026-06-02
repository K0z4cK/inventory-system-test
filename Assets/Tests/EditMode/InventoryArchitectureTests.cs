using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class InventoryArchitectureTests
{
    [Test]
    public void TryAddItems_DoesNotPartiallyMutateInventory_WhenThereIsNotEnoughSpace()
    {
        ItemObject wood = CreateItem("wood");
        InventoryModel inventory = new InventoryModel(1, 5);

        Assert.That(inventory.TryAddItems(wood, 4), Is.True);
        Assert.That(inventory.TryAddItems(wood, 2), Is.False);

        Assert.That(inventory.CountItems(wood), Is.EqualTo(4));
    }

    [Test]
    public void TryAddItems_RaisesSlotChanged_WhenSlotChanges()
    {
        ItemObject wood = CreateItem("wood");
        InventoryModel inventory = new InventoryModel(1, 5);
        int changedSlot = -1;
        InventoryItem changedItem = default;

        inventory.OnSlotChanged += (slot, item) =>
        {
            changedSlot = slot;
            changedItem = item;
        };

        inventory.TryAddItems(wood, 3);

        Assert.That(changedSlot, Is.EqualTo(0));
        Assert.That(changedItem.ItemObject, Is.EqualTo(wood));
        Assert.That(changedItem.Count, Is.EqualTo(3));
    }

    [Test]
    public void CanCraft_ReturnsFalse_WhenIngredientsAreMissing()
    {
        ItemObject wood = CreateItem("wood");
        ItemObject stone = CreateItem("stone");
        ItemObject axe = CreateItem("axe");
        InventoryModel inventory = new InventoryModel(2, 5);
        CraftingService crafting = new CraftingService(CreateCrafts(CreateCraft(axe, 1, (wood, 1), (stone, 1))), inventory);

        inventory.TryAddItems(wood, 1);

        Assert.That(crafting.CanCraft(crafting.GetAllCrafts()[0]), Is.False);
    }

    [Test]
    public void TryCraft_AllowsFullInventory_WhenRecipeFreesSpaceForResult()
    {
        ItemObject wood = CreateItem("wood");
        ItemObject stone = CreateItem("stone");
        ItemObject axe = CreateItem("axe");
        InventoryModel inventory = new InventoryModel(2, 5);
        ItemCraftStruct craft = CreateCraft(axe, 1, (wood, 5), (stone, 5));
        CraftingService crafting = new CraftingService(CreateCrafts(craft), inventory);

        inventory.TryAddItems(wood, 5);
        inventory.TryAddItems(stone, 5);

        Assert.That(crafting.CanCraft(craft), Is.True);
        Assert.That(crafting.TryCraft(craft), Is.True);
        Assert.That(inventory.CountItems(wood), Is.Zero);
        Assert.That(inventory.CountItems(stone), Is.Zero);
        Assert.That(inventory.CountItems(axe), Is.EqualTo(1));
    }

    private static ItemObject CreateItem(string itemId)
    {
        ItemObject item = ScriptableObject.CreateInstance<ItemObject>();
        item.ItemId = itemId;
        item.Name = itemId;
        return item;
    }

    private static ItemCrafts CreateCrafts(params ItemCraftStruct[] crafts)
    {
        ItemCrafts itemCrafts = ScriptableObject.CreateInstance<ItemCrafts>();
        itemCrafts.itemCrafts = new List<ItemCraftStruct>(crafts);
        return itemCrafts;
    }

    private static ItemCraftStruct CreateCraft(ItemObject result, int resultCount, params (ItemObject item, int count)[] recipe)
    {
        List<InventoryItem> recipeItems = new List<InventoryItem>();
        foreach ((ItemObject item, int count) in recipe)
        {
            recipeItems.Add(new InventoryItem(item, count));
        }

        return new ItemCraftStruct
        {
            CraftRecipe = recipeItems,
            ItemResult = new InventoryItem(result, resultCount)
        };
    }
}
