using System;
using UnityEngine;

public interface IWindowService
{
    event Action<BaseWindow> OnWindowOpened;
    event Action OnWindowClosed;

    BaseWindow CurrentWindow { get; }

    void Configure(WindowStaticData staticData);
    void SetParent(Transform parent);
    BaseWindow Show(WindowTypeId windowTypeId);
    void CloseCurrent();
}
