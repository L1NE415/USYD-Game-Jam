using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
// Component placed on every bait prefab. It detects the player touching bait.
// Then asks FishGameManager to apply the actual card-like bait effect.
public class BaitPickup : MonoBehaviour
{
    public FishBaitType baitType = FishBaitType.Worm;
    public SpriteRenderer spriteRenderer;
    public TMP_Text label;

    private bool eaten;

    private void Reset()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        label = GetComponentInChildren<TMP_Text>();
        Collider2D baitCollider = GetComponent<Collider2D>();
        baitCollider.isTrigger = true;
    }

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (label == null)
        {
            label = GetComponentInChildren<TMP_Text>();
        }

        ApplyLabel();
    }

    public void Configure(FishBaitType type)
    {
        // The spawner can reuse one prefab shape for a selected bait type.
        baitType = type;
        ApplyLabel();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (eaten)
        {
            return;
        }

        FishPlayerController player = other.GetComponentInParent<FishPlayerController>();
        if (player == null)
        {
            return;
        }

        // Bait remains collectable during capture tools so the player can keep
        // making greedy scoring choices while dodging.
        FishGameManager gameManager = FishGameManager.Instance;
        if (gameManager == null || !gameManager.CanEatBait)
        {
            return;
        }

        eaten = true;
        gameManager.EatBait(this);
        Destroy(gameObject);
    }

    public void ApplyLabel()
    {
        if (label == null)
        {
            return;
        }

        label.text = GetStats(baitType).shortLabel;
    }

    public static FishBaitStats GetStats(FishBaitType type)
    {
        // This switch is the prototype's bait balance table. Add new bait types
        // here, then handle special behavior in FishGameManager.ApplyBaitEffect.
        switch (type)
        {
            case FishBaitType.Shrimp:
                return new FishBaitStats("Shrimp", "Shrimp\nNEXT x2", 0, 15f, new Color(1f, 0.55f, 0.45f));
            case FishBaitType.GlowBug:
                return new FishBaitStats("Glow Bug", "Glow Bug\n+1 MULT", 0, 20f, new Color(0.5f, 1f, 0.7f));
            case FishBaitType.SmallFish:
                return new FishBaitStats("Small Fish", "Small Fish\nREPEAT", 0, 20f, new Color(0.6f, 0.85f, 1f));
            case FishBaitType.GoldenShrimp:
                return new FishBaitStats("Golden Shrimp", "GOLD\n+100", 100, 35f, new Color(1f, 0.86f, 0.2f));
            case FishBaitType.FakeBait:
                return new FishBaitStats("Fake Bait", "Fake?\n+0", 0, 50f, new Color(0.95f, 0.25f, 0.25f));
            default:
                return new FishBaitStats("Worm", "Worm\n+10", 10, 10f, new Color(1f, 0.8f, 0.5f));
        }
    }
}
