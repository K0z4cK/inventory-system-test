using UnityEngine;
using UnityEngine.Serialization;

public abstract class BasePanelUI : MonoBehaviour
{
    [Header("Panel")]
    [FormerlySerializedAs("_panel")]
    [SerializeField] private GameObject panel;

    public virtual void ShowPanel()
    {
        if (panel != null)
            panel.SetActive(true);
    }

    public virtual void HidePanel()
    {
        if (panel != null)
            panel.SetActive(false);
    }
}
