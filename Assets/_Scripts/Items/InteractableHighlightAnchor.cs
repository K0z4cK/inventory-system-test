using UnityEngine;

public class InteractableHighlightAnchor : MonoBehaviour
{
    [SerializeField] private Transform anchor;

    public Vector3 Position => anchor != null ? anchor.position : transform.position;
}
