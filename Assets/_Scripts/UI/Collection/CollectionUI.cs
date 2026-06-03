using System.Collections.Generic;
using UnityEngine;

public class CollectionUI : BasePanelUI
{
    [Header("Prefabs")]
    [SerializeField] private CollectionItemUI itemPrefab;

    [Header("Data")]
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("Layout")]
    [SerializeField] private Transform itemsLayout;

    private readonly List<CollectionItemUI> _itemViews = new List<CollectionItemUI>();

    private void Awake()
    {
        ResolveDatabase();
        HidePanel();
    }

    public override void ShowPanel()
    {
        base.ShowPanel();
        ShowItems();
    }

    private void ShowItems()
    {
        if (itemDatabase == null || itemPrefab == null || itemsLayout == null)
        {
            Debug.LogError("CollectionUI requires ItemDatabase, item prefab, and layout references.");
            return;
        }

        IReadOnlyList<ItemObject> items = itemDatabase.Items;
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

    private void ResolveDatabase()
    {
        if (itemDatabase == null)
            itemDatabase = Resources.Load<ItemDatabase>("ItemDatabase");

        if (itemDatabase == null)
            Debug.LogError("CollectionUI requires ItemDatabase. Assign it or place ItemDatabase.asset in a Resources folder.");
    }
}
