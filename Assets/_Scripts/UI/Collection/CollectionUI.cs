using System.Collections.Generic;
using TMPro;
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

    [Header("Progress")]
    [SerializeField] private TMP_Text progressTMP;

    private readonly List<CollectionItemUI> _itemViews = new List<CollectionItemUI>();
    private ItemDatabase _itemDatabase;
    private CollectionProgressService _progressService;

    private void Awake()
    {
        HidePanel();
    }

    public void Initialize(ItemDatabase itemDatabase, CollectionProgressService progressService)
    {
        if (_progressService != null)
            _progressService.OnProgressChanged -= RefreshCollectionState;

        _itemDatabase = itemDatabase;
        _progressService = progressService;

        if (_progressService != null)
            _progressService.OnProgressChanged += RefreshCollectionState;

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

        UpdateProgressText();

        IReadOnlyList<ItemObject> items = _itemDatabase.Items;
        for (int i = 0; i < items.Count; i++)
        {
            if (_itemViews.Count <= i)
                _itemViews.Add(Instantiate(itemPrefab, itemsLayout));

            _itemViews[i].Init(items[i], _progressService == null || _progressService.IsDiscovered(items[i]));
            _itemViews[i].gameObject.SetActive(true);
        }

        for (int i = items.Count; i < _itemViews.Count; i++)
        {
            _itemViews[i].gameObject.SetActive(false);
        }
    }

    private void RefreshCollectionState()
    {
        if (!Panel.activeSelf)
            return;

        ShowItems();
    }

    private void UpdateProgressText()
    {
        if (progressTMP == null || _progressService == null)
            return;

        progressTMP.text = $"{_progressService.CurrentRank}  {_progressService.DiscoveredCount}/{_progressService.TotalCount}";
    }

    private void OnDestroy()
    {
        if (_progressService != null)
            _progressService.OnProgressChanged -= RefreshCollectionState;
    }
}
