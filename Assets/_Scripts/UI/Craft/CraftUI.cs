using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CraftUI : BasePanelUI
{
    [Header("Prefabs")]
    [FormerlySerializedAs("_craftItemPrefab")]
    [SerializeField] private CraftItemUI craftItemPrefab;

    [Header("Crafts Layout")]
    [FormerlySerializedAs("_craftsLayout")]
    [SerializeField] private Transform craftsLayout;

    [Header("Recipe Objects")]
    [FormerlySerializedAs("_recipeItems")]
    [SerializeField] private List<ItemUI> recipeItems;
    [FormerlySerializedAs("_craftButton")]
    [SerializeField] private Button craftButton;

    private readonly List<CraftItemUI> _craftItems = new List<CraftItemUI>();
    private CraftItemUI _selectedItem;
    private ItemCraftStruct? _selectedCraft;
    private CraftingService _craftingService;
    private IInventory _inventory;
    private IGameplayFeedback _feedback;
    private ItemCrafts _itemCrafts;

    private bool _isShowAllCrafts = true;

    private void Awake()
    {
        HidePanel();
    }

    public void Initialize(ItemCrafts crafts, IInventory inventory, IGameplayFeedback feedback)
    {
        if (_inventory != null)
            _inventory.OnInventoryChanged -= RefreshCraftState;

        _itemCrafts = crafts;
        _inventory = inventory;
        _feedback = feedback;
        _craftingService = new CraftingService(_itemCrafts, _inventory);

        if (_inventory != null)
            _inventory.OnInventoryChanged += RefreshCraftState;
        else
            Debug.LogError("CraftUI requires IInventory.");

        if (_itemCrafts == null)
            Debug.LogError("CraftUI requires ItemCrafts.");
    }

    public override void ShowPanel()
    {
        base.ShowPanel();
        ShowCrafts();
        if (_craftingService != null && _craftingService.GetAllCrafts().Count == 0)
            _feedback?.ShowNoCraftsAvailable();
        else
            _feedback?.ShowCraftSelectionRequired();

        recipeItems.ForEach(item => { item.gameObject.SetActive(false); });
        craftButton.interactable = false;
    }

    public override void HidePanel()
    {
        base.HidePanel();
        if (_selectedItem != null)
            _selectedItem.SetUnselectedColor();

        _selectedItem = null;
        _selectedCraft = null;
    }

    private void SelectCraftItem(CraftItemUI craftItemUI)
    {
        if (_selectedItem != null)
            _selectedItem.SetUnselectedColor();
        _selectedItem = craftItemUI;
        _selectedItem.SetSelectedColor();
    }

    private void ShowCrafts()
    {
        if (_craftingService == null)
            return;

        List<ItemCraftStruct> craftsToShow = _isShowAllCrafts
            ? _craftingService.GetAllCrafts()
            : _craftingService.GetAvailableCrafts();

        if (craftsToShow.Count == 0)
        {
            Debug.LogWarning("Craft UI has no crafts to show. Check ItemCrafts reference and recipes.");
        }

        if (craftItemPrefab == null || craftsLayout == null)
        {
            Debug.LogError("CraftUI requires CraftItemUI prefab and crafts layout references.");
            return;
        }

        for(int i = 0; i < craftsToShow.Count; i++)
        {
            if(_craftItems.Count <= i)
            {
               var newCraftItem = Instantiate(craftItemPrefab, craftsLayout);
                _craftItems.Add(newCraftItem);
            }
            _craftItems[i].Init(craftsToShow[i], (itemCraft, itemUI) => ShowCraftRecipe(itemCraft, itemUI, true));
            _craftItems[i].gameObject.SetActive(true);
        }

        for (int i = craftsToShow.Count; i < _craftItems.Count; i++)
        {
            _craftItems[i].gameObject.SetActive(false);
        }
    }

    private void ShowCraftRecipe(ItemCraftStruct itemCraft, CraftItemUI itemUI, bool showUnavailableFeedback = false)
    {
        if (_craftingService == null)
            return;

        SelectCraftItem(itemUI);
        _selectedCraft = itemCraft;

        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(delegate
        {
            bool crafted = _craftingService.TryCraft(itemCraft);
            if (crafted)
                _feedback?.ShowCraftSucceeded(itemCraft.ItemResult.ItemObject, itemCraft.ItemResult.Count);
            else
                _feedback?.ShowCraftUnavailable(GetCraftUnavailableReason(itemCraft));

            ShowCraftRecipe(itemCraft, itemUI);
        });

        recipeItems.ForEach(recipeItem => recipeItem.gameObject.SetActive(false));

        var availableRecipeItems = _craftingService.GetCraftItems(itemCraft.CraftRecipe);

        for(int i = 0; i < itemCraft.CraftRecipe.Count; i++)
        {
            if (i >= recipeItems.Count)
            {
                Debug.LogWarning("Craft UI does not have enough recipe item views.");
                break;
            }

            recipeItems[i].gameObject.SetActive(true);
            recipeItems[i].SetItem(itemCraft.CraftRecipe[i]);
            if (availableRecipeItems.Exists(x => x.Matches(itemCraft.CraftRecipe[i].ItemObject)))
                recipeItems[i].SetTextColor(Color.green);
            else
                recipeItems[i].SetTextColor(Color.red);
        }

        bool canCraft = _craftingService.CanCraft(itemCraft);
        craftButton.interactable = canCraft;

        if (!canCraft && showUnavailableFeedback)
            _feedback?.ShowCraftUnavailable(GetCraftUnavailableReason(itemCraft));
    }

    private string GetCraftUnavailableReason(ItemCraftStruct itemCraft)
    {
        CraftAvailability availability = _craftingService.GetCraftAvailability(itemCraft);
        switch (availability.UnavailableReason)
        {
            case CraftUnavailableReason.InventoryUnavailable:
                return "Cannot craft: inventory is unavailable";
            case CraftUnavailableReason.MissingResult:
                return "Cannot craft: recipe has no result item";
            case CraftUnavailableReason.MissingRecipe:
                return "Cannot craft: recipe has no ingredients";
            case CraftUnavailableReason.InvalidIngredient:
                return "Cannot craft: recipe contains an invalid ingredient";
            case CraftUnavailableReason.MissingIngredients:
                return $"Missing {GetItemName(availability.MissingItem.ItemObject)} x{availability.MissingItem.Count}";
            case CraftUnavailableReason.InventoryFull:
                return "Cannot craft: inventory has no space for result";
            default:
                return "Cannot craft selected item";
        }
    }

    private string GetItemName(ItemObject itemObject)
    {
        if (itemObject == null)
            return "item";

        return string.IsNullOrWhiteSpace(itemObject.Name) ? itemObject.ItemId : itemObject.Name;
    }

    private void RefreshCraftState()
    {
        ShowCrafts();

        if (_selectedCraft.HasValue && _selectedItem != null)
            ShowCraftRecipe(_selectedCraft.Value, _selectedItem);
    }

    private void OnDestroy()
    {
        if (_inventory != null)
            _inventory.OnInventoryChanged -= RefreshCraftState;
    }
}
