using System;

namespace Infrastructure
{
    public interface IPlayerContext : IDisposable
    {
        string PlayerId { get; }
        bool IsLocalPlayer { get; }
        bool IsInitialized { get; }

        TService Get<TService>();
        bool TryGet<TService>(out TService service);
    }

    public interface ILocalPlayerProvider
    {
        event Action<IPlayerContext> OnLocalPlayerChanged;

        IPlayerContext LocalPlayer { get; }

        void SetLocalPlayer(IPlayerContext playerContext);
        void ClearLocalPlayer(IPlayerContext playerContext);
        TService Get<TService>();
        bool TryGet<TService>(out TService service);
    }
}
