using System;

namespace Infrastructure
{
    public sealed class LocalPlayerProvider : ILocalPlayerProvider
    {
        public event Action<IPlayerContext> OnLocalPlayerChanged;

        public IPlayerContext LocalPlayer { get; private set; }

        public void SetLocalPlayer(IPlayerContext playerContext)
        {
            if (playerContext == null)
                throw new ArgumentNullException(nameof(playerContext));

            if (ReferenceEquals(LocalPlayer, playerContext))
                return;

            LocalPlayer = playerContext;
            OnLocalPlayerChanged?.Invoke(LocalPlayer);
        }

        public void ClearLocalPlayer(IPlayerContext playerContext)
        {
            if (!ReferenceEquals(LocalPlayer, playerContext))
                return;

            LocalPlayer = null;
            OnLocalPlayerChanged?.Invoke(null);
        }

        public TService Get<TService>()
        {
            if (LocalPlayer == null)
                throw new InvalidOperationException("Local player context is not registered.");

            return LocalPlayer.Get<TService>();
        }

        public bool TryGet<TService>(out TService service)
        {
            if (LocalPlayer != null)
                return LocalPlayer.TryGet(out service);

            service = default;
            return false;
        }
    }
}
