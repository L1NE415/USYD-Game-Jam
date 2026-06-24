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
    public TMP_Text gameOverText;
    public GameObject gameOverPanel;
    public Image alertFill;
    public Image netSweepFill;

    private void Awake()
    {
        EnsureGameOverOverlay();
    }

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
        SetText(netSweepText, game.CaptureToolName + " " + Mathf.CeilToInt(game.CaptureToolProgress * 100f) + "%");
        SetText(gameOverText, "CAUGHT IN THE NET\nFinal Score: " + game.TotalScore + "\nPress R to restart");

        if (alertFill != null)
        {
            // Alert shifts color when the fisherman is close to noticing.
            alertFill.fillAmount = Mathf.Clamp01(game.Alert / 100f);
            alertFill.color = game.Alert >= 80f ? new Color(1f, 0.12f, 0.08f) : new Color(1f, 0.62f, 0.18f);
        }

        // This panel is a compact net-sweep warning meter.
        bool showNetSweep = game.State == FishGameState.FishingHazard;
        if (netSweepPanel != null)
        {
            netSweepPanel.SetActive(showNetSweep);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(game.IsGameOver);
        }

        if (netSweepFill != null)
        {
            netSweepFill.fillAmount = Mathf.Clamp01(game.CaptureToolProgress);
        }
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private void EnsureGameOverOverlay()
    {
        if (gameOverPanel != null)
        {
            return;
        }

        GameObject panel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(760f, 300f);

        Image panelImage = panel.GetComponent<Image>();
        panelImage.color = new Color(0.04f, 0.06f, 0.08f, 0.9f);

        GameObject textObject = new GameObject("GameOverText", typeof(RectTransform));
        textObject.transform.SetParent(panel.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(32f, 24f);
        textRect.offsetMax = new Vector2(-32f, -24f);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.font = statusText != null ? statusText.font : TMP_Settings.defaultFontAsset;
        text.fontSize = 46f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;

        gameOverPanel = panel;
        gameOverText = text;
        gameOverPanel.SetActive(false);
    }
}
