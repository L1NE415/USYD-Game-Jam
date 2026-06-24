using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

// Central gameplay state machine.
//
// Score model for the current design:
// - TotalScore is the visible score and also the currency spent when pressing E to attack.
// - CurrentRunScore is the amount gained during the current greed streak; dodging a tool
//   clears this risk, while getting caught ends the run.
// - Alert triggers the current fisherman's capture tool when it reaches 100.
public class FishGameManager : MonoBehaviour
{
    public static FishGameManager Instance { get; private set; }

    [Header("Scene References")]
    public FishPlayerController player;
    public FishermanController fisherman;
    public FishingLineView fishingLine;
    public NetSweepHazard netSweep;
    public ClawShotHazard clawShot;
    public ElectricWaveHazard electricWave;
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

    [Header("Capture Tools")]
    public float netDodgedRecoverySeconds = 0.55f;

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
    public FishFishermanType CurrentFishermanType => GetFishermanTypeForLevel(level);
    public string CaptureToolName => GetCaptureToolName(CurrentFishermanType);
    public float CaptureToolProgress => state == FishGameState.FishingHazard ? GetActiveCaptureToolProgress() : 0f;
    public float NetSweepProgress => CaptureToolProgress;
    public bool IsGameOver => state == FishGameState.Caught;

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

        if (clawShot != null)
        {
            clawShot.Hide();
        }

        if (electricWave != null)
        {
            electricWave.Hide();
        }

        ApplyFishermanVariant();

        if (baitSpawner != null)
        {
            baitSpawner.SpawnOpeningBaits(7);
        }
    }

    private void Update()
    {
        if (state == FishGameState.Caught)
        {
            if (ReadRestartPressed())
            {
                RestartGame();
            }

            if (ui != null)
            {
                ui.UpdateFrom(this);
            }

            return;
        }

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
        if (state == FishGameState.FishingHazard)
        {
            StatusText = "Dash away from the capture tool.";
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

        HideAllCaptureTools();

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

        FishFishermanType nextType = GetFishermanTypeForLevel(nextLevel);
        if (fisherman != null)
        {
            yield return fisherman.FleeAndReturn(nextLevel, nextType);
        }

        level = nextLevel;
        ApplyFishermanVariant();
        statusText = "Level " + level + ": " + CaptureToolName + " fisherman arrives.";
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
            BeginCaptureTool();
        }
    }

    private void BeginCaptureTool()
    {
        if (state != FishGameState.Normal)
        {
            return;
        }

        StartCoroutine(CaptureToolRoutine());
    }

    private IEnumerator CaptureToolRoutine()
    {
        // Alert full is not instant death. It starts the current fisherman's
        // readable capture tool pattern that the player can dodge.
        state = FishGameState.FishingHazard;
        FishFishermanType type = CurrentFishermanType;
        string toolName = GetCaptureToolName(type);
        statusText = toolName.ToUpperInvariant() + " WARNING! DODGE!";

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
            ShowPopup(player.transform.position + Vector3.up * 0.85f, toolName.ToUpperInvariant() + "!", Color.red);
        }

        yield return PlayCaptureTool(type);

        if (WasCaughtByTool(type))
        {
            CatchFish();
        }
        else
        {
            EscapeCaptureTool(toolName);
        }
    }

    private IEnumerator PlayCaptureTool(FishFishermanType type)
    {
        switch (type)
        {
            case FishFishermanType.Claw:
                if (clawShot == null)
                {
                    clawShot = ClawShotHazard.CreateRuntimeClaw();
                }
                yield return clawShot.PlayVolley(player, level);
                break;
            case FishFishermanType.Electric:
                if (electricWave == null)
                {
                    electricWave = ElectricWaveHazard.CreateRuntimeElectric();
                }
                yield return electricWave.PlayWaves(player, level);
                break;
            default:
                if (netSweep == null)
                {
                    netSweep = NetSweepHazard.CreateRuntimeNet();
                }
                yield return netSweep.PlaySweep(player, level);
                break;
        }
    }

    private void EscapeCaptureTool(string toolName)
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

        // Dodging a tool keeps TotalScore. It only clears the current
        // risk/combo state so the player can start a fresh greed streak.
        ResetRunState();
        statusText = "DODGED THE " + toolName.ToUpperInvariant() + "! Keep stealing or press E to attack.";
        comboText = "";
        StartCoroutine(ReturnToNormalAfter(FishGameState.Recovering, netDodgedRecoverySeconds));
    }

    private void CatchFish()
    {
        state = FishGameState.Caught;
        statusText = "CAUGHT BY THE " + CaptureToolName.ToUpperInvariant() + "! Press R to restart.";
        comboText = "Final score: " + totalScore;

        if (player != null)
        {
            ShowPopup(player.transform.position, "CAUGHT!", Color.red);
        }

        if (baitSpawner != null)
        {
            baitSpawner.ClearBaits();
        }

        ResetRunState();

        if (fisherman != null)
        {
            fisherman.SetNotice(true);
            fisherman.SetReelWarning(false);
        }

        if (fishingLine != null)
        {
            fishingLine.SetLineVisible(false);
        }

        // Game over is intentional here. The player restarts the scene with R
        // instead of automatically recovering after touching a capture tool.
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

    private void ApplyFishermanVariant()
    {
        if (fisherman != null)
        {
            fisherman.SetVariant(CurrentFishermanType, level);
        }
    }

    private float GetActiveCaptureToolProgress()
    {
        switch (CurrentFishermanType)
        {
            case FishFishermanType.Claw:
                return clawShot != null ? clawShot.Progress : 0f;
            case FishFishermanType.Electric:
                return electricWave != null ? electricWave.Progress : 0f;
            default:
                return netSweep != null ? netSweep.Progress : 0f;
        }
    }

    private bool WasCaughtByTool(FishFishermanType type)
    {
        switch (type)
        {
            case FishFishermanType.Claw:
                return clawShot != null && clawShot.CaughtPlayer;
            case FishFishermanType.Electric:
                return electricWave != null && electricWave.CaughtPlayer;
            default:
                return netSweep != null && netSweep.CaughtPlayer;
        }
    }

    private void HideAllCaptureTools()
    {
        if (netSweep != null)
        {
            netSweep.Hide();
        }

        if (clawShot != null)
        {
            clawShot.Hide();
        }

        if (electricWave != null)
        {
            electricWave.Hide();
        }
    }

    private static FishFishermanType GetFishermanTypeForLevel(int targetLevel)
    {
        switch (Mathf.Abs(targetLevel - 1) % 3)
        {
            case 1:
                return FishFishermanType.Claw;
            case 2:
                return FishFishermanType.Electric;
            default:
                return FishFishermanType.Net;
        }
    }

    private static string GetCaptureToolName(FishFishermanType type)
    {
        switch (type)
        {
            case FishFishermanType.Claw:
                return "Claw";
            case FishFishermanType.Electric:
                return "Electric Wave";
            default:
                return "Net";
        }
    }

    private void ShowPopup(Vector3 position, string message, Color color)
    {
        if (popupFont != null)
        {
            FloatingText.Spawn(position, message, color, popupFont);
        }
    }

    public void RestartGame()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex >= 0)
        {
            SceneManager.LoadScene(activeScene.buildIndex);
        }
        else
        {
            SceneManager.LoadScene(activeScene.name);
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

    private static bool ReadRestartPressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.rKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.R);
#endif
    }
}
