using System;
using System.Collections.Generic;

namespace Infrastructure
{
    public sealed class ServiceContainer : IDisposable
    {
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly List<IDisposable> _disposables = new List<IDisposable>();

        public void Register<TContract>(TContract service)
        {
            Register(typeof(TContract), service, false);
        }

        public void Replace<TContract>(TContract service)
        {
            Register(typeof(TContract), service, true);
        }

        public TContract Resolve<TContract>()
        {
            if (TryResolve(out TContract service))
                return service;

            throw new InvalidOperationException(
                $"Service {typeof(TContract).Name} is not registered in this context.");
        }

        public bool TryResolve<TContract>(out TContract service)
        {
            if (_services.TryGetValue(typeof(TContract), out object registeredService)
                && registeredService is TContract typedService)
            {
                service = typedService;
                return true;
            }

            service = default;
            return false;
        }

        public void Dispose()
        {
            for (int i = _disposables.Count - 1; i >= 0; i--)
                _disposables[i].Dispose();

            _disposables.Clear();
            _services.Clear();
        }

        private void Register(Type contractType, object service, bool replace)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service), $"Cannot register null as {contractType.Name}.");

            if (_services.TryGetValue(contractType, out object currentService))
            {
                if (ReferenceEquals(currentService, service))
                    return;

                if (!replace)
                    throw new InvalidOperationException($"Service {contractType.Name} is already registered.");

                RemoveDisposableIfUnused(currentService, contractType);
            }

            _services[contractType] = service;
            if (service is IDisposable disposable && !_disposables.Contains(disposable))
                _disposables.Add(disposable);
        }

        private void RemoveDisposableIfUnused(object service, Type replacedContract)
        {
            if (!(service is IDisposable disposable))
                return;

            foreach (KeyValuePair<Type, object> binding in _services)
            {
                if (binding.Key != replacedContract && ReferenceEquals(binding.Value, service))
                    return;
            }

            _disposables.Remove(disposable);
            disposable.Dispose();
        }
    }
}
