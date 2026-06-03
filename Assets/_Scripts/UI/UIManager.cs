using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
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
        EnsureCollectionButton();

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

    private void EnsureCollectionButton()
    {
        if (showCollectionBtn != null || collectionPanel == null)
            return;

        Transform parent = transform.Find("HUD") != null ? transform.Find("HUD") : transform;
        showCollectionBtn = CreateButton("ButtonCollection", parent, "Items", new Vector2(-20f, 532f));
    }

    private Button CreateButton(string objectName, Transform parent, string value, Vector2 anchoredPosition)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(1f, 0f);
        rectTransform.anchorMax = new Vector2(1f, 0f);
        rectTransform.pivot = new Vector2(1f, 0f);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = new Vector2(150f, 96f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = Color.white;

        Button button = buttonObject.GetComponent<Button>();

        GameObject labelObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);

        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TMP_Text label = labelObject.GetComponent<TMP_Text>();
        label.text = value;
        label.fontSize = 24f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.black;

        return button;
    }
}
