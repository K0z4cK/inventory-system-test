using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : SingletonComponent<UIManager>
{
    [Header("Inventory")]
    [FormerlySerializedAs("_inventoryPanel")]
    [SerializeField] private InventoryUI inventoryPanel;
    [FormerlySerializedAs("_showInventoryBtn")]
    [SerializeField] private Button showInventoryBtn;
    [FormerlySerializedAs("_hideInventoryBtn")]
    [SerializeField] private Button hideInventoryBtn;
    public InventoryUI InventoryPanel => inventoryPanel;

    [Header("Craft")]
    [FormerlySerializedAs("_craftPanel")]
    [SerializeField] private CraftUI craftPanel;
    [FormerlySerializedAs("_showCraftBtn")]
    [SerializeField] private Button showCraftBtn;
    [FormerlySerializedAs("_hideCraftBtn")]
    [SerializeField] private Button hideCraftBtn;
    public CraftUI CraftPanel => craftPanel;

    private void Awake()
    {
        showInventoryBtn.onClick.AddListener(inventoryPanel.ShowPanel);
        hideInventoryBtn.onClick.AddListener(inventoryPanel.HidePanel);

        showCraftBtn.onClick.AddListener(craftPanel.ShowPanel);
        hideCraftBtn.onClick.AddListener(craftPanel.HidePanel);
    }

}
