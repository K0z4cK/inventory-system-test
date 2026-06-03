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

    [Header("Collection")]
    [SerializeField] private CollectionUI collectionPanel;
    [SerializeField] private Button showCollectionBtn;
    [SerializeField] private Button hideCollectionBtn;
    public CollectionUI CollectionPanel => collectionPanel;

    private void Awake()
    {
        if (showInventoryBtn != null && inventoryPanel != null)
            showInventoryBtn.onClick.AddListener(inventoryPanel.ShowPanel);
        if (hideInventoryBtn != null && inventoryPanel != null)
            hideInventoryBtn.onClick.AddListener(inventoryPanel.HidePanel);

        if (showCraftBtn != null && craftPanel != null)
            showCraftBtn.onClick.AddListener(craftPanel.ShowPanel);
        if (hideCraftBtn != null && craftPanel != null)
            hideCraftBtn.onClick.AddListener(craftPanel.HidePanel);

        if (showCollectionBtn != null && collectionPanel != null)
            showCollectionBtn.onClick.AddListener(collectionPanel.ShowPanel);
        if (hideCollectionBtn != null && collectionPanel != null)
            hideCollectionBtn.onClick.AddListener(collectionPanel.HidePanel);
    }

}
