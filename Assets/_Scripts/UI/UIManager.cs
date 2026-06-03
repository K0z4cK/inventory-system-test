using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private InventoryUI inventoryPanel;
    [SerializeField] private Button showInventoryBtn;
    [SerializeField] private Button hideInventoryBtn;
    public InventoryUI InventoryPanel => inventoryPanel;

    [Header("Craft")]
    [SerializeField] private CraftUI craftPanel;
    [SerializeField] private Button showCraftBtn;
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
            showCollectionBtn.onClick.AddListener(() =>
            {
                collectionPanel.ShowPanel();
                showCollectionBtn.gameObject.SetActive(false);
            });
        if (hideCollectionBtn != null && collectionPanel != null)
            hideCollectionBtn.onClick.AddListener(() =>
            {
                collectionPanel.HidePanel();
                showCollectionBtn.gameObject.SetActive(true);
            });

        if (collectionPanel != null && showCollectionBtn == null)
            Debug.LogWarning("UIManager has CollectionPanel but no scene reference for Show Collection button.");
    }
}
