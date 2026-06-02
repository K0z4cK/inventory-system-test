using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class CraftItemUI : MonoBehaviour
{
    [FormerlySerializedAs("_icon")]
    [SerializeField] private Image icon;
    [FormerlySerializedAs("_nameTMP")]
    [SerializeField] private TMP_Text nameTMP;
    [FormerlySerializedAs("_descriptionTMP")]
    [SerializeField] private TMP_Text descriptionTMP;

    [FormerlySerializedAs("_selectedColor")]
    [SerializeField] private Color selectedColor;
    [FormerlySerializedAs("_unselectedColor")]
    [SerializeField] private Color unselectedColor;

    private Image _image;

    private Button _button;

    private void Awake()
    {
        EnsureComponents();
        _image.color = unselectedColor;
    }

    public void Init(ItemCraftStruct itemCraft, Action<ItemCraftStruct, CraftItemUI> onClick)
    {
        EnsureComponents();

        if (itemCraft.ItemResult.IsEmpty)
        {
            gameObject.SetActive(false);
            return;
        }

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(delegate { onClick?.Invoke(itemCraft, this);});

        if (icon != null)
            icon.sprite = itemCraft.ItemResult.ItemObject.Sprite;

        if (nameTMP != null)
            nameTMP.text = itemCraft.ItemResult.ItemObject.Name;

        if (descriptionTMP != null)
            descriptionTMP.text = itemCraft.ItemResult.ItemObject.Description;
    }

    public void SetSelectedColor()
    {
        EnsureComponents();
        _image.color = selectedColor;
    }

    public void SetUnselectedColor()
    {
        EnsureComponents();
        _image.color = unselectedColor;
    }

    private void EnsureComponents()
    {
        if (_image == null)
            _image = GetComponent<Image>();

        if (_button == null)
            _button = GetComponent<Button>();
    }
}
