using UnityEngine;

public enum ItemCategory { None, Resource, Food, Tool, Weapon }

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemScriptableObject")]
public class ItemObject : ScriptableObject
{
    public string ItemId;
    public ItemCategory Category;
    public string Name;
    public string Description;
    public Sprite Sprite;
    public GameObject EquippablePrefab;

    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(ItemId))
            Debug.LogWarning($"{name} has an empty ItemId.", this);

        if (string.IsNullOrWhiteSpace(Name))
            Debug.LogWarning($"{name} has an empty display Name.", this);

        if (Sprite == null)
            Debug.LogWarning($"{name} has no Sprite assigned.", this);
    }
}
