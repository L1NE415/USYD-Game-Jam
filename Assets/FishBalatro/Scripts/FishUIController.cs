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
    public TMP_Text netSweepText;
    public GameObject netSweepPanel;
    public Image alertFill;
    public Image netSweepFill;

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
        SetText(netSweepText, "Net Sweep " + Mathf.CeilToInt(game.NetSweepProgress * 100f) + "%");

        if (alertFill != null)
        {
            // Alert shifts color when the fisherman is close to noticing.
            alertFill.fillAmount = Mathf.Clamp01(game.Alert / 100f);
            alertFill.color = game.Alert >= 80f ? new Color(1f, 0.12f, 0.08f) : new Color(1f, 0.62f, 0.18f);
        }

        // This panel is a compact net-sweep warning meter.
        bool showNetSweep = game.State == FishGameState.NetSweep;
        if (netSweepPanel != null)
        {
            netSweepPanel.SetActive(showNetSweep);
        }

        if (netSweepFill != null)
        {
            netSweepFill.fillAmount = Mathf.Clamp01(game.NetSweepProgress);
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
