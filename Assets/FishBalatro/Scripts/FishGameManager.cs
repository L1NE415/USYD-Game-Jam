using System.Collections;
using TMPro;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

// Central gameplay state machine.
//
// Score model for the current design:
// - TotalScore is the visible score and also the currency spent when pressing E to attack.
// - CurrentRunScore is the amount gained during the current greed streak; if the player is
//   caught by the net sweep, this amount is removed from TotalScore.
// - Alert triggers a large pendulum net sweep when it reaches 100.
public class FishGameManager : MonoBehaviour
{
    public static FishGameManager Instance { get; private set; }

    [Header("Scene References")]
    public FishPlayerController player;
    public FishermanController fisherman;
    public FishingLineView fishingLine;
    public NetSweepHazard netSweep;
    public BaitSpawner baitSpawner;
    public BigFishAlly bigFish;
    public FishUIController ui;
    public TMP_FontAsset popupFont;
    public Transform playerRespawn;

    [Header("Scoring")]
    // Attack cost is level-based so later fishermen need more stolen bait before
    // the big fish can scare them away.
    public int baseAttackCost = 240;
    public int attackCostStep = 140;

    [Header("Net Sweep")]
    public float netDodgedRecoverySeconds = 0.55f;
    public float netCaughtRecoverySeconds = 0.85f;

    private FishGameState state = FishGameState.Normal;
    private int totalScore;
    private int currentRunScore;
    private int multiplier = 1;
    private int nextBaitMultiplier = 1;
    private int level = 1;
    private float alert;
    private bool hasPreviousEffect;
    private FishBaitType previousEffectType;
    private float stateMessageTimer;
    private string statusText = "Steal bait for score. Press E to attack the fisherman.";
    private string comboText = "";

    public FishGameState State => state;
    public bool CanEatBait => state == FishGameState.Normal;
    public bool CanCallBigFish => state == FishGameState.Normal && totalScore >= AttackCost;
    public int AttackCost => baseAttackCost + (level - 1) * attackCostStep;
    public int TotalScore => totalScore;
    public int CurrentRunScore => currentRunScore;
    public int Multiplier => multiplier;
    public int NextBaitMultiplier => nextBaitMultiplier;
    public int Level => level;
    public float Alert => alert;
    public float NetSweepProgress => state == FishGameState.NetSweep && netSweep != null ? netSweep.Progress : 0f;

    public string StatusText
    {
        get => statusText;
        set
        {
            statusText = value;
            stateMessageTimer = 0.2f;
        }
    }

    public string ComboText => comboText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (fishingLine != null)
        {
            fishingLine.SetLineVisible(false);
        }

        if (netSweep != null)
        {
            netSweep.Hide();
        }

        if (baitSpawner != null)
        {
            baitSpawner.SpawnOpeningBaits(7);
        }
    }

    private void Update()
    {
        if (state == FishGameState.Normal && ReadAttackPressed())
        {
            TryCallBigFish();
        }

        if (stateMessageTimer > 0f)
        {
            stateMessageTimer -= Time.deltaTime;
        }
        else if (state == FishGameState.Normal)
        {
            statusText = "Eat bait for score. Press E to attack when you can pay the cost.";
        }

        if (ui != null)
        {
            ui.UpdateFrom(this);
        }
    }

    public void EatBait(BaitPickup bait)
    {
        if (bait == null || !CanEatBait)
        {
            return;
        }

        // Small Fish repeats the previous effect, so we remember the effect that
        // should be copied next time.
        FishBaitType memoryType = bait.baitType;
        ApplyBaitEffect(bait.baitType, bait.transform.position, true);

        if (bait.baitType == FishBaitType.SmallFish && hasPreviousEffect)
        {
            memoryType = previousEffectType;
        }

        previousEffectType = memoryType;
        hasPreviousEffect = true;
    }

    public void OnPlayerBurstDash()
    {
        if (state == FishGameState.NetSweep)
        {
            StatusText = "Dash away from the sweeping net.";
            return;
        }
    }

    public void TryCallBigFish()
    {
        if (!CanCallBigFish)
        {
            StatusText = "Need " + AttackCost + " score to attack.";
            return;
        }

        StartCoroutine(BigFishAttackRoutine());
    }

    private IEnumerator BigFishAttackRoutine()
    {
        // The big fish attack is the level transition: spend score, clear current
        // pressure, make the fisherman flee, then spawn the next set of bait.
        state = FishGameState.BigFishAttack;
        int cost = AttackCost;
        totalScore = Mathf.Max(0, totalScore - cost);
        ResetRunState();
        statusText = "ATTACK! -" + cost + " score";
        comboText = "";

        if (baitSpawner != null)
        {
            baitSpawner.ClearBaits();
        }

        if (fishingLine != null)
        {
            fishingLine.SetLineVisible(false);
        }

        if (netSweep != null)
        {
            netSweep.Hide();
        }

        if (fisherman != null)
        {
            fisherman.SetNotice(true);
        }

        if (bigFish != null && fisherman != null)
        {
            yield return bigFish.PlayAttack(fisherman.transform.position);
        }
        else
        {
            yield return new WaitForSeconds(0.6f);
        }

        int nextLevel = level + 1;

        if (fisherman != null)
        {
            yield return fisherman.FleeAndReturn(nextLevel);
        }

        level = nextLevel;
        statusText = "Level " + level + ": new fisherman, greedier bait.";
        state = FishGameState.Normal;

        if (baitSpawner != null)
        {
            baitSpawner.SpawnOpeningBaits(6 + Mathf.Min(level, 4));
        }
    }

    private void ApplyBaitEffect(FishBaitType type, Vector3 worldPosition, bool addAlert)
    {
        FishBaitStats stats = BaitPickup.GetStats(type);

        // Setup baits change combo state; scoring baits call AwardScore.
        switch (type)
        {
            case FishBaitType.Shrimp:
                nextBaitMultiplier *= 2;
                comboText = "Shrimp: next scoring bait x" + nextBaitMultiplier;
                ShowPopup(worldPosition, "NEXT x" + nextBaitMultiplier, stats.popupColor);
                break;
            case FishBaitType.GlowBug:
                multiplier += 1;
                comboText = "Glow Bug: multiplier is now x" + multiplier;
                ShowPopup(worldPosition, "MULT x" + multiplier, stats.popupColor);
                break;
            case FishBaitType.SmallFish:
                if (hasPreviousEffect)
                {
                    comboText = "Small Fish repeats " + BaitPickup.GetStats(previousEffectType).displayName;
                    ShowPopup(worldPosition, "REPEAT!", stats.popupColor);
                    ApplyBaitEffect(previousEffectType, worldPosition + Vector3.up * 0.25f, false);
                }
                else
                {
                    comboText = "Small Fish had nothing to repeat.";
                    ShowPopup(worldPosition, "No repeat", Color.gray);
                }
                break;
            case FishBaitType.FakeBait:
                comboText = "Fake bait! All danger, no score.";
                ShowPopup(worldPosition, "FAKE!", stats.popupColor);
                break;
            default:
                AwardScore(stats.baseScore, worldPosition, stats);
                break;
        }

        if (addAlert)
        {
            AddAlert(stats.alertIncrease);
        }
    }

    private void AwardScore(int baseScore, Vector3 worldPosition, FishBaitStats stats)
    {
        // Score immediately becomes spendable, but the same gained amount remains
        // at risk until the current greed streak is reset by escape/attack/caught.
        int gained = baseScore * multiplier * nextBaitMultiplier;
        totalScore += gained;
        currentRunScore += gained;
        comboText = stats.displayName + ": +" + gained + " (" + baseScore + " x" + multiplier + " x" + nextBaitMultiplier + ")";
        nextBaitMultiplier = 1;
        ShowPopup(worldPosition, "+" + gained, stats.popupColor);
    }

    private void AddAlert(float amount)
    {
        alert = Mathf.Clamp(alert + amount, 0f, 100f);

        if (alert >= 100f)
        {
            BeginNetSweep();
        }
    }

    private void BeginNetSweep()
    {
        if (state != FishGameState.Normal)
        {
            return;
        }

        StartCoroutine(NetSweepRoutine());
    }

    private IEnumerator NetSweepRoutine()
    {
        // Alert full is not instant death. It starts a large, readable sweep
        // hazard that the player can dodge through movement.
        state = FishGameState.NetSweep;
        statusText = "FISHERMAN DROPS A NET! DODGE THE SWING!";

        if (fisherman != null)
        {
            fisherman.SetNotice(true);
            fisherman.SetReelWarning(true);
        }

        if (fishingLine != null)
        {
            fishingLine.SetLineVisible(false);
        }

        if (player != null)
        {
            ShowPopup(player.transform.position + Vector3.up * 0.85f, "NET!", Color.red);
        }

        if (netSweep == null)
        {
            netSweep = NetSweepHazard.CreateRuntimeNet();
        }

        yield return netSweep.PlaySweep(player, level);

        if (netSweep.CaughtPlayer)
        {
            CatchFish();
        }
        else
        {
            EscapeNet();
        }
    }

    private void EscapeNet()
    {
        if (fisherman != null)
        {
            fisherman.SetNotice(false);
            fisherman.SetReelWarning(false);
        }

        if (fishingLine != null)
        {
            fishingLine.SetLineVisible(false);
        }

        // Dodging the net keeps TotalScore. It only clears the current
        // risk/combo state so the player can start a fresh greed streak.
        ResetRunState();
        statusText = "DODGED THE NET! Keep stealing or press E to attack.";
        comboText = "";
        StartCoroutine(ReturnToNormalAfter(FishGameState.Recovering, netDodgedRecoverySeconds));
    }

    private void CatchFish()
    {
        state = FishGameState.Caught;
        ShowPopup(player.transform.position, "NETTED! Run lost.", Color.red);
        // Getting caught loses only the current greed streak, not the whole game.
        if (currentRunScore > 0)
        {
            totalScore = Mathf.Max(0, totalScore - currentRunScore);
        }
        ResetRunState();

        if (playerRespawn != null)
        {
            player.ResetTo(playerRespawn.position);
        }

        if (fisherman != null)
        {
            fisherman.SetNotice(false);
            fisherman.SetReelWarning(false);
        }

        if (fishingLine != null)
        {
            fishingLine.SetLineVisible(false);
        }

        if (netSweep != null)
        {
            netSweep.Hide();
        }

        StartCoroutine(ReturnToNormalAfter(FishGameState.Caught, netCaughtRecoverySeconds));
    }

    private IEnumerator ReturnToNormalAfter(FishGameState temporaryState, float delay)
    {
        state = temporaryState;
        yield return new WaitForSeconds(delay);
        if (state == temporaryState)
        {
            state = FishGameState.Normal;
        }
    }

    private void ResetRunState()
    {
        // Called whenever a streak ends: attack, escape, or caught.
        alert = 0f;
        currentRunScore = 0;
        multiplier = 1;
        nextBaitMultiplier = 1;
        hasPreviousEffect = false;
    }

    private void ShowPopup(Vector3 position, string message, Color color)
    {
        if (popupFont != null)
        {
            FloatingText.Spawn(position, message, color, popupFont);
        }
    }

    private static bool ReadAttackPressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.eKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.E);
#endif
    }
}
