using System.Collections.Generic;
using UnityEngine;

public class ItemsHolder : MonoBehaviour
{
    [SerializeField] private List<HeldItemView> heldItems = new List<HeldItemView>();

    private HeldItemView _currentItem;
    private IInventorySlotSelector _slotSelector;

    private void Awake()
    {
        RegisterHeldItemsIfNeeded();
        ClearCurrentItem();
    }

    public void Bind(IInventorySlotSelector slotSelector)
    {
        if (_slotSelector != null)
            _slotSelector.OnSelectedItemChanged -= SetNewItem;

        _slotSelector = slotSelector;
        if (_slotSelector != null)
        {
            _slotSelector.OnSelectedItemChanged += SetNewItem;
            SetNewItem(_slotSelector.SelectedItem);
        }
        else
        {
            ClearCurrentItem();
        }
    }

    public void SetNewItem(ItemObject itemObject)
    {
        HeldItemView nextItem = FindHeldItem(itemObject);
        if (nextItem == null)
        {
            ClearCurrentItem();
            return;
        }

        if (_currentItem == nextItem)
            return;

        ClearCurrentItem();
        _currentItem = nextItem;
        _currentItem.ViewObject.SetActive(true);
    }

    private void ClearCurrentItem()
    {
        if (_currentItem == null)
        {
            foreach (HeldItemView heldItem in heldItems)
            {
                if (heldItem != null && heldItem.ViewObject != null)
                    heldItem.ViewObject.SetActive(false);
            }

            return;
        }

        if (_currentItem.ViewObject != null)
            _currentItem.ViewObject.SetActive(false);
        _currentItem = null;
    }

    private HeldItemView FindHeldItem(ItemObject itemObject)
    {
        if (itemObject == null)
            return null;

        foreach (HeldItemView heldItem in heldItems)
        {
            if (heldItem == null || heldItem.ItemObject == null)
                continue;

            if (new InventoryItem(heldItem.ItemObject, 1).Matches(itemObject))
                return heldItem;
        }

        return null;
    }

    private void RegisterHeldItemsIfNeeded()
    {
        if (heldItems.Count > 0)
            return;

        heldItems.AddRange(GetComponentsInChildren<HeldItemView>(true));
    }

    private void OnDestroy()
    {
        if (_slotSelector != null)
            _slotSelector.OnSelectedItemChanged -= SetNewItem;
    }
}
