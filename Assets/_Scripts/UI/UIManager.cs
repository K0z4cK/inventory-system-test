using UnityEngine;
using UnityEngine.UI;

public class UIManager : SingletonComponent<UIManager>
{
    [Header("Inventory")]
    [SerializeField] private InventoryUI _inventoryPanel;
    [SerializeField] private Button _showInventoryBtn;
    [SerializeField] private Button _hideInventoryBtn;
    public InventoryUI InventoryPanel => _inventoryPanel;

    [Header("Craft")]
    [SerializeField] private CraftUI _craftPanel;
    [SerializeField] private Button _showCraftBtn;
    [SerializeField] private Button _hideCraftBtn;
    public CraftUI CraftPanel => _craftPanel;

    private void Awake()
    {
        _showInventoryBtn.onClick.AddListener(_inventoryPanel.ShowPanel);
        _hideInventoryBtn.onClick.AddListener(_inventoryPanel.HidePanel);

        _showCraftBtn.onClick.AddListener(_craftPanel.ShowPanel);
        _hideCraftBtn.onClick.AddListener(_craftPanel.HidePanel);
    }

}
