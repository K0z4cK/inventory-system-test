using UnityEngine;

public class ItemsHolder : MonoBehaviour
{
    private GameObject _currentItem;

    public void SetNewItem(ItemObject itemObject)
    {
        if (itemObject == null || itemObject.EquippablePrefab == null)
        {
            ClearCurrentItem();
            return;
        }

        if(_currentItem != null)
            Destroy(_currentItem);

        _currentItem = Instantiate(itemObject.EquippablePrefab, transform);
        _currentItem.transform.localPosition = Vector3.zero;
        _currentItem.transform.localRotation = Quaternion.identity;
        _currentItem.transform.localScale = Vector3.one;
    }

    private void ClearCurrentItem()
    {
        if (_currentItem == null)
            return;

        Destroy(_currentItem);
        _currentItem = null;
    }
}
