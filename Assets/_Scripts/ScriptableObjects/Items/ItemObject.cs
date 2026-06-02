using UnityEngine;

public enum ItemType {None, Stone, Wood, Berry, Axe, Spear, Pickaxe }

[CreateAssetMenu(fileName = "ItemData", menuName = "ScriptableObjects/ItemScriptableObject")]
public class ItemObject : ScriptableObject
{
    public ItemType Type;   
    public string Name;
    public string Description;
    public Sprite Sprite;
}
