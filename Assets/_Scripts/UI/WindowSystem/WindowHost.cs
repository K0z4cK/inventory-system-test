using Infrastructure;
using UnityEngine;

public class WindowHost : MonoBehaviour
{
    [SerializeField] private Transform windowsParent;
    [SerializeField] private GameObject hudRoot;

    private IWindowService _windowService;

    private void Awake()
    {
        _windowService = ProjectContext.Get<IWindowService>();
        Transform parent = windowsParent != null ? windowsParent : transform;
        _windowService.SetParent(parent);
        ApplyHudVisibility();
    }

    private void OnEnable()
    {
        if (_windowService == null)
            _windowService = ProjectContext.Get<IWindowService>();

        _windowService.OnWindowOpened += HandleWindowOpened;
        _windowService.OnWindowClosed += HandleWindowClosed;
        ApplyHudVisibility();
    }

    private void OnDisable()
    {
        if (_windowService == null)
            return;

        _windowService.OnWindowOpened -= HandleWindowOpened;
        _windowService.OnWindowClosed -= HandleWindowClosed;
    }

    private void HandleWindowOpened(BaseWindow window)
    {
        ApplyHudVisibility();
    }

    private void HandleWindowClosed()
    {
        ApplyHudVisibility();
    }

    private void ApplyHudVisibility()
    {
        if (hudRoot != null)
            hudRoot.SetActive(_windowService == null || _windowService.CurrentWindow == null);
    }
}
