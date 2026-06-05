using UnityEngine;

public class HeldItemView : MonoBehaviour
{
    [SerializeField] private ItemObject itemObject;
    [SerializeField] private GameObject viewObject;

    public ItemObject ItemObject => itemObject;
    public GameObject ViewObject => viewObject != null ? viewObject : gameObject;
}
