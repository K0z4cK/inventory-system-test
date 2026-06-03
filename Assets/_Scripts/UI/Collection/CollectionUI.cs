using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CollectionUI : BasePanelUI
{
    [Header("Prefabs")]
    [FormerlySerializedAs("_itemPrefab")]
    [SerializeField] private CollectionItemUI itemPrefab;

    [Header("Layout")]
    [FormerlySerializedAs("_itemsLayout")]
    [SerializeField] private Transform itemsLayout;

    private readonly List<CollectionItemUI> _itemViews = new List<CollectionItemUI>();
    private ItemDatabase _itemDatabase;

    private void Awake()
    {
        EnsureDefaultLayout();
        HidePanel();
    }

    public void Initialize(ItemDatabase itemDatabase)
    {
        _itemDatabase = itemDatabase;

        if (_itemDatabase == null)
            Debug.LogError("CollectionUI requires ItemDatabase.");
    }

    public override void ShowPanel()
    {
        base.ShowPanel();
        ShowItems();
    }

    private void ShowItems()
    {
        if (_itemDatabase == null || itemsLayout == null)
        {
            Debug.LogError("CollectionUI requires ItemDatabase and layout references.");
            return;
        }

        IReadOnlyList<ItemObject> items = _itemDatabase.Items;
        for (int i = 0; i < items.Count; i++)
        {
            if (_itemViews.Count <= i)
                _itemViews.Add(CreateItemView());

            _itemViews[i].Init(items[i]);
            _itemViews[i].gameObject.SetActive(true);
        }

        for (int i = items.Count; i < _itemViews.Count; i++)
        {
            _itemViews[i].gameObject.SetActive(false);
        }
    }

    private CollectionItemUI CreateItemView()
    {
        if (itemPrefab != null)
            return Instantiate(itemPrefab, itemsLayout);

        GameObject itemObject = new GameObject("CollectionItem", typeof(RectTransform), typeof(CollectionItemUI));
        itemObject.transform.SetParent(itemsLayout, false);
        return itemObject.GetComponent<CollectionItemUI>();
    }

    private void EnsureDefaultLayout()
    {
        RectTransform rectTransform = transform as RectTransform;
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = new Vector2(720f, 520f);
        }

        Image background = GetComponent<Image>();
        if (background == null)
            background = gameObject.AddComponent<Image>();

        background.color = new Color(0.08f, 0.09f, 0.1f, 0.96f);

        if (itemsLayout != null)
            return;

        Transform existingContent = transform.Find("ItemsLayout");
        if (existingContent != null)
        {
            itemsLayout = existingContent;
            return;
        }

        CreateHeader();
        itemsLayout = CreateItemsLayout().transform;
    }

    private void CreateHeader()
    {
        GameObject header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        header.transform.SetParent(transform, false);

        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f);
        headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(0.5f, 1f);
        headerRect.anchoredPosition = new Vector2(0f, -16f);
        headerRect.sizeDelta = new Vector2(-32f, 44f);

        HorizontalLayoutGroup layoutGroup = header.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.childAlignment = TextAnchor.MiddleLeft;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = false;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.spacing = 12f;

        TMP_Text title = CreateText("Title", header.transform, "Collection", 24f, FontStyles.Bold);
        LayoutElement titleLayout = title.gameObject.AddComponent<LayoutElement>();
        titleLayout.flexibleWidth = 1f;

        Button closeButton = CreateButton("CloseButton", header.transform, "X");
        closeButton.onClick.AddListener(HidePanel);
    }

    private GameObject CreateItemsLayout()
    {
        GameObject content = new GameObject("ItemsLayout", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        content.transform.SetParent(transform, false);

        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0f, 0f);
        contentRect.anchorMax = new Vector2(1f, 1f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = new Vector2(0f, -36f);
        contentRect.sizeDelta = new Vector2(-32f, -96f);

        VerticalLayoutGroup layoutGroup = content.GetComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 8f;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = true;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;

        ContentSizeFitter sizeFitter = content.GetComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        return content;
    }

    private TMP_Text CreateText(string objectName, Transform parent, string value, float fontSize, FontStyles fontStyle)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = Color.white;
        return text;
    }

    private Button CreateButton(string objectName, Transform parent, string value)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.22f, 0.24f, 0.26f, 1f);

        LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
        layoutElement.minWidth = 44f;
        layoutElement.preferredWidth = 44f;
        layoutElement.minHeight = 36f;
        layoutElement.preferredHeight = 36f;

        TMP_Text label = CreateText("Text", buttonObject.transform, value, 18f, FontStyles.Bold);
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;
        label.alignment = TextAlignmentOptions.Center;

        return buttonObject.GetComponent<Button>();
    }
}
