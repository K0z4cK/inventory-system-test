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
    [SerializeField] private FeedbackUI feedbackUI;

    private void Start()
    {
        if (playerInventory == null)
        {
            Debug.LogError("GameCompositionRoot requires player InventorySystem.");
            return;
        }

        GameplayFeedbackService feedbackService = new GameplayFeedbackService();
        playerInventory.Initialize(feedbackService);

        if (feedbackUI != null)
            feedbackUI.Initialize(feedbackService);
        else
            Debug.LogError("GameCompositionRoot requires FeedbackUI.");

        if (inventoryUI != null)
            inventoryUI.Initialize(playerInventory, playerInventory);
        else
            Debug.LogError("GameCompositionRoot requires InventoryUI.");

        if (craftUI != null)
            craftUI.Initialize(itemCrafts, playerInventory, feedbackService);
        else
            Debug.LogError("GameCompositionRoot requires CraftUI.");

        if (collectionUI != null)
            collectionUI.Initialize(itemDatabase);
        else
            Debug.LogError("GameCompositionRoot requires CollectionUI.");
    }
}
