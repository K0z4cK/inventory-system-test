using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class CollectionItemUI : MonoBehaviour
{
    [FormerlySerializedAs("_icon")]
    [SerializeField] private Image icon;
    [FormerlySerializedAs("_nameTMP")]
    [SerializeField] private TMP_Text nameTMP;
    [FormerlySerializedAs("_descriptionTMP")]
    [SerializeField] private TMP_Text descriptionTMP;
    [FormerlySerializedAs("_attributesTMP")]
    [SerializeField] private TMP_Text attributesTMP;

    public void Init(ItemObject itemObject)
    {
        EnsureComponents();

        if (itemObject == null)
        {
            gameObject.SetActive(false);
            return;
        }

        if (icon != null)
            icon.sprite = itemObject.Sprite;

        if (nameTMP != null)
            nameTMP.text = string.IsNullOrWhiteSpace(itemObject.Name) ? itemObject.ItemId : itemObject.Name;

        if (descriptionTMP != null)
            descriptionTMP.text = itemObject.Description;

        if (attributesTMP != null)
            attributesTMP.text = $"{itemObject.Category} | {itemObject.ItemId}";
    }

    private void EnsureComponents()
    {
        if (icon != null && nameTMP != null && descriptionTMP != null && attributesTMP != null)
            return;

        Image background = GetComponent<Image>();
        if (background == null)
            background = gameObject.AddComponent<Image>();

        background.color = new Color(0.16f, 0.16f, 0.16f, 0.95f);

        HorizontalLayoutGroup horizontalLayoutGroup = GetComponent<HorizontalLayoutGroup>();
        if (horizontalLayoutGroup == null)
            horizontalLayoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();

        horizontalLayoutGroup.padding = new RectOffset(12, 12, 8, 8);
        horizontalLayoutGroup.spacing = 12f;
        horizontalLayoutGroup.childAlignment = TextAnchor.MiddleLeft;
        horizontalLayoutGroup.childControlWidth = false;
        horizontalLayoutGroup.childControlHeight = true;
        horizontalLayoutGroup.childForceExpandWidth = false;
        horizontalLayoutGroup.childForceExpandHeight = false;

        LayoutElement layoutElement = GetComponent<LayoutElement>();
        if (layoutElement == null)
            layoutElement = gameObject.AddComponent<LayoutElement>();

        layoutElement.minHeight = 84f;
        layoutElement.preferredHeight = 84f;

        if (icon == null)
            icon = CreateIcon();

        Transform textRoot = transform.Find("Text");
        if (textRoot == null)
            textRoot = CreateTextRoot().transform;

        if (nameTMP == null)
            nameTMP = CreateText("Name", textRoot, 20f, FontStyles.Bold);

        if (descriptionTMP == null)
            descriptionTMP = CreateText("Description", textRoot, 15f, FontStyles.Normal);

        if (attributesTMP == null)
            attributesTMP = CreateText("Attributes", textRoot, 13f, FontStyles.Italic);
    }

    private Image CreateIcon()
    {
        GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(LayoutElement));
        iconObject.transform.SetParent(transform, false);

        LayoutElement layoutElement = iconObject.GetComponent<LayoutElement>();
        layoutElement.minWidth = 56f;
        layoutElement.preferredWidth = 56f;
        layoutElement.minHeight = 56f;
        layoutElement.preferredHeight = 56f;

        Image image = iconObject.GetComponent<Image>();
        image.preserveAspect = true;
        image.color = Color.white;
        return image;
    }

    private GameObject CreateTextRoot()
    {
        GameObject textRoot = new GameObject("Text", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        textRoot.transform.SetParent(transform, false);

        VerticalLayoutGroup verticalLayoutGroup = textRoot.GetComponent<VerticalLayoutGroup>();
        verticalLayoutGroup.spacing = 2f;
        verticalLayoutGroup.childControlWidth = true;
        verticalLayoutGroup.childControlHeight = true;
        verticalLayoutGroup.childForceExpandWidth = true;
        verticalLayoutGroup.childForceExpandHeight = false;

        LayoutElement layoutElement = textRoot.GetComponent<LayoutElement>();
        layoutElement.flexibleWidth = 1f;

        return textRoot;
    }

    private TMP_Text CreateText(string objectName, Transform parent, float fontSize, FontStyles fontStyle)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.color = Color.white;
        return text;
    }
}
