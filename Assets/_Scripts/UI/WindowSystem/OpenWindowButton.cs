using Infrastructure;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class OpenWindowButton : MonoBehaviour
{
    [SerializeField] private WindowTypeId windowTypeId;
    [SerializeField] private Button button;

    private IWindowService _windowService;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        _windowService = ProjectContext.Get<IWindowService>();
    }

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(OpenWindow);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(OpenWindow);
    }

    private void OpenWindow()
    {
        _windowService?.Show(windowTypeId);
    }
}
