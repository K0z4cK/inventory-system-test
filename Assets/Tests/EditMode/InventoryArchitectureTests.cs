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
    public void InventoryItems_DoesNotExposeMutableInternalArray()
    {
        ItemObject wood = CreateItem("wood");
        InventoryModel inventory = new InventoryModel(1, 5);

        inventory.TryAddItems(wood, 1);

        Assert.That(inventory.InventoryItems, Is.Not.TypeOf<InventoryItem[]>());
        Assert.That(inventory.InventoryItems[0].Count, Is.EqualTo(1));
    }

    [Test]
    public void TryAddItems_FillsPartialStackAndCreatesNewStack_WhenThereIsEnoughSpace()
    {
        ItemObject wood = CreateItem("wood");
        InventoryModel inventory = new InventoryModel(2, 5);
        List<int> changedSlots = new List<int>();

        inventory.OnSlotChanged += (slot, _) => changedSlots.Add(slot);

        Assert.That(inventory.TryAddItems(wood, 4), Is.True);
        Assert.That(inventory.TryAddItems(wood, 3), Is.True);

        Assert.That(inventory.InventoryItems[0].Count, Is.EqualTo(5));
        Assert.That(inventory.InventoryItems[1].Count, Is.EqualTo(2));
        Assert.That(inventory.CountItems(wood), Is.EqualTo(7));
        Assert.That(changedSlots, Does.Contain(0));
        Assert.That(changedSlots, Does.Contain(1));
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
    public void CanCraft_ReturnsFalse_WhenRecipeSelectionIsEmpty()
    {
        InventoryModel inventory = new InventoryModel(2, 5);
        CraftingService crafting = new CraftingService(CreateCrafts(), inventory);

        Assert.That(crafting.CanCraft(default), Is.False);
        Assert.That(crafting.TryCraft(default), Is.False);
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

    [Test]
    public void PickableItem_ReturnsPickupResult_FromInventoryListener()
    {
        PickableItem pickable = new GameObject("Pickable").AddComponent<PickableItem>();
        pickable.SubscribeOnItemPickUp((_, _, _) => false);

        Assert.That(pickable.PickUp(), Is.False);

        pickable.SubscribeOnItemPickUp((_, _, _) => true);

        Assert.That(pickable.PickUp(), Is.True);

        Object.DestroyImmediate(pickable.gameObject);
    }

    [Test]
    public void ItemDatabaseValidation_ReportsDuplicateItemIds()
    {
        ItemObject firstWood = CreateItem("wood");
        ItemObject secondWood = CreateItem("wood");
        ItemDatabase itemDatabase = ScriptableObject.CreateInstance<ItemDatabase>();
        SerializedObjectUtility.SetPrivateList(itemDatabase, "items", new List<ItemObject> { firstWood, secondWood });

        Assert.That(itemDatabase.GetValidationErrors(), Has.Some.Contains("Duplicate ItemId 'wood'"));
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

    private static class SerializedObjectUtility
    {
        public static void SetPrivateList<T>(object target, string fieldName, List<T> value)
        {
            target.GetType()
                .GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(target, value);
        }
    }
}
