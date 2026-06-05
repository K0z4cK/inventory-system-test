using System;
using Infrastructure;
using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class PlayerContext : MonoBehaviour, IPlayerContext
{
    [Header("Player Identity")]
    [SerializeField] private string playerId = "LocalPlayer";
    [SerializeField] private bool isLocalPlayer = true;
    [SerializeField] private bool manualInitialization;

    [Header("Inventory")]
    [SerializeField, Min(1)] private int maxCells = 16;
    [SerializeField, Min(1)] private int maxItemsInCell = 10;
    [SerializeField] private ItemsHolder itemsHolder;

    private ServiceContainer _services;
    private ILocalPlayerProvider _localPlayerProvider;
    private CollectionProgressService _collectionProgressService;
    private IGameplayFeedback _feedback;
    private bool _isDisposed;

    public event Action<PlayerContext> OnInitialized;

    public string PlayerId => playerId;
    public bool IsLocalPlayer => isLocalPlayer;
    public bool IsInitialized => _services != null;

    protected virtual void Awake()
    {
        if (!manualInitialization)
            InstallPlayerServices();
    }

    public void Initialize(string configuredPlayerId, bool localPlayer)
    {
        if (IsInitialized)
            throw new InvalidOperationException("PlayerContext is already initialized.");

        playerId = string.IsNullOrWhiteSpace(configuredPlayerId) ? playerId : configuredPlayerId;
        isLocalPlayer = localPlayer;
        InstallPlayerServices();
    }

    public TService Get<TService>()
    {
        EnsureInstalled();
        return _services.Resolve<TService>();
    }

    public bool TryGet<TService>(out TService service)
    {
        if (_services == null)
        {
            service = default;
            return false;
        }

        return _services.TryResolve(out service);
    }

    public void Dispose()
    {
        if (_isDisposed)
            return;

        _isDisposed = true;
        _localPlayerProvider?.ClearLocalPlayer(this);
        UnsubscribeCollectionFeedback();
        _services?.Dispose();
        _services = null;
        OnInitialized = null;
    }

    private void InstallPlayerServices()
    {
        if (_isDisposed)
            throw new ObjectDisposedException(nameof(PlayerContext));

        if (_services != null)
            return;

        _services = new ServiceContainer();
        _localPlayerProvider = ProjectContext.Get<ILocalPlayerProvider>();
        _feedback = isLocalPlayer ? ProjectContext.Get<IGameplayFeedback>() : null;

        InventoryService inventoryService = new InventoryService(maxCells, maxItemsInCell, _feedback);
        _services.Register<IInventoryService>(inventoryService);
        _services.Register<IInventory>(inventoryService);
        _services.Register<IInventorySlotSelector>(inventoryService);

        ProjectContext.TryGet(out ItemCrafts itemCrafts);
        CraftingService craftingService = new CraftingService(itemCrafts, inventoryService);
        _services.Register<ICraftingService>(craftingService);
        _services.Register(craftingService);

        ProjectContext.TryGet(out ItemDatabase itemDatabase);
        _collectionProgressService = new CollectionProgressService(itemDatabase, inventoryService);
        if (_feedback != null)
        {
            _collectionProgressService.OnItemDiscovered += _feedback.ShowItemDiscovered;
            _collectionProgressService.OnMilestoneReached += _feedback.ShowCollectionMilestone;
        }
        _services.Register<ICollectionProgressService>(_collectionProgressService);
        _services.Register(_collectionProgressService);

        RegisterInventoryDropService();
        ResolveItemsHolder();
        itemsHolder?.Bind(inventoryService);

        if (isLocalPlayer)
            _localPlayerProvider.SetLocalPlayer(this);

        OnInitialized?.Invoke(this);
    }

    private void ResolveItemsHolder()
    {
        if (itemsHolder == null)
            itemsHolder = GetComponentInChildren<ItemsHolder>(true);
    }

    private void RegisterInventoryDropService()
    {
        IInventoryDropService dropService = GetComponentInChildren<IInventoryDropService>(true);
        if (dropService != null)
            _services.Register(dropService);
    }

    private void UnsubscribeCollectionFeedback()
    {
        if (_collectionProgressService == null || _feedback == null)
            return;

        _collectionProgressService.OnItemDiscovered -= _feedback.ShowItemDiscovered;
        _collectionProgressService.OnMilestoneReached -= _feedback.ShowCollectionMilestone;
    }

    private void EnsureInstalled()
    {
        if (_services == null && !manualInitialization)
            InstallPlayerServices();

        if (_services == null)
            throw new InvalidOperationException("PlayerContext requires manual initialization before resolving services.");
    }

    protected virtual void OnDestroy()
    {
        Dispose();
    }
}
