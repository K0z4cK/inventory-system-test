using Infrastructure;
using UnityEngine;

[DefaultExecutionOrder(-9000)]
public class GameCompositionRoot : MonoBehaviour
{
    [Header("Static Data Overrides")]
    [SerializeField] private ItemCrafts itemCrafts;
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private WindowStaticData windowStaticData;

    private void Awake()
    {
        if (!ProjectContext.IsReady)
        {
            Debug.LogError("ProjectContext must be created before GameCompositionRoot.", this);
            return;
        }

        ProjectContext.Instance.ConfigureStaticData(itemDatabase, itemCrafts, windowStaticData);
    }
}
