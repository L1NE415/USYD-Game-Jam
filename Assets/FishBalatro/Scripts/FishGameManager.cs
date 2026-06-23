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
//   caught during the hook escape, this amount is removed from TotalScore.
// - Alert pushes the player into the hook escape phase when it reaches 100.
public class FishGameManager : MonoBehaviour
{
    public static FishGameManager Instance { get; private set; }

    [Header("Scene References")]
    public FishPlayerController player;
    public FishermanController fisherman;
    public FishingLineView fishingLine;
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

    [Header("Hook Escape")]
    public float waterSurfaceY = 3.55f;
    public float hookGripStart = 100f;
    public float pullSpeed = 1.9f;
    public float reelYankSpeed = 3.1f;
    public float wiggleReductionPerSecond = 11f;
    public float wiggleTurnBonus = 3f;
    public float perfectBurstReduction = 30f;
    public float badBurstReduction = 8f;
    public float reelPulseInterval = 1.45f;
    public float reelWarningSeconds = 0.42f;
    public float reelYankSeconds = 0.18f;

    private FishGameState state = FishGameState.Normal;
    private int totalScore;
    private int currentRunScore;
    private int multiplier = 1;
    private int nextBaitMultiplier = 1;
    private int level = 1;
    private float alert;
    private float hookGrip;
    private bool hasPreviousEffect;
    private FishBaitType previousEffectType;
    private float reelTimer;
    private float reelWarningTimer;
    private float reelYankTimer;
    private int lastHorizontalSign;
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
    public float HookGrip => hookGrip;

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
        hookGrip = hookGripStart;
        reelTimer = reelPulseInterval;

        if (fishingLine != null)
        {
            fishingLine.SetHooked(false);
        }

        if (baitSpawner != null)
        {
            baitSpawner.SpawnOpeningBaits(7);
        }
    }

    private void Update()
    {
        // While hooked, the normal attack key is disabled because the player is
        // busy escaping the line.
        if (state == FishGameState.Hooked)
        {
            UpdateHooked();
        }
        else if (state == FishGameState.Normal && ReadAttackPressed())
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

    public Vector2 GetHookPullVelocity()
    {
        if (state != FishGameState.Hooked || fisherman == null || player == null)
        {
            return Vector2.zero;
        }

        Vector2 direction = ((Vector2)fisherman.HookAnchorPosition - (Vector2)player.transform.position).normalized;
        float speed = pullSpeed + (reelYankTimer > 0f ? reelYankSpeed : 0f);
        return direction * speed;
    }

    public void OnPlayerBurstDash()
    {
        if (state != FishGameState.Hooked)
        {
            return;
        }

        bool perfect = reelWarningTimer > 0f;
        float reduction = perfect ? perfectBurstReduction : badBurstReduction;
        ReduceHookGrip(reduction);
        ShowPopup(player.transform.position + Vector3.up * 0.55f, perfect ? "PERFECT DASH!" : "Dash struggle", perfect ? Color.cyan : Color.white);
        StatusText = perfect ? "Perfect dash! The hook slips." : "Dash fights the line.";
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
            fishingLine.SetHooked(false);
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
            BeginHooked();
        }
    }

    private void BeginHooked()
    {
        if (state == FishGameState.Hooked)
        {
            return;
        }

        // Alert full is not instant death. It starts the short escape minigame.
        state = FishGameState.Hooked;
        hookGrip = hookGripStart;
        reelTimer = reelPulseInterval;
        reelWarningTimer = 0f;
        reelYankTimer = 0f;
        lastHorizontalSign = 0;
        statusText = "FISHERMAN NOTICED YOU! ESCAPE!";

        if (fisherman != null)
        {
            fisherman.SetNotice(true);
        }

        if (fishingLine != null)
        {
            fishingLine.SetHooked(true);
        }

        if (player != null)
        {
            ShowPopup(player.transform.position + Vector3.up * 0.85f, "ESCAPE!", Color.red);
        }
    }

    private void UpdateHooked()
    {
        if (player == null)
        {
            return;
        }

        HandleWiggle();
        HandleReelPulse();

        if (player.transform.position.y >= waterSurfaceY)
        {
            CatchFish();
            return;
        }

        if (hookGrip <= 0f)
        {
            EscapeHook();
        }
    }

    private void HandleWiggle()
    {
        // Alternating left/right input lowers Hook Grip faster than holding one
        // direction. This is intentionally simple and readable for jam tuning.
        float horizontal = player.MoveInput.x;
        if (Mathf.Abs(horizontal) <= 0.15f)
        {
            return;
        }

        ReduceHookGrip(wiggleReductionPerSecond * Time.deltaTime);

        int sign = horizontal > 0f ? 1 : -1;
        if (lastHorizontalSign != 0 && sign != lastHorizontalSign)
        {
            ReduceHookGrip(wiggleTurnBonus);
        }

        lastHorizontalSign = sign;
    }

    private void HandleReelPulse()
    {
        // Reel pulse loop: warn briefly, then yank. A dash during the warning is
        // rewarded in OnPlayerBurstDash.
        reelTimer -= Time.deltaTime;
        reelWarningTimer = Mathf.Max(0f, reelWarningTimer - Time.deltaTime);
        reelYankTimer = Mathf.Max(0f, reelYankTimer - Time.deltaTime);

        bool warning = reelWarningTimer > 0f;

        if (reelTimer <= 0f && !warning)
        {
            reelWarningTimer = reelWarningSeconds;
            reelTimer = reelPulseInterval + reelWarningSeconds;
            warning = true;
            StatusText = "Line flashing: dash sideways now!";
        }

        if (warning && reelWarningTimer <= Time.deltaTime)
        {
            reelYankTimer = reelYankSeconds;
            StatusText = "The fisherman yanks the line!";
        }

        if (fisherman != null)
        {
            fisherman.SetReelWarning(warning);
        }

        if (fishingLine != null)
        {
            fishingLine.SetWarning(warning);
        }
    }

    private void EscapeHook()
    {
        if (fisherman != null)
        {
            fisherman.SetNotice(false);
            fisherman.SetReelWarning(false);
        }

        if (fishingLine != null)
        {
            fishingLine.SetHooked(false);
        }

        // Escaping keeps TotalScore. It only clears the current risk/combo state.
        ResetRunState();
        statusText = "ESCAPED! Keep stealing or press E to attack.";
        comboText = "";
        StartCoroutine(ReturnToNormalAfter(FishGameState.Recovering, 0.65f));
    }

    private void CatchFish()
    {
        state = FishGameState.Caught;
        ShowPopup(player.transform.position, "CAUGHT! Run lost.", Color.red);
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
            fishingLine.SetHooked(false);
        }

        StartCoroutine(ReturnToNormalAfter(FishGameState.Caught, 0.85f));
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
        hookGrip = hookGripStart;
        reelWarningTimer = 0f;
        reelYankTimer = 0f;
        reelTimer = reelPulseInterval;
    }

    private void ReduceHookGrip(float amount)
    {
        hookGrip = Mathf.Max(0f, hookGrip - amount);
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
