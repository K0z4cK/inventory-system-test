using UnityEngine;
using UnityEngine.Serialization;

public abstract class BasePanelUI : MonoBehaviour
{
    [Header("Panel")]
    [FormerlySerializedAs("_panel")]
    [SerializeField] private GameObject panel;

    protected GameObject Panel => panel != null ? panel : gameObject;

    public virtual void ShowPanel()
    {
        Panel.SetActive(true);
    }

    public virtual void HidePanel()
    {
        Panel.SetActive(false);
    }
}
