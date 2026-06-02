using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CraftUI : BasePanelUI
{
    [Header("Prefabs")]
    [FormerlySerializedAs("_craftItemPrefab")]
    [SerializeField] private CraftItemUI craftItemPrefab;

    [Header("Services")]
    [SerializeField] private ItemCrafts itemCrafts;
    [SerializeField] private InventorySystem inventorySystem;

    [Header("Crafts Layout")]
    [FormerlySerializedAs("_craftsLayout")]
    [SerializeField] private Transform craftsLayout;

    [Header("Recipe Objects")]
    [FormerlySerializedAs("_recipeItems")]
    [SerializeField] private List<ItemUI> recipeItems;
    [FormerlySerializedAs("_craftButton")]
    [SerializeField] private Button craftButton;

    private List<CraftItemUI> _craftItems = new List<CraftItemUI>();
    private CraftItemUI _selectedItem;
    private ItemCraftStruct? _selectedCraft;
    private CraftingService _craftingService;

    private bool _isShowAllCrafts = true;

    private void Awake()
    {
        ResolveDependencies();
        _craftingService = new CraftingService(itemCrafts, inventorySystem);

        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged += RefreshCraftState;
    }

    public override void ShowPanel()
    {
        base.ShowPanel();
        ShowCrafts();
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

        for(int i = 0; i < craftsToShow.Count; i++)
        {
            if(_craftItems.Count <= i)
            {
               var newCraftItem = Instantiate(craftItemPrefab, craftsLayout);
                _craftItems.Add(newCraftItem);
            }
            _craftItems[i].Init(craftsToShow[i], ShowCraftRecipe);
            _craftItems[i].gameObject.SetActive(true);
        }

        for (int i = craftsToShow.Count; i < _craftItems.Count; i++)
        {
            _craftItems[i].gameObject.SetActive(false);
        }
    }

    private void ShowCraftRecipe(ItemCraftStruct itemCraft, CraftItemUI itemUI)
    {
        if (_craftingService == null)
            return;

        SelectCraftItem(itemUI);
        _selectedCraft = itemCraft;

        craftButton.onClick.RemoveAllListeners();
        craftButton.onClick.AddListener(delegate
        {
            _craftingService.TryCraft(itemCraft);
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

        craftButton.interactable = _craftingService.CanCraft(itemCraft);
    }

    private void RefreshCraftState()
    {
        ShowCrafts();

        if (_selectedCraft.HasValue && _selectedItem != null)
            ShowCraftRecipe(_selectedCraft.Value, _selectedItem);
    }

    private void ResolveDependencies()
    {
        if (itemCrafts == null)
            itemCrafts = Resources.Load<ItemCrafts>("ItemCrafts");

        if (itemCrafts == null)
            Debug.LogError("CraftUI requires ItemCrafts. Assign it in the inspector or place ItemCrafts.asset in a Resources folder.");

        if (inventorySystem == null)
            inventorySystem = FindFirstObjectByType<InventorySystem>();

        if (inventorySystem == null)
            Debug.LogError("CraftUI requires InventorySystem.");
    }

    private void OnDestroy()
    {
        if (inventorySystem != null)
            inventorySystem.OnInventoryChanged -= RefreshCraftState;
    }
}
