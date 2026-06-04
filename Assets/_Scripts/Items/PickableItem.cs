using UnityEngine;
using UnityEngine.Serialization;

public class PickableItem : BaseItem, IInteractable
{
    [FormerlySerializedAs("_pickUpCount")]
    [SerializeField, Min(1)] private int pickUpCount = 1;

    private bool _isPickedUp;

    public bool CanInteract => !_isPickedUp && itemObject != null;

    public bool Interact(Character character)
    {
        if (character == null)
            return false;

        if (!character.TryAddItemsToInventory(itemObject, pickUpCount))
            return false;

        _isPickedUp = true;
        Destroy(gameObject);
        return true;
    }
}
