using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CollectionItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameTMP;
    [SerializeField] private TMP_Text descriptionTMP;
    [SerializeField] private TMP_Text attributesTMP;
    [SerializeField] private TMP_Text statusTMP;
    [SerializeField] private Color discoveredColor = Color.white;
    [SerializeField] private Color undiscoveredColor = new Color(0.55f, 0.55f, 0.55f, 1f);

    public void Init(ItemObject itemObject)
    {
        Init(itemObject, true);
    }

    public void Init(ItemObject itemObject, bool isDiscovered)
    {
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

        Color stateColor = isDiscovered ? discoveredColor : undiscoveredColor;
        if (icon != null)
            icon.color = stateColor;

        if (nameTMP != null)
            nameTMP.color = stateColor;

        if (descriptionTMP != null)
            descriptionTMP.color = stateColor;

        if (attributesTMP != null)
            attributesTMP.color = stateColor;

        if (statusTMP != null)
            statusTMP.text = isDiscovered ? "Discovered" : "Not discovered";
        else if (attributesTMP != null)
            attributesTMP.text = $"{itemObject.Category} | {itemObject.ItemId} | {(isDiscovered ? "Discovered" : "Not discovered")}";
    }
}
