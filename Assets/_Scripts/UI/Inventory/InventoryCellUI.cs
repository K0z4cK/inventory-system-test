using System;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class InventoryCellUI : MonoBehaviour
{
    public event Action<Vector3, InventoryCellUI> OnItemPositionChanged;
    public event Action<InventoryCellUI> OnItemClick;

    [SerializeField] private Image selectionImage;

    private DraggableItemUI _itemUI;
    public bool HasItem => _itemUI != null;

    public void Init(Action<Vector3, InventoryCellUI> onItemPositionChanged)
    {
        OnItemPositionChanged = onItemPositionChanged;
    }

    public void SetItem(ObjectPool<DraggableItemUI> itemsPool, InventoryItem item)
    {
        if (item.IsEmpty)
        {
            ClearCell(itemsPool);
            return;
        }

        if (_itemUI == null)
        {
            _itemUI = itemsPool.Get();
            _itemUI.transform.SetParent(transform, false);    
        }

        _itemUI.transform.localPosition = Vector3.zero;
        _itemUI.SetItem(item);
        _itemUI.UnsubscribeOnDragger(ChangeItemPosition);
        _itemUI.UnsubscribeOnClick(ItemClick);
        _itemUI.SubscribeOnDragger(ChangeItemPosition);
        _itemUI.SubscribeOnClick(ItemClick);
    }

    public void ClearCell(ObjectPool<DraggableItemUI> itemsPool)
    {
        if (_itemUI == null)
            return;

        _itemUI.UnsubscribeOnDragger(ChangeItemPosition);
        _itemUI.UnsubscribeOnClick(ItemClick);
        itemsPool.Release(_itemUI);
        _itemUI = null;
    }

    public void SetDraggerActive(bool isActive)
    {
        if(_itemUI != null)
            _itemUI.SetDraggerActive(isActive);
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionImage == null)
            selectionImage = GetComponent<Image>();

        if (selectionImage != null)
            selectionImage.enabled = isSelected;
    }

    private void ChangeItemPosition(Vector3 position)
    {
        if (_itemUI == null)
            return;

        OnItemPositionChanged?.Invoke(position, this);
    }

    private void ItemClick()
    {
        if(_itemUI == null)
            return;
        OnItemClick?.Invoke(this);
    }

    private void OnDestroy()
    {
        if (_itemUI != null)
        {
            _itemUI.UnsubscribeOnDragger(ChangeItemPosition);
            _itemUI.UnsubscribeOnClick(ItemClick);
        }

        OnItemPositionChanged = null;
        OnItemClick = null;
    }
}
