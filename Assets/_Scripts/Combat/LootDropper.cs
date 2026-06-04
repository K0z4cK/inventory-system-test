using System;
using System.Collections.Generic;
using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [SerializeField] private List<LootDropEntry> loot = new List<LootDropEntry>();
    [SerializeField, Min(0f)] private float scatterRadius = 0.75f;
    [SerializeField] private float heightOffset = 0.2f;

    private bool _hasDropped;

    public void DropLoot()
    {
        if (_hasDropped)
            return;

        _hasDropped = true;
        foreach (LootDropEntry entry in loot)
        {
            if (entry == null || !entry.ShouldDrop())
                continue;

            int instanceCount = entry.GetInstanceCount();
            for (int i = 0; i < instanceCount; i++)
            {
                Vector2 offset = UnityEngine.Random.insideUnitCircle * scatterRadius;
                Vector3 spawnPosition = transform.position + new Vector3(offset.x, heightOffset, offset.y);
                Instantiate(entry.PickablePrefab, spawnPosition, entry.PickablePrefab.transform.rotation);
            }
        }
    }
}

[Serializable]
public class LootDropEntry
{
    [SerializeField] private PickableItem pickablePrefab;
    [SerializeField, Range(0f, 1f)] private float dropChance = 1f;
    [SerializeField, Min(1)] private int minInstances = 1;
    [SerializeField, Min(1)] private int maxInstances = 1;

    public PickableItem PickablePrefab => pickablePrefab;

    public bool ShouldDrop()
    {
        return pickablePrefab != null && UnityEngine.Random.value <= Mathf.Clamp01(dropChance);
    }

    public int GetInstanceCount()
    {
        int min = Mathf.Max(1, minInstances);
        int max = Mathf.Max(min, maxInstances);
        return UnityEngine.Random.Range(min, max + 1);
    }
}
