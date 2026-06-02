using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftUI : BasePanelUI
{
    [Header("Prefabs")]
    [SerializeField] private CraftItemUI _craftItemPrefab;

    [Header("Crafts Layout")]
    [SerializeField] private Transform _craftsLayout;

    [Header("Recipe Objects")]
    [SerializeField] private List<ItemUI> _recipeItems;
    [SerializeField] private Button _craftButton;

    private List<CraftItemUI> _craftItems = new List<CraftItemUI>();
    private CraftItemUI _selectedItem;

    private bool _isShowAllCrafts = true;

    public override void ShowPanel()
    {
        base.ShowPanel();
        ShowCrafts();
        _recipeItems.ForEach(item => {item.gameObject.SetActive(false); });
        _craftButton.interactable = false;
    }

    public override void HidePanel()
    {
        base.HidePanel();
        _selectedItem.SetUnselectedColor();
        _selectedItem = null;
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
        List<ItemCraftStruct> craftsToShow = new List<ItemCraftStruct>();
        if (_isShowAllCrafts)
            craftsToShow = CraftManager.Instance.GetAllCrafts();
        else
            craftsToShow = CraftManager.Instance.GetAvalibleCrafts();

        for(int i = 0; i < craftsToShow.Count; i++)
        {
            if(_craftItems.Count <= i)
            {
               var newCraftItem = Instantiate(_craftItemPrefab, _craftsLayout);
                _craftItems.Add(newCraftItem);
            }
            _craftItems[i].Init(craftsToShow[i], ShowCraftRecipe);
        }
    }

    private void ShowCraftRecipe(ItemCraftStruct itemCraft, CraftItemUI itemUI)
    {
        SelectCraftItem(itemUI);

        _craftButton.onClick.RemoveAllListeners();
        _craftButton.onClick.AddListener(delegate { CraftManager.Instance.CraftItem(itemCraft); ShowCraftRecipe(itemCraft, itemUI); });

        _recipeItems.ForEach(recipeItem => recipeItem.gameObject.SetActive(false));

        var avalibleRecipeItems = CraftManager.Instance.GetCraftItems(itemCraft.CraftRecipe);

        for(int i = 0; i < itemCraft.CraftRecipe.Count; i++)
        {
            _recipeItems[i].gameObject.SetActive(true);
            _recipeItems[i].SetItem(itemCraft.CraftRecipe[i]);
            if (avalibleRecipeItems.Find(x => x.ItemObject == itemCraft.CraftRecipe[i].ItemObject).ItemObject != null)
                _recipeItems[i].SetTextColor(Color.green);
            else
                _recipeItems[i].SetTextColor(Color.red);
        }

        if(itemCraft.CraftRecipe.Count == avalibleRecipeItems.Count)
            _craftButton.interactable = true;
        else
            _craftButton.interactable = false;
    }
}
