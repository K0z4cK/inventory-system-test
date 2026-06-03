using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class CollectionUI : BasePanelUI
{
    [Header("Prefabs")]
    [FormerlySerializedAs("_itemPrefab")]
    [SerializeField] private CollectionItemUI itemPrefab;

    [Header("Layout")]
    [FormerlySerializedAs("_itemsLayout")]
    [SerializeField] private Transform itemsLayout;

    private readonly List<CollectionItemUI> _itemViews = new List<CollectionItemUI>();
    private ItemDatabase _itemDatabase;

    private void Awake()
    {
        HidePanel();
    }

    public void Initialize(ItemDatabase itemDatabase)
    {
        _itemDatabase = itemDatabase;

        if (_itemDatabase == null)
            Debug.LogError("CollectionUI requires ItemDatabase.");
    }

    public override void ShowPanel()
    {
        base.ShowPanel();
        ShowItems();
    }

    private void ShowItems()
    {
        if (_itemDatabase == null || itemPrefab == null || itemsLayout == null)
        {
            Debug.LogError("CollectionUI requires ItemDatabase, item prefab, and layout references.");
            return;
        }

        IReadOnlyList<ItemObject> items = _itemDatabase.Items;
        for (int i = 0; i < items.Count; i++)
        {
            if (_itemViews.Count <= i)
                _itemViews.Add(Instantiate(itemPrefab, itemsLayout));

            _itemViews[i].Init(items[i]);
            _itemViews[i].gameObject.SetActive(true);
        }

        for (int i = items.Count; i < _itemViews.Count; i++)
        {
            _itemViews[i].gameObject.SetActive(false);
        }
    }
}
