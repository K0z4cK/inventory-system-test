using UnityEngine;
using UnityEngine.Serialization;

public abstract class BaseItem : MonoBehaviour
{
    [FormerlySerializedAs("_itemObject")]
    [SerializeField] protected ItemObject itemObject;
}

