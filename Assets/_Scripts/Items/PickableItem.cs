using UnityEngine;
using UnityEngine.Serialization;

public class PickableItem : BaseItem, IInteractable
{
    [FormerlySerializedAs("_pickUpCount")]
    [SerializeField, Min(1)] private int pickUpCount = 1;

    public bool Interact(Character character)
    {
        if (character == null)
            return false;

        if (!character.TryAddItemsToInventory(itemObject, pickUpCount))
            return false;

        Destroy(gameObject);
        return true;
    }
}
