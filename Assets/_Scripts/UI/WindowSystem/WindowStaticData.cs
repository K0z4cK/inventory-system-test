using System;
using System.Collections.Generic;
using UnityEngine;

public enum WindowTypeId
{
    Unknown = 0,
    Inventory = 1,
    Craft = 2,
    Collection = 3
}

[Serializable]
public class WindowConfig
{
    [SerializeField] private WindowTypeId windowTypeId;
    [SerializeField] private BaseWindow prefab;

    public WindowTypeId WindowTypeId => windowTypeId;
    public BaseWindow Prefab => prefab;
}

[CreateAssetMenu(menuName = "Static Data/Window Static Data", fileName = "WindowStaticData")]
public class WindowStaticData : ScriptableObject
{
    [SerializeField] private List<WindowConfig> configs = new List<WindowConfig>();

    public IReadOnlyList<WindowConfig> Configs => configs;

    public bool TryGetConfig(WindowTypeId windowTypeId, out WindowConfig config)
    {
        foreach (WindowConfig windowConfig in configs)
        {
            if (windowConfig != null && windowConfig.WindowTypeId == windowTypeId)
            {
                config = windowConfig;
                return true;
            }
        }

        config = null;
        return false;
    }

    public List<string> GetValidationErrors()
    {
        List<string> errors = new List<string>();
        HashSet<WindowTypeId> registeredTypes = new HashSet<WindowTypeId>();

        for (int i = 0; i < configs.Count; i++)
        {
            WindowConfig config = configs[i];
            if (config == null)
            {
                errors.Add($"Window config at index {i} is null.");
                continue;
            }

            if (config.WindowTypeId == WindowTypeId.Unknown)
                errors.Add($"Window config at index {i} uses Unknown type.");
            else if (!registeredTypes.Add(config.WindowTypeId))
                errors.Add($"Duplicate window config for {config.WindowTypeId}.");

            if (config.Prefab == null)
                errors.Add($"Window prefab is missing for {config.WindowTypeId}.");
        }

        return errors;
    }

    private void OnValidate()
    {
        foreach (string error in GetValidationErrors())
            Debug.LogWarning(error, this);
    }
}
