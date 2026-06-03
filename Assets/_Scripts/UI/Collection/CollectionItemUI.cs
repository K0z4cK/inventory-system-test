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
}
