using System;
using System.Collections.Generic;
using UnityEngine;

public class ItemsHolder : MonoBehaviour
{
    [SerializeField] private List<HolderItem> _itemsToHold = new List<HolderItem>();

    private GameObject _currentItem;

    public void SetNewItem(ItemType type)
    {
        if(_currentItem != null)
            _currentItem.SetActive(false);
        _currentItem = _itemsToHold.Find(x=> x.Type == type).Item;
        _currentItem.SetActive(true);
    }
}

[Serializable]
public struct HolderItem 
{
    public ItemType Type;
    public GameObject Item;
}
