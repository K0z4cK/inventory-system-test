using UnityEngine;

public interface IWindowService
{
    BaseWindow CurrentWindow { get; }

    void Configure(WindowStaticData staticData);
    void SetParent(Transform parent);
    BaseWindow Show(WindowTypeId windowTypeId);
    void CloseCurrent();
}
