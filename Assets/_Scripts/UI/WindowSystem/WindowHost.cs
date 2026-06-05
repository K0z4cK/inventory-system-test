using Infrastructure;
using UnityEngine;

public class WindowHost : MonoBehaviour
{
    [SerializeField] private Transform windowsParent;

    private void Awake()
    {
        Transform parent = windowsParent != null ? windowsParent : transform;
        ProjectContext.Get<IWindowService>().SetParent(parent);
    }
}
