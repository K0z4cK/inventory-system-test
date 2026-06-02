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
}
