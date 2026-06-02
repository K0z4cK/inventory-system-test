using System;
using UnityEngine;
using UnityEngine.Pool;

public class InventoryCellUI : MonoBehaviour
{
    public event Action<Vector3, InventoryCellUI> OnItemPositionChanged;
    public event Action<InventoryCellUI> OnItemClick;

    private DraggableItemUI _itemUI;
    public bool HasItem => _itemUI != null;

    public void Init(Action<Vector3, InventoryCellUI> onItemPositionChanged)
    {
        OnItemPositionChanged = onItemPositionChanged;
    }

    public void SetItem(ObjectPool<DraggableItemUI> itemsPool, InventoryItem item)
    {
        if (_itemUI == null)
        {
            _itemUI = itemsPool.Get();
            _itemUI.transform.SetParent(transform, false);    
        }

        _itemUI.transform.localPosition = Vector3.zero;
        _itemUI.SetItem(item);
        _itemUI.SubscribeOnDragger(ChangeItemPosition);
        _itemUI.SubscribeOnClick(ItemClick);
    }

    public void ClearCell(ObjectPool<DraggableItemUI> itemsPool)
    {
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

    private void ChangeItemPosition(Vector3 position)
    {
        if (_itemUI == null)
            return;
        _itemUI.UnsubscribeOnDragger(ChangeItemPosition);
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
    }
}
