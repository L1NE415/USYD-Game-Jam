using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Thin UI adapter. FishGameManager owns the values; this script only copies
// them into TextMeshPro labels and progress bars.
[ExecuteAlways]
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
    public Sprite scorePanelSprite;
    public Sprite multiplierPanelSprite;
    public Sprite alertPanelSprite;
    public Sprite controlsPanelSprite;
    public Sprite keycapSprite;
    public Sprite alertSegmentSprite;
    public Image scorePanelImage;
    public Image multiplierPanelImage;
    public Image alertPanelImage;
    public Image controlsPanelImage;
    public GameObject controlsPanel;
    public TMP_Text controlsText;
    public SpriteRenderer controlsWorldPanelRenderer;

    private void Awake()
    {
        if (!Application.isPlaying)
        {
            SetCanvasRendering(false);
            return;
        }

        SetCanvasRendering(true);
        EnsureHudLayout();
        EnsureGameOverOverlay();
    }

    private void OnEnable()
    {
        if (!Application.isPlaying)
        {
            SetCanvasRendering(false);
        }
    }

    public void UpdateFrom(FishGameManager game)
    {
        if (game == null)
        {
            return;
        }

        SetText(totalScoreText, "SCORE\n" + FormatScore(game.TotalScore));
        SetText(currentRunText, string.Empty);
        SetText(multiplierText, "MULTIPLIER\n<color=#ffc530>x" + game.Multiplier + "</color>");
        SetText(alertText, "<color=#ff5a50>ALERT METER</color>   " + Mathf.RoundToInt(game.Alert) + "%");
        SetText(levelText, "Level " + game.Level);
        SetText(attackCostText, "Press E Attack: " + game.AttackCost);
        SetText(statusText, game.StatusText);
        SetText(controlsText, "<color=#ff5a50>ESCAPE!</color>\nWASD / ARROWS   SWIM\nSHIFT   BURST\nE   ATTACK " + game.AttackCost + "\nR   RESTART");
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

    private static string FormatScore(int score)
    {
        return score.ToString("N0", CultureInfo.InvariantCulture);
    }

    private void SetCanvasRendering(bool enabled)
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.enabled = enabled;
        }

        GraphicRaycaster raycaster = GetComponent<GraphicRaycaster>();
        if (raycaster != null)
        {
            raycaster.enabled = enabled;
        }
    }

    private void EnsureHudLayout()
    {
        scorePanelSprite = scorePanelSprite != null ? scorePanelSprite : CreatePanelSprite(142, 78, new Color32(96, 173, 222, 255));
        multiplierPanelSprite = multiplierPanelSprite != null ? multiplierPanelSprite : CreatePanelSprite(142, 78, new Color32(255, 191, 48, 255));
        alertPanelSprite = alertPanelSprite != null ? alertPanelSprite : CreateAlertPanelSprite();
        controlsPanelSprite = controlsPanelSprite != null ? controlsPanelSprite : CreatePanelSprite(260, 118, new Color32(255, 86, 75, 255));
        alertSegmentSprite = alertSegmentSprite != null ? alertSegmentSprite : CreateAlertSegmentSprite();

        RectTransform scoreRoot = EnsureContainer("Score UI", transform);
        RectTransform alertRoot = EnsureContainer("Alert UI", transform);
        RectTransform controlsRoot = EnsureContainer("Controls UI", transform);
        controlsRoot.gameObject.SetActive(false);

        scorePanelImage = EnsurePanelImage(scoreRoot, "Score Panel", scorePanelSprite, new Vector2(0f, 1f), new Vector2(24f, -18f), new Vector2(250f, 86f), TextAnchor.UpperLeft);
        multiplierPanelImage = EnsurePanelImage(scoreRoot, "Multiplier Panel", multiplierPanelSprite, new Vector2(0f, 1f), new Vector2(292f, -18f), new Vector2(250f, 86f), TextAnchor.UpperLeft);
        alertPanelImage = EnsurePanelImage(alertRoot, "Alert Meter Panel", alertPanelSprite, new Vector2(1f, 1f), new Vector2(-24f, -18f), new Vector2(500f, 72f), TextAnchor.UpperRight);
        controlsPanelImage = null;

        ConfigureText(totalScoreText, scoreRoot, new Vector2(0f, 1f), new Vector2(42f, -27f), new Vector2(214f, 68f), 28f, Color.white, TextAlignmentOptions.Center);
        ConfigureText(multiplierText, scoreRoot, new Vector2(0f, 1f), new Vector2(310f, -27f), new Vector2(214f, 68f), 28f, Color.white, TextAlignmentOptions.Center);
        ConfigureText(alertText, alertRoot, new Vector2(1f, 1f), new Vector2(-44f, -26f), new Vector2(430f, 30f), 24f, Color.white, TextAlignmentOptions.Center);
        ConfigureText(comboText, transform, new Vector2(0.5f, 0f), new Vector2(0f, 112f), new Vector2(980f, 44f), 28f, new Color(1f, 0.92f, 0.56f), TextAlignmentOptions.Center);

        if (alertFill != null)
        {
            alertFill.sprite = alertSegmentSprite;
            alertFill.type = Image.Type.Filled;
            alertFill.fillMethod = Image.FillMethod.Horizontal;
            alertFill.fillOrigin = 0;
            alertFill.preserveAspect = false;
        }

        if (alertFill != null && alertFill.transform.parent != null)
        {
            RectTransform bar = alertFill.transform.parent.GetComponent<RectTransform>();
            if (bar != null)
            {
                bar.SetParent(alertRoot, false);
                bar.anchorMin = new Vector2(1f, 1f);
                bar.anchorMax = new Vector2(1f, 1f);
                bar.pivot = new Vector2(1f, 1f);
                bar.anchoredPosition = new Vector2(-44f, -54f);
                bar.sizeDelta = new Vector2(430f, 22f);
            }
        }

        EnsureWorldControlsHint();

        SetObjectActive(levelText, false);
        SetObjectActive(currentRunText, false);
        SetObjectActive(attackCostText, false);
        SetObjectActive(statusText, false);
    }

    private static RectTransform EnsureContainer(string name, Transform parent)
    {
        Transform existing = parent.Find(name);
        GameObject container = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        container.transform.SetParent(parent, false);

        RectTransform rect = container.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        return rect;
    }

    private Image EnsurePanelImage(RectTransform parent, string name, Sprite sprite, Vector2 anchor, Vector2 anchoredPosition, Vector2 size, TextAnchor corner)
    {
        Transform existing = parent.Find(name);
        GameObject panel = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        panel.transform.SetAsFirstSibling();

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = corner == TextAnchor.UpperRight || corner == TextAnchor.LowerRight ? new Vector2(1f, anchor.y) : new Vector2(0f, anchor.y);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Image image = panel.GetComponent<Image>();
        image.sprite = sprite;
        image.type = sprite != null && sprite.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
        image.color = Color.white;
        return image;
    }

    private void EnsureWorldControlsHint()
    {
        GameObject root = controlsPanel != null ? controlsPanel : GameObject.Find("Controls Hint");
        if (root == null)
        {
            root = new GameObject("Controls Hint");
        }

        controlsPanel = root;
        root.transform.position = new Vector3(6.35f, -3.72f, 0f);
        root.transform.localScale = Vector3.one;

        Transform panelTransform = root.transform.Find("Panel");
        GameObject panel = panelTransform != null ? panelTransform.gameObject : new GameObject("Panel");
        panel.transform.SetParent(root.transform, false);
        panel.transform.localPosition = new Vector3(0.96f, -0.65f, 0f);
        panel.transform.localScale = new Vector3(1.25f, 1.15f, 1f);

        controlsWorldPanelRenderer = panel.GetComponent<SpriteRenderer>();
        if (controlsWorldPanelRenderer == null)
        {
            controlsWorldPanelRenderer = panel.AddComponent<SpriteRenderer>();
        }

        controlsWorldPanelRenderer.sprite = controlsPanelSprite;
        controlsWorldPanelRenderer.color = new Color(1f, 1f, 1f, 0.56f);
        controlsWorldPanelRenderer.sortingOrder = 6;

        Transform textTransform = root.transform.Find("Text");
        GameObject textObject = textTransform != null ? textTransform.gameObject : new GameObject("Text");
        textObject.transform.SetParent(root.transform, false);
        textObject.transform.localPosition = new Vector3(0.98f, -0.65f, 0f);

        TextMeshPro text = textObject.GetComponent<TextMeshPro>();
        if (text == null)
        {
            text = textObject.AddComponent<TextMeshPro>();
        }

        controlsText = text;
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = 1.8f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.richText = true;
        text.rectTransform.sizeDelta = new Vector2(3.2f, 1.2f);

        MeshRenderer textRenderer = text.GetComponent<MeshRenderer>();
        if (textRenderer != null)
        {
            textRenderer.sortingOrder = 7;
        }
    }

    private static TextMeshProUGUI CreateHudText(Transform parent, string name)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.raycastTarget = false;
        return text;
    }

    private static void ConfigureText(TMP_Text text, Transform parent, Vector2 anchor, Vector2 anchoredPosition, Vector2 size, float fontSize, Color color, TextAlignmentOptions alignment)
    {
        if (text == null)
        {
            return;
        }

        text.transform.SetParent(parent, false);
        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = new Vector2(anchor.x, anchor.y);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.richText = true;
    }

    private static void SetObjectActive(TMP_Text text, bool active)
    {
        if (text != null)
        {
            text.gameObject.SetActive(active);
        }
    }

    private static Sprite CreatePanelSprite(int width, int height, Color32 accent)
    {
        Color32[] pixels = new Color32[width * height];
        FillRect(pixels, width, height, 0, 0, width, height, new Color32(0, 0, 0, 0));
        FillRect(pixels, width, height, 5, 5, width - 5, height - 5, new Color32(4, 8, 13, 150));
        FillRect(pixels, width, height, 0, 0, width - 7, height - 7, new Color32(13, 23, 34, 238));
        FillRect(pixels, width, height, 7, 7, width - 21, height - 21, new Color32(22, 38, 54, 220));
        DrawRect(pixels, width, height, 1, 1, width - 9, height - 9, new Color32(9, 15, 24, 255), 4);
        DrawRect(pixels, width, height, 5, 5, width - 17, height - 17, new Color32(49, 75, 96, 255), 3);
        DrawLine(pixels, width, height, 12, 9, width - 28, 9, accent);

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 1, SpriteMeshType.FullRect, new Vector4(8f, 8f, 8f, 8f));
    }

    private static Sprite CreateAlertPanelSprite()
    {
        int width = 420;
        int height = 60;
        Color32[] pixels = new Color32[width * height];
        FillRect(pixels, width, height, 0, 0, width, height, new Color32(0, 0, 0, 0));
        FillRect(pixels, width, height, 5, 5, width - 5, height - 5, new Color32(4, 8, 13, 150));
        FillRect(pixels, width, height, 0, 0, width - 7, height - 7, new Color32(13, 23, 34, 238));
        FillRect(pixels, width, height, 7, 7, width - 21, height - 21, new Color32(22, 38, 54, 220));
        DrawRect(pixels, width, height, 1, 1, width - 9, height - 9, new Color32(9, 15, 24, 255), 4);
        DrawRect(pixels, width, height, 5, 5, width - 17, height - 17, new Color32(49, 75, 96, 255), 3);

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 1, SpriteMeshType.FullRect, new Vector4(8f, 8f, 8f, 8f));
    }

    private static Sprite CreateAlertSegmentSprite()
    {
        int width = 26;
        int height = 24;
        Color32[] pixels = new Color32[width * height];
        FillRect(pixels, width, height, 0, 0, width, height, new Color32(0, 0, 0, 0));
        FillRect(pixels, width, height, 1, 1, 24, 22, new Color32(92, 26, 34, 255));
        FillRect(pixels, width, height, 3, 4, 20, 17, new Color32(188, 45, 51, 255));
        FillRect(pixels, width, height, 4, 5, 18, 5, new Color32(255, 84, 74, 255));

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 1, SpriteMeshType.FullRect, new Vector4(2f, 2f, 2f, 2f));
    }

    private static void FillRect(Color32[] pixels, int width, int height, int x, int y, int rectWidth, int rectHeight, Color32 color)
    {
        for (int py = Mathf.Max(0, y); py < Mathf.Min(height, y + rectHeight); py++)
        {
            for (int px = Mathf.Max(0, x); px < Mathf.Min(width, x + rectWidth); px++)
            {
                pixels[py * width + px] = color;
            }
        }
    }

    private static void DrawRect(Color32[] pixels, int width, int height, int x, int y, int rectWidth, int rectHeight, Color32 color, int thickness)
    {
        FillRect(pixels, width, height, x, y, rectWidth, thickness, color);
        FillRect(pixels, width, height, x, y + rectHeight - thickness, rectWidth, thickness, color);
        FillRect(pixels, width, height, x, y, thickness, rectHeight, color);
        FillRect(pixels, width, height, x + rectWidth - thickness, y, thickness, rectHeight, color);
    }

    private static void DrawLine(Color32[] pixels, int width, int height, int x0, int y0, int x1, int y1, Color32 color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            if (x0 >= 0 && x0 < width && y0 >= 0 && y0 < height)
            {
                pixels[y0 * width + x0] = color;
            }

            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            int e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x0 += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                y0 += sy;
            }
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
        text.fontSize = 2f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;

        gameOverPanel = panel;
        gameOverText = text;
        gameOverPanel.SetActive(false);
    }
}
