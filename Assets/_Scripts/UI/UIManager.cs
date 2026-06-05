using Infrastructure;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Window Host")]
    [SerializeField] private Transform windowsParent;

    [Header("Legacy Inventory Buttons")]
    [SerializeField] private Button showInventoryBtn;
    [SerializeField] private Button hideInventoryBtn;

    [Header("Legacy Craft Buttons")]
    [SerializeField] private Button showCraftBtn;
    [SerializeField] private Button hideCraftBtn;

    [Header("Legacy Collection Buttons")]
    [SerializeField] private Button showCollectionBtn;
    [SerializeField] private Button hideCollectionBtn;

    private IWindowService _windowService;

    private void Awake()
    {
        Transform parent = windowsParent != null ? windowsParent : transform;
        _windowService = ProjectContext.Get<IWindowService>();
        _windowService.SetParent(parent);

        SubscribeLegacyButtons();
    }

    private void SubscribeLegacyButtons()
    {
        showInventoryBtn?.onClick.AddListener(ShowInventory);
        hideInventoryBtn?.onClick.AddListener(HideInventory);
        showCraftBtn?.onClick.AddListener(ShowCraft);
        hideCraftBtn?.onClick.AddListener(HideCraft);
        showCollectionBtn?.onClick.AddListener(ShowCollection);
        hideCollectionBtn?.onClick.AddListener(HideCollection);
    }

    private void ShowInventory() => _windowService?.Show(WindowTypeId.Inventory);
    private void HideInventory() => _windowService?.CloseCurrent();
    private void ShowCraft() => _windowService?.Show(WindowTypeId.Craft);
    private void HideCraft() => _windowService?.CloseCurrent();

    private void ShowCollection()
    {
        _windowService?.Show(WindowTypeId.Collection);
        if (showCollectionBtn != null)
            showCollectionBtn.gameObject.SetActive(false);
    }

    private void HideCollection()
    {
        _windowService?.CloseCurrent();
        if (showCollectionBtn != null)
            showCollectionBtn.gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        showInventoryBtn?.onClick.RemoveListener(ShowInventory);
        hideInventoryBtn?.onClick.RemoveListener(HideInventory);
        showCraftBtn?.onClick.RemoveListener(ShowCraft);
        hideCraftBtn?.onClick.RemoveListener(HideCraft);
        showCollectionBtn?.onClick.RemoveListener(ShowCollection);
        hideCollectionBtn?.onClick.RemoveListener(HideCollection);
    }
}
