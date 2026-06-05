using UnityEngine;
using UnityEngine.Serialization;

public class HeldItemView : MonoBehaviour
{
    [FormerlySerializedAs("_itemObject")]
    [SerializeField] private ItemObject itemObject;
    [SerializeField] private GameObject viewObject;

    public ItemObject ItemObject => itemObject;
    public GameObject ViewObject => viewObject != null ? viewObject : gameObject;
}
