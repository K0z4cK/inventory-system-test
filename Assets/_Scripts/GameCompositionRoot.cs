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

    private CollectionProgressService _collectionProgressService;

    private void Start()
    {
        if (playerInventory == null)
        {
            Debug.LogError("GameCompositionRoot requires player InventorySystem.");
            return;
        }

        GameplayFeedbackService feedbackService = new GameplayFeedbackService();
        playerInventory.Initialize(feedbackService);
        Character playerCharacter = playerInventory.GetComponent<Character>();
        if (playerCharacter != null)
            playerCharacter.Initialize(feedbackService);
        else
            Debug.LogError("GameCompositionRoot requires Character next to player InventorySystem.");

        _collectionProgressService = new CollectionProgressService(itemDatabase, playerInventory);
        _collectionProgressService.OnItemDiscovered += feedbackService.ShowItemDiscovered;
        _collectionProgressService.OnMilestoneReached += feedbackService.ShowCollectionMilestone;

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
            collectionUI.Initialize(itemDatabase, _collectionProgressService);
        else
            Debug.LogError("GameCompositionRoot requires CollectionUI.");
    }

    private void OnDestroy()
    {
        if (_collectionProgressService != null)
            _collectionProgressService.Dispose();
    }
}
