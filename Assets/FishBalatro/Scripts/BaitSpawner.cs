using System.Collections.Generic;
using UnityEngine;

// Keeps the water populated with bait while the game is in the Normal state.
// Spawn weights get slightly more dangerous/rewarding as levels increase.
public class BaitSpawner : MonoBehaviour
{
    public FishGameManager gameManager;
    public FishPlayerController player;
    public BaitPickup[] baitPrefabs;
    public Vector2 spawnMin = new Vector2(-6.9f, -2.8f);
    public Vector2 spawnMax = new Vector2(7.1f, 2.7f);
    public int baseMaxBaits = 8;
    public float spawnInterval = 0.8f;
    public float minDistanceFromPlayer = 1.2f;

    private readonly List<BaitPickup> activeBaits = new List<BaitPickup>();
    private float spawnTimer;

    private void Update()
    {
        // Remove destroyed bait references after pickups are eaten.
        activeBaits.RemoveAll(bait => bait == null);

        if (gameManager == null || gameManager.State != FishGameState.Normal)
        {
            return;
        }

        spawnTimer -= Time.deltaTime;
        // Later levels allow a few more bait pieces on screen at once.
        int maxBaits = baseMaxBaits + Mathf.Min(4, gameManager.Level - 1);

        if (activeBaits.Count < maxBaits && spawnTimer <= 0f)
        {
            SpawnBait();
            spawnTimer = spawnInterval;
        }
    }

    public void SpawnOpeningBaits(int count)
    {
        // Used at the start of a level so the arena is immediately playable.
        for (int i = 0; i < count; i++)
        {
            SpawnBait();
        }
    }

    public void ClearBaits()
    {
        // Called during the big fish transition to prevent old level bait from
        // hanging around after the fisherman changes.
        for (int i = activeBaits.Count - 1; i >= 0; i--)
        {
            if (activeBaits[i] != null)
            {
                Destroy(activeBaits[i].gameObject);
            }
        }

        activeBaits.Clear();
    }

    private void SpawnBait()
    {
        if (baitPrefabs == null || baitPrefabs.Length == 0)
        {
            return;
        }

        FishBaitType type = PickWeightedType();
        BaitPickup prefab = FindPrefab(type);
        if (prefab == null)
        {
            prefab = baitPrefabs[0];
        }

        Vector3 position = PickSpawnPosition();
        BaitPickup bait = Instantiate(prefab, position, Quaternion.identity, transform);
        bait.Configure(type);
        activeBaits.Add(bait);
    }

    private Vector3 PickSpawnPosition()
    {
        Vector3 position = Vector3.zero;

        // Try a few random points so bait does not appear directly on the fish.
        for (int attempts = 0; attempts < 16; attempts++)
        {
            position = new Vector3(
                Random.Range(spawnMin.x, spawnMax.x),
                Random.Range(spawnMin.y, spawnMax.y),
                0f);

            if (player == null || Vector2.Distance(position, player.transform.position) >= minDistanceFromPlayer)
            {
                return position;
            }
        }

        return position;
    }

    private BaitPickup FindPrefab(FishBaitType type)
    {
        for (int i = 0; i < baitPrefabs.Length; i++)
        {
            if (baitPrefabs[i] != null && baitPrefabs[i].baitType == type)
            {
                return baitPrefabs[i];
            }
        }

        return null;
    }

    private FishBaitType PickWeightedType()
    {
        // Simple weighted random table. Tune these numbers first when changing
        // level pacing or bait frequency.
        int level = gameManager != null ? gameManager.Level : 1;
        int goldenWeight = Mathf.Clamp(5 + level, 5, 14);
        int smallFishWeight = Mathf.Clamp(9 + level, 9, 16);
        int fakeWeight = Mathf.Clamp(level - 1, 0, 7);
        int totalWeight = 42 + 20 + 16 + smallFishWeight + goldenWeight + fakeWeight;
        int roll = Random.Range(0, totalWeight);

        if ((roll -= 42) < 0)
        {
            return FishBaitType.Worm;
        }
        if ((roll -= 20) < 0)
        {
            return FishBaitType.Shrimp;
        }
        if ((roll -= 16) < 0)
        {
            return FishBaitType.GlowBug;
        }
        if ((roll -= smallFishWeight) < 0)
        {
            return FishBaitType.SmallFish;
        }
        if ((roll -= goldenWeight) < 0)
        {
            return FishBaitType.GoldenShrimp;
        }

        return FishBaitType.FakeBait;
    }
}
