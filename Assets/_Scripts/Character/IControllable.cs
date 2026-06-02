using UnityEngine;

public interface IControllable
{
    public void Move(Vector2 direction);
    public void Interact();
    public void Action();
}
