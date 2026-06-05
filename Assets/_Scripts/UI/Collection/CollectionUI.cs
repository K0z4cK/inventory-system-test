using System.Collections.Generic;
using Infrastructure;
using TMPro;
using UnityEngine;

public class CollectionUI : PlayerWindow
{
    [Header("Prefabs")]
    [SerializeField] private CollectionItemUI itemPrefab;

    [Header("Layout")]
    [SerializeField] private Transform itemsLayout;

    [Header("Progress")]
    [SerializeField] private TMP_Text progressTMP;

    private readonly List<CollectionItemUI> _itemViews = new List<CollectionItemUI>();
    private ItemDatabase _itemDatabase;
    private ICollectionProgressService _progressService;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void BindPlayer(IPlayerContext playerContext)
    {
        if (_progressService != null)
            _progressService.OnProgressChanged -= RefreshCollectionState;

        ProjectContext.TryGet(out _itemDatabase);
        _progressService = playerContext.Get<ICollectionProgressService>();
        _progressService.OnProgressChanged += RefreshCollectionState;
    }

    protected override void UnbindPlayer()
    {
        if (_progressService != null)
            _progressService.OnProgressChanged -= RefreshCollectionState;

        _itemDatabase = null;
        _progressService = null;
    }

    protected override void OnOpened()
    {
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
        if (!IsOpen)
            return;

        ShowItems();
    }

    private void UpdateProgressText()
    {
        if (progressTMP == null || _progressService == null)
            return;

        progressTMP.text = $"{_progressService.CurrentRank}  {_progressService.DiscoveredCount}/{_progressService.TotalCount}";
    }

    protected override void OnDisable()
    {
        if (_progressService != null)
            _progressService.OnProgressChanged -= RefreshCollectionState;

        base.OnDisable();
    }
}
