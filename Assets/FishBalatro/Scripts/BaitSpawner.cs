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
    public int baseMaxBaits = 6;
    public int maxActiveBaits = 8;
    public float spawnInterval = 1.1f;
    public float minDistanceFromPlayer = 1.8f;
    public float minDistanceBetweenBaits = 1.55f;
    public Vector2 bigFishBlockCenter = new Vector2(-6.35f, -2.85f);
    public Vector2 bigFishBlockSize = new Vector2(5.25f, 2.8f);
    public int baseLevelBaitBudget = 16;
    public int baitBudgetPerLevel = 3;
    public int maxLevelBaitBudget = 24;
    public int emergencyBaitBudget = 4;

    private readonly List<BaitPickup> activeBaits = new List<BaitPickup>();
    private float spawnTimer;
    private int budgetLevel;
    private int levelBaitBudget;
    private int spawnedThisLevel;
    private bool emergencyBudgetUsed;

    private void Update()
    {
        // Remove destroyed bait references after pickups are eaten.
        activeBaits.RemoveAll(bait => bait == null);

        if (gameManager == null || gameManager.State != FishGameState.Normal)
        {
            return;
        }

        EnsureBudgetForCurrentLevel();

        spawnTimer -= Time.deltaTime;
        int targetActiveBaits = GetTargetActiveBaits();
        AddEmergencyBudgetIfNeeded();

        if (activeBaits.Count < targetActiveBaits && spawnedThisLevel < levelBaitBudget && spawnTimer <= 0f)
        {
            if (!SpawnBait())
            {
                // If the arena is too crowded for the spacing rules, wait a
                // short beat and try again after the player moves/eats bait.
                spawnTimer = 0.25f;
                return;
            }

            spawnTimer = spawnInterval;
        }
    }

    public void SpawnOpeningBaits(int count)
    {
        // Used at the start of a level so the arena is immediately playable.
        // This also resets the finite bait budget for the new fisherman.
        BeginLevelBudget();
        int openingCount = Mathf.Min(count, GetTargetActiveBaits(), levelBaitBudget);

        for (int i = 0; i < openingCount; i++)
        {
            if (!SpawnBait())
            {
                break;
            }
        }

        spawnTimer = spawnInterval;
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

    private void BeginLevelBudget()
    {
        int level = gameManager != null ? gameManager.Level : 1;
        budgetLevel = level;
        levelBaitBudget = Mathf.Min(maxLevelBaitBudget, baseLevelBaitBudget + (level - 1) * baitBudgetPerLevel);
        spawnedThisLevel = 0;
        emergencyBudgetUsed = false;
    }

    private void EnsureBudgetForCurrentLevel()
    {
        int level = gameManager != null ? gameManager.Level : 1;
        if (budgetLevel != level || levelBaitBudget <= 0)
        {
            BeginLevelBudget();
        }
    }

    private int GetTargetActiveBaits()
    {
        int level = gameManager != null ? gameManager.Level : 1;
        int levelBonus = Mathf.Min(2, level - 1);
        return Mathf.Clamp(baseMaxBaits + levelBonus, 1, maxActiveBaits);
    }

    private void AddEmergencyBudgetIfNeeded()
    {
        // Prevent a hard lock if the player spends the whole level budget but
        // still cannot afford the attack. This is once per level, so bait never
        // becomes an infinite score faucet.
        if (emergencyBudgetUsed || activeBaits.Count > 0 || spawnedThisLevel < levelBaitBudget)
        {
            return;
        }

        if (gameManager != null && gameManager.TotalScore < gameManager.AttackCost)
        {
            levelBaitBudget += emergencyBaitBudget;
            emergencyBudgetUsed = true;
            spawnTimer = 0f;
            gameManager.StatusText = "Only a few risky scraps remain.";
        }
    }

    private bool SpawnBait()
    {
        if (baitPrefabs == null || baitPrefabs.Length == 0)
        {
            return false;
        }

        FishBaitType type = PickWeightedType();
        BaitPickup prefab = FindPrefab(type);
        if (prefab == null)
        {
            prefab = baitPrefabs[0];
        }

        if (!TryPickSpawnPosition(out Vector3 position))
        {
            return false;
        }

        BaitPickup bait = Instantiate(prefab, position, Quaternion.identity, transform);
        bait.Configure(type);
        activeBaits.Add(bait);
        spawnedThisLevel++;
        return true;
    }

    private bool TryPickSpawnPosition(out Vector3 position)
    {
        position = Vector3.zero;

        // Try a few random points so bait does not appear directly on the fish
        // or too close to another bait. This prevents accidental multi-pickups.
        for (int attempts = 0; attempts < 32; attempts++)
        {
            position = new Vector3(
                Random.Range(spawnMin.x, spawnMax.x),
                Random.Range(spawnMin.y, spawnMax.y),
                0f);

            if (IsValidSpawnPosition(position))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsValidSpawnPosition(Vector3 position)
    {
        if (IsInsideBigFishBlock(position))
        {
            return false;
        }

        if (player != null && Vector2.Distance(position, player.transform.position) < minDistanceFromPlayer)
        {
            return false;
        }

        for (int i = 0; i < activeBaits.Count; i++)
        {
            if (activeBaits[i] != null && Vector2.Distance(position, activeBaits[i].transform.position) < minDistanceBetweenBaits)
            {
                return false;
            }
        }

        return true;
    }

    private bool IsInsideBigFishBlock(Vector3 position)
    {
        // The big fish lives in the lower-left corner. Blocking this rectangle
        // prevents bait from appearing underneath the ally sprite or prompt.
        Vector2 halfSize = bigFishBlockSize * 0.5f;
        return position.x >= bigFishBlockCenter.x - halfSize.x
            && position.x <= bigFishBlockCenter.x + halfSize.x
            && position.y >= bigFishBlockCenter.y - halfSize.y
            && position.y <= bigFishBlockCenter.y + halfSize.y;
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
