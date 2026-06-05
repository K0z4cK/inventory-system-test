using System;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseWindow : MonoBehaviour
{
    [Header("Window")]
    [SerializeField] private Button closeButton;

    public event Action<BaseWindow> OnCloseRequested;
    protected bool IsOpen { get; private set; }

    protected virtual void OnEnable()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    protected virtual void OnDisable()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(Close);
    }

    public virtual void Open()
    {
        if (IsOpen)
            return;

        IsOpen = true;
        OnOpened();
    }

    public virtual void Close()
    {
        CloseWithoutRequest();
        OnCloseRequested?.Invoke(this);
    }

    internal void CloseWithoutRequest()
    {
        if (!IsOpen)
            return;

        IsOpen = false;
        OnClosed();
    }

    protected virtual void OnOpened()
    {
    }

    protected virtual void OnClosed()
    {
    }
}
