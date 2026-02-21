using System.Collections.Generic;
using UnityEngine;

public class VendingMachineSpawner : MonoBehaviour
{
    [SerializeField] private Transform[] surfaceSlots;
    [SerializeField] private float depthOffset = 0.25f;
    [SerializeField] private int maxDepthGap = 2;
    [SerializeField] private List<ItemPrefabData> itemPrefabs;

    private Dictionary<ItemType, Item> prefabLookup;

    private void Awake()
    {
        prefabLookup = new Dictionary<ItemType, Item>();
        foreach (var data in itemPrefabs)
            prefabLookup[data.itemType] = data.prefab;
    }

    private void Start() => Spawn();

    private void Spawn()
    {
        int width = surfaceSlots.Length;
        int totalItems = itemPrefabs.Count * 3;
        int depthCount = Mathf.CeilToInt((float)totalItems / width);

        List<ItemType>[] depthBuckets = new List<ItemType>[depthCount];
        for (int i = 0; i < depthCount; i++)
            depthBuckets[i] = new List<ItemType>();

        int GetFreeDepth(int min, int max)
        {
            List<int> candidates = new List<int>();
            for (int d = min; d <= max; d++)
                if (d < depthCount && depthBuckets[d].Count < width)
                    candidates.Add(d);
            return candidates.Count == 0 ? -1 : candidates[Random.Range(0, candidates.Count)];
        }

        foreach (var data in itemPrefabs)
        {
            int baseDepth = Random.Range(0, depthCount);
            int minDepth = baseDepth;
            int maxDepth = Mathf.Min(baseDepth + maxDepthGap, depthCount - 1);

            for (int i = 0; i < 3; i++)
            {
                int depth = GetFreeDepth(minDepth, maxDepth);
                if (depth == -1) depth = GetFreeDepth(0, depthCount - 1);
                if (depth == -1) break;
                depthBuckets[depth].Add(data.itemType);
            }
        }

        for (int d = 0; d < depthBuckets.Length; d++)
            Shuffle(depthBuckets[d]);

        List<ItemType> finalSpawnList = new List<ItemType>();
        for (int d = 0; d < depthBuckets.Length; d++)
            finalSpawnList.AddRange(depthBuckets[d]);

        int index = 0;
        for (int depth = 0; depth < depthCount; depth++)
        {
            for (int slot = 0; slot < surfaceSlots.Length; slot++)
            {
                if (index >= finalSpawnList.Count) return;

                ItemType type = finalSpawnList[index++];
                Item prefab = prefabLookup[type];
                Vector3 pos = surfaceSlots[slot].position + Vector3.forward * depth * depthOffset;
                Item item = Instantiate(prefab, pos, Quaternion.identity);
                item.itemType = type;
            }
        }
    }

    private void Shuffle(List<ItemType> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
