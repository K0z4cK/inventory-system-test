using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [FormerlySerializedAs("_icon")]
    [SerializeField] private Image icon;
    [FormerlySerializedAs("_countTMP")]
    [SerializeField] private TMP_Text countTMP;

    public void SetItem(InventoryItem item)
    {
        if (item.IsEmpty)
        {
            icon.sprite = null;
            countTMP.text = string.Empty;
            return;
        }

        icon.sprite = item.ItemObject.Sprite;
        countTMP.text = item.Count.ToString();
    }

    public void SetTextColor(Color color) => countTMP.color = color;
}
