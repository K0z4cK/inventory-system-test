public interface IInteractable
{
    bool CanInteract { get; }
    bool Interact(Character character);
}
