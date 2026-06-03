using UnityEngine;

public class GameCompositionRoot : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private InventorySystem playerInventory;

    [Header("Data")]
    [SerializeField] private ItemCrafts itemCrafts;
    [SerializeField] private ItemDatabase itemDatabase;

    [Header("UI")]
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private CraftUI craftUI;
    [SerializeField] private CollectionUI collectionUI;

    private void Start()
    {
        if (playerInventory == null)
        {
            Debug.LogError("GameCompositionRoot requires player InventorySystem.");
            return;
        }

        if (inventoryUI != null)
            inventoryUI.Initialize(playerInventory, playerInventory);
        else
            Debug.LogError("GameCompositionRoot requires InventoryUI.");

        if (craftUI != null)
            craftUI.Initialize(itemCrafts, playerInventory);
        else
            Debug.LogError("GameCompositionRoot requires CraftUI.");

        if (collectionUI != null)
            collectionUI.Initialize(itemDatabase);
        else
            Debug.LogError("GameCompositionRoot requires CollectionUI.");
    }
}
