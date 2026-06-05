using System.Collections.Generic;
using Infrastructure;
using NUnit.Framework;
using UnityEngine;

public class InventoryArchitectureTests
{
    [Test]
    public void ServiceContainer_ResolvesContractsAndRejectsDuplicateBindings()
    {
        ServiceContainer container = new ServiceContainer();
        TestService service = new TestService();

        container.Register<ITestService>(service);

        Assert.That(container.Resolve<ITestService>(), Is.SameAs(service));
        Assert.Throws<System.InvalidOperationException>(() => container.Register<ITestService>(new TestService()));

        container.Dispose();
    }

    [Test]
    public void ServiceContainer_DisposesSharedServiceOnlyOnce()
    {
        ServiceContainer container = new ServiceContainer();
        TestService service = new TestService();

        container.Register<ITestService>(service);
        container.Register<TestService>(service);
        container.Dispose();

        Assert.That(service.DisposeCount, Is.EqualTo(1));
    }

    [Test]
    public void SeparateInventoryServices_DoNotSharePlayerState()
    {
        ItemObject wood = CreateItem("wood");
        InventoryService firstPlayerInventory = new InventoryService(2, 5);
        InventoryService secondPlayerInventory = new InventoryService(2, 5);

        firstPlayerInventory.TryAddItems(wood, 3);

        Assert.That(firstPlayerInventory.CountItems(wood), Is.EqualTo(3));
        Assert.That(secondPlayerInventory.CountItems(wood), Is.Zero);

        firstPlayerInventory.Dispose();
        secondPlayerInventory.Dispose();
    }

    [Test]
    public void InventoryService_RaisesSelectedItemChangedAndTracksSelectedSlot()
    {
        ItemObject wood = CreateItem("wood");
        InventoryService inventory = new InventoryService(2, 5);
        ItemObject selectedItem = null;

        inventory.OnSelectedItemChanged += item => selectedItem = item;
        inventory.TryAddItems(wood, 1);
        inventory.SelectSlot(0);

        Assert.That(inventory.SelectedSlotIndex, Is.Zero);
        Assert.That(inventory.SelectedItem, Is.SameAs(wood));
        Assert.That(selectedItem, Is.SameAs(wood));

        inventory.TryRemoveItems(wood, 1);

        Assert.That(inventory.SelectedItem, Is.Null);
        Assert.That(selectedItem, Is.Null);
        inventory.Dispose();
    }

    [Test]
    public void LocalPlayerProvider_ResolvesServicesFromCurrentPlayerOnly()
    {
        LocalPlayerProvider provider = new LocalPlayerProvider();
        TestPlayerContext firstPlayer = new TestPlayerContext("first");
        TestPlayerContext secondPlayer = new TestPlayerContext("second");
        firstPlayer.Register<string>("first inventory");
        secondPlayer.Register<string>("second inventory");

        provider.SetLocalPlayer(firstPlayer);
        Assert.That(provider.Get<string>(), Is.EqualTo("first inventory"));

        provider.SetLocalPlayer(secondPlayer);
        Assert.That(provider.Get<string>(), Is.EqualTo("second inventory"));

        firstPlayer.Dispose();
        secondPlayer.Dispose();
    }

    [Test]
    public void WindowStaticData_ReportsDuplicateWindowTypes()
    {
        WindowConfig firstConfig = new WindowConfig();
        WindowConfig secondConfig = new WindowConfig();
        SerializedObjectUtility.SetPrivateField(firstConfig, "windowTypeId", WindowTypeId.Inventory);
        SerializedObjectUtility.SetPrivateField(secondConfig, "windowTypeId", WindowTypeId.Inventory);
        WindowStaticData staticData = ScriptableObject.CreateInstance<WindowStaticData>();
        SerializedObjectUtility.SetPrivateList(
            staticData,
            "configs",
            new List<WindowConfig> { firstConfig, secondConfig });

        Assert.That(staticData.GetValidationErrors(), Has.Some.Contains("Duplicate window config for Inventory."));
    }

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
        Assert.That(
            crafting.GetCraftAvailability(crafting.GetAllCrafts()[0]).UnavailableReason,
            Is.EqualTo(CraftUnavailableReason.MissingIngredients));
    }

    [Test]
    public void CanCraft_ReturnsFalse_WhenRecipeSelectionIsEmpty()
    {
        InventoryModel inventory = new InventoryModel(2, 5);
        CraftingService crafting = new CraftingService(CreateCrafts(), inventory);

        Assert.That(crafting.CanCraft(default), Is.False);
        Assert.That(crafting.TryCraft(default), Is.False);
        Assert.That(
            crafting.GetCraftAvailability(default).UnavailableReason,
            Is.EqualTo(CraftUnavailableReason.MissingResult));
    }

    [Test]
    public void CraftAvailability_ReportsMissingAndInvalidRecipeData()
    {
        ItemObject axe = CreateItem("axe");
        InventoryModel inventory = new InventoryModel(2, 5);
        CraftingService crafting = new CraftingService(CreateCrafts(), inventory);
        ItemCraftStruct missingRecipe = CreateCraft(axe, 1);
        ItemCraftStruct invalidIngredient = new ItemCraftStruct
        {
            CraftRecipe = new List<InventoryItem> { default },
            ItemResult = new InventoryItem(axe, 1)
        };

        Assert.That(
            crafting.GetCraftAvailability(missingRecipe).UnavailableReason,
            Is.EqualTo(CraftUnavailableReason.MissingRecipe));
        Assert.That(
            crafting.GetCraftAvailability(invalidIngredient).UnavailableReason,
            Is.EqualTo(CraftUnavailableReason.InvalidIngredient));
    }

    [Test]
    public void CraftAvailability_ReportsUnavailableInventoryAndFullResultSpace()
    {
        ItemObject wood = CreateItem("wood");
        ItemObject axe = CreateItem("axe");
        ItemCraftStruct craft = CreateCraft(axe, 5, (wood, 1));
        CraftingService unavailableCrafting = new CraftingService(CreateCrafts(craft), null);
        InventoryModel fullInventory = new InventoryModel(1, 5);
        CraftingService fullInventoryCrafting = new CraftingService(CreateCrafts(craft), fullInventory);
        fullInventory.TryAddItems(wood, 5);

        Assert.That(
            unavailableCrafting.GetCraftAvailability(craft).UnavailableReason,
            Is.EqualTo(CraftUnavailableReason.InventoryUnavailable));
        Assert.That(
            fullInventoryCrafting.GetCraftAvailability(craft).UnavailableReason,
            Is.EqualTo(CraftUnavailableReason.InventoryFull));
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
    public void WorldItemInteractions_UseSharedInteractableContract()
    {
        PickableItem pickable = new GameObject("Pickable").AddComponent<PickableItem>();
        HarvestableResource harvestable = new GameObject("Harvestable").AddComponent<HarvestableResource>();

        Assert.That(pickable, Is.AssignableTo<IInteractable>());
        Assert.That(harvestable, Is.AssignableTo<IInteractable>());
        
        Object.DestroyImmediate(pickable.gameObject);
        Object.DestroyImmediate(harvestable.gameObject);
    }    

    [Test]
    public void ItemObject_DefaultAttackDamage_IsOne()
    {
        ItemObject item = CreateItem("item");

        Assert.That(item.AttackDamage, Is.EqualTo(1));
    }

    [Test]
    public void AttackableObject_BecomesUnavailable_WhenHealthIsDepleted()
    {
        AttackableObject attackable = new GameObject("Attackable").AddComponent<AttackableObject>();
        SerializedObjectUtility.SetPrivateField(attackable, "maxHealth", 2);
        SerializedObjectUtility.SetPrivateField(attackable, "_currentHealth", 2);
        SerializedObjectUtility.SetPrivateField(attackable, "destroyWhenDefeated", false);

        attackable.ReceiveAttack(null, 1);

        Assert.That(attackable.CurrentHealth, Is.EqualTo(1));
        Assert.That(attackable.CanBeAttacked, Is.True);

        attackable.ReceiveAttack(null, 1);

        Assert.That(attackable.CurrentHealth, Is.Zero);
        Assert.That(attackable.CanBeAttacked, Is.False);

        Object.DestroyImmediate(attackable.gameObject);
    }

    [Test]
    public void AttackableObject_ReturnsCompleteResult_WhenDefeated()
    {
        AttackableObject attackable = new GameObject("Crate").AddComponent<AttackableObject>();
        SerializedObjectUtility.SetPrivateField(attackable, "maxHealth", 2);
        SerializedObjectUtility.SetPrivateField(attackable, "_currentHealth", 2);
        SerializedObjectUtility.SetPrivateField(attackable, "destroyWhenDefeated", false);

        AttackResult result = attackable.ReceiveAttack(null, 2);

        Assert.That(result.Succeeded, Is.True);
        Assert.That(result.TargetName, Is.EqualTo("Crate"));
        Assert.That(result.DamageDealt, Is.EqualTo(2));
        Assert.That(result.RemainingHealth, Is.Zero);
        Assert.That(result.MaxHealth, Is.EqualTo(2));
        Assert.That(result.Defeated, Is.True);

        Object.DestroyImmediate(attackable.gameObject);
    }

    [Test]
    public void AttackableObject_RaisesHealthChanged_WhenDamaged()
    {
        AttackableObject attackable = new GameObject("Attackable").AddComponent<AttackableObject>();
        SerializedObjectUtility.SetPrivateField(attackable, "maxHealth", 3);
        SerializedObjectUtility.SetPrivateField(attackable, "_currentHealth", 3);
        int receivedCurrentHealth = -1;
        int receivedMaxHealth = -1;

        attackable.OnHealthChanged += (currentHealth, maxHealth) =>
        {
            receivedCurrentHealth = currentHealth;
            receivedMaxHealth = maxHealth;
        };

        attackable.ReceiveAttack(null, 1);

        Assert.That(receivedCurrentHealth, Is.EqualTo(2));
        Assert.That(receivedMaxHealth, Is.EqualTo(3));

        Object.DestroyImmediate(attackable.gameObject);
    }

    [Test]
    public void EnemyAndBreakableObject_UseSharedAttackableContract()
    {
        Enemy enemy = new GameObject("Enemy").AddComponent<Enemy>();
        BreakableObject breakable = new GameObject("Breakable").AddComponent<BreakableObject>();

        Assert.That(enemy, Is.AssignableTo<IAttackable>());
        Assert.That(breakable, Is.AssignableTo<IAttackable>());

        Object.DestroyImmediate(enemy.gameObject);
        Object.DestroyImmediate(breakable.gameObject);
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

    [Test]
    public void GameplayFeedbackService_RaisesMessage_WhenPickupFeedbackRequested()
    {
        ItemObject wood = CreateItem("wood");
        GameplayFeedbackService feedbackService = new GameplayFeedbackService();
        string receivedMessage = null;

        feedbackService.OnMessageRaised += message => receivedMessage = message;
        feedbackService.ShowPickedUp(wood, 2);

        Assert.That(receivedMessage, Is.EqualTo("Picked up wood x2"));
    }

    [Test]
    public void GameplayFeedbackService_RaisesStructuredFailure_WhenToolIsRequired()
    {
        ItemObject axe = CreateItem("axe");
        GameplayFeedbackService feedbackService = new GameplayFeedbackService();
        GameplayFeedbackMessage receivedMessage = default;

        feedbackService.OnFeedbackRaised += message => receivedMessage = message;
        feedbackService.ShowRequiredTool(axe);

        Assert.That(receivedMessage.Text, Is.EqualTo("Requires axe"));
        Assert.That(receivedMessage.Kind, Is.EqualTo(GameplayFeedbackKind.Failure));
    }

    [Test]
    public void GameplayFeedbackService_DescribesCombatResult()
    {
        GameplayFeedbackService feedbackService = new GameplayFeedbackService();
        GameplayFeedbackMessage receivedMessage = default;

        feedbackService.OnFeedbackRaised += message => receivedMessage = message;
        feedbackService.ShowDamageDealt("Crate", 3, 2, 5);

        Assert.That(receivedMessage.Text, Is.EqualTo("Hit Crate for 3 damage (2/5)"));
        Assert.That(receivedMessage.Kind, Is.EqualTo(GameplayFeedbackKind.Success));
    }

    [Test]
    public void CollectionProgressService_DiscoversEachItemIdOnlyOnce()
    {
        ItemObject wood = CreateItem("wood");
        InventoryModel inventory = new InventoryModel(2, 5);
        CollectionProgressService progressService = new CollectionProgressService(CreateDatabase(wood), inventory);
        int discoveryCount = 0;

        progressService.OnItemDiscovered += (_, _, _) => discoveryCount++;

        inventory.TryAddItems(wood, 1);
        inventory.TryAddItems(wood, 1);

        Assert.That(progressService.IsDiscovered(wood), Is.True);
        Assert.That(progressService.DiscoveredCount, Is.EqualTo(1));
        Assert.That(discoveryCount, Is.EqualTo(1));

        progressService.Dispose();
    }

    [Test]
    public void CollectionProgressService_ReachesMasterCollector_WhenAllItemsAreDiscovered()
    {
        ItemObject wood = CreateItem("wood");
        ItemObject stone = CreateItem("stone");
        InventoryModel inventory = new InventoryModel(2, 5);
        CollectionProgressService progressService = new CollectionProgressService(CreateDatabase(wood, stone), inventory);
        string lastRank = null;

        progressService.OnMilestoneReached += (rank, _, _) => lastRank = rank;

        inventory.TryAddItems(wood, 1);
        inventory.TryAddItems(stone, 1);

        Assert.That(progressService.DiscoveredCount, Is.EqualTo(2));
        Assert.That(progressService.TotalCount, Is.EqualTo(2));
        Assert.That(progressService.CurrentRank, Is.EqualTo("Master Collector"));
        Assert.That(lastRank, Is.EqualTo("Master Collector"));

        progressService.Dispose();
    }

    private static ItemObject CreateItem(string itemId)
    {
        ItemObject item = ScriptableObject.CreateInstance<ItemObject>();
        item.ItemId = itemId;
        item.Name = itemId;
        return item;
    }

    private static ItemDatabase CreateDatabase(params ItemObject[] items)
    {
        ItemDatabase itemDatabase = ScriptableObject.CreateInstance<ItemDatabase>();
        SerializedObjectUtility.SetPrivateList(itemDatabase, "items", new List<ItemObject>(items));
        return itemDatabase;
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

        public static void SetPrivateField<T>(object target, string fieldName, T value)
        {
            target.GetType()
                .GetField(fieldName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(target, value);
        }
    }

    private interface ITestService
    {
    }

    private sealed class TestService : ITestService, System.IDisposable
    {
        public int DisposeCount { get; private set; }

        public void Dispose()
        {
            DisposeCount++;
        }
    }

    private sealed class TestPlayerContext : IPlayerContext
    {
        private readonly ServiceContainer _services = new ServiceContainer();

        public string PlayerId { get; }
        public bool IsLocalPlayer => true;
        public bool IsInitialized => true;

        public TestPlayerContext(string playerId)
        {
            PlayerId = playerId;
        }

        public void Register<TService>(TService service)
        {
            _services.Register(service);
        }

        public TService Get<TService>()
        {
            return _services.Resolve<TService>();
        }

        public bool TryGet<TService>(out TService service)
        {
            return _services.TryResolve(out service);
        }

        public void Dispose()
        {
            _services.Dispose();
        }
    }
}
