using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Thin UI adapter. FishGameManager owns the values; this script only copies
// them into TextMeshPro labels and progress bars.
public class FishUIController : MonoBehaviour
{
    public TMP_Text totalScoreText;
    public TMP_Text currentRunText;
    public TMP_Text multiplierText;
    public TMP_Text alertText;
    public TMP_Text levelText;
    public TMP_Text attackCostText;
    public TMP_Text statusText;
    public TMP_Text comboText;
    public TMP_Text hookGripText;
    public GameObject hookGripPanel;
    public Image alertFill;
    public Image hookGripFill;

    public void UpdateFrom(FishGameManager game)
    {
        if (game == null)
        {
            return;
        }

        SetText(totalScoreText, "Total: " + game.TotalScore);
        SetText(currentRunText, "At Risk: " + game.CurrentRunScore);
        SetText(multiplierText, "Mult x" + game.Multiplier + "  Next x" + game.NextBaitMultiplier);
        SetText(alertText, "Alert " + Mathf.RoundToInt(game.Alert) + "%");
        SetText(levelText, "Level " + game.Level);
        SetText(attackCostText, "Press E Attack: " + game.AttackCost);
        SetText(statusText, game.StatusText);
        SetText(comboText, game.ComboText);
        SetText(hookGripText, "Hook Grip " + Mathf.CeilToInt(game.HookGrip) + "%");

        if (alertFill != null)
        {
            // Alert shifts color when the fisherman is close to noticing.
            alertFill.fillAmount = Mathf.Clamp01(game.Alert / 100f);
            alertFill.color = game.Alert >= 80f ? new Color(1f, 0.12f, 0.08f) : new Color(1f, 0.62f, 0.18f);
        }

        // Hook Grip is hidden during normal swimming and appears only in escape.
        bool showHook = game.State == FishGameState.Hooked;
        if (hookGripPanel != null)
        {
            hookGripPanel.SetActive(showHook);
        }

        if (hookGripFill != null)
        {
            hookGripFill.fillAmount = Mathf.Clamp01(game.HookGrip / 100f);
        }
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }
}
