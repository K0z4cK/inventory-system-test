using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _countTMP;

    public void SetItem(InventoryItem item)
    {
        _icon.sprite = item.ItemObject.Sprite;
        _countTMP.text = item.Count.ToString();
    }

    public void SetTextColor(Color color) => _countTMP.color = color;
}
