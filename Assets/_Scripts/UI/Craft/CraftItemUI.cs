using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CraftItemUI : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _nameTMP;
    [SerializeField] private TMP_Text _descriptionTMP;

    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _unselectedColor;

    private Image _image;

    private Button _button;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _button = GetComponent<Button>();
        _image.color = _unselectedColor;
    }

    public void Init(ItemCraftStruct itemCraft, Action<ItemCraftStruct, CraftItemUI> onClick)
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(delegate { onClick?.Invoke(itemCraft, this);});

        _icon.sprite = itemCraft.ItemResult.ItemObject.Sprite;
        _nameTMP.text = itemCraft.ItemResult.ItemObject.Name;
        _descriptionTMP.text = itemCraft.ItemResult.ItemObject.Description;
    }

    public void SetSelectedColor() => _image.color = _selectedColor;
    public void SetUnselectedColor() => _image.color = _unselectedColor;
}
