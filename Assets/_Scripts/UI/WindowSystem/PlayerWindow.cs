using Infrastructure;

public abstract class PlayerWindow : BaseWindow
{
    private ILocalPlayerProvider _localPlayerProvider;
    private IPlayerContext _boundPlayer;

    protected virtual void Awake()
    {
        ResolveProvider();
        BindCurrentPlayer();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        ResolveProvider();
        _localPlayerProvider.OnLocalPlayerChanged += HandleLocalPlayerChanged;
        BindCurrentPlayer();
    }

    protected override void OnDisable()
    {
        if (_localPlayerProvider != null)
            _localPlayerProvider.OnLocalPlayerChanged -= HandleLocalPlayerChanged;

        UnbindCurrentPlayer();
        base.OnDisable();
    }

    protected abstract void BindPlayer(IPlayerContext playerContext);
    protected abstract void UnbindPlayer();

    private void ResolveProvider()
    {
        if (_localPlayerProvider == null)
            _localPlayerProvider = ProjectContext.Get<ILocalPlayerProvider>();
    }

    private void BindCurrentPlayer()
    {
        IPlayerContext localPlayer = _localPlayerProvider.LocalPlayer;
        if (ReferenceEquals(_boundPlayer, localPlayer))
            return;

        UnbindCurrentPlayer();
        _boundPlayer = localPlayer;
        if (_boundPlayer != null)
            BindPlayer(_boundPlayer);
    }

    private void UnbindCurrentPlayer()
    {
        if (_boundPlayer == null)
            return;

        UnbindPlayer();
        _boundPlayer = null;
    }

    private void HandleLocalPlayerChanged(IPlayerContext playerContext)
    {
        BindCurrentPlayer();
    }
}
