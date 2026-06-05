using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public sealed class WindowService : IWindowService, IDisposable
{
    private WindowStaticData _staticData;
    private Transform _parent;
    private readonly Dictionary<WindowTypeId, BaseWindow> _windowsByType = new Dictionary<WindowTypeId, BaseWindow>();

    public BaseWindow CurrentWindow { get; private set; }

    public WindowService(WindowStaticData staticData)
    {
        Configure(staticData);
    }

    public void Configure(WindowStaticData staticData)
    {
        _staticData = staticData;
        BindWindows();
    }

    public void SetParent(Transform parent)
    {
        _parent = parent;
    }

    public BaseWindow Show(WindowTypeId windowTypeId)
    {
        if (!_windowsByType.TryGetValue(windowTypeId, out BaseWindow prefab))
        {
            Debug.LogWarning($"Window config is not registered for {windowTypeId}.");
            return null;
        }

        CloseCurrent();

        CurrentWindow = Object.Instantiate(prefab, _parent, false);
        CurrentWindow.OnCloseRequested += HandleCloseRequested;
        CurrentWindow.Open();
        return CurrentWindow;
    }

    public void CloseCurrent()
    {
        if (CurrentWindow == null)
            return;

        BaseWindow window = CurrentWindow;
        CurrentWindow = null;
        window.OnCloseRequested -= HandleCloseRequested;
        window.CloseWithoutRequest();
        Object.Destroy(window.gameObject);
    }

    public void Dispose()
    {
        CloseCurrent();
        _parent = null;
        _staticData = null;
        _windowsByType.Clear();
    }

    private void BindWindows()
    {
        _windowsByType.Clear();

        if (_staticData == null)
        {
            Debug.LogWarning("WindowStaticData is not assigned.");
            return;
        }

        foreach (string error in _staticData.GetValidationErrors())
            Debug.LogWarning(error, _staticData);

        foreach (WindowConfig config in _staticData.Configs)
        {
            if (config == null ||
                config.WindowTypeId == WindowTypeId.Unknown ||
                config.Prefab == null ||
                _windowsByType.ContainsKey(config.WindowTypeId))
            {
                continue;
            }

            _windowsByType.Add(config.WindowTypeId, config.Prefab);
        }
    }

    private void HandleCloseRequested(BaseWindow window)
    {
        if (window == null)
            return;

        if (window == CurrentWindow)
            CurrentWindow = null;

        window.OnCloseRequested -= HandleCloseRequested;
        Object.Destroy(window.gameObject);
    }
}
