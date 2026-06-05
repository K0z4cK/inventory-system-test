using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
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
