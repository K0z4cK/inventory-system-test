using UnityEngine;

public abstract class BasePanelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject _panel;

    public virtual void ShowPanel() => _panel.SetActive(true);
    public virtual void HidePanel() => _panel.SetActive(false);
}
