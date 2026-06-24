using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
    public Sprite gearButtonSprite;
    public Sprite settingsPanelSprite;
    public Button settingsButton;
    public GameObject settingsPanel;
    public TMP_Text settingsTitleText;
    public TMP_Text musicVolumeValueText;
    public TMP_Text sfxVolumeValueText;
    public TMP_Text uiScaleValueText;
    public TMP_Text difficultyValueText;
    public TMP_Text volumeValueText;
    public Slider musicVolumeSlider;
    public Slider sfxVolumeSlider;
    public Slider uiScaleSlider;
    public Slider volumeSlider;
    public Toggle baitLabelsToggle;
    public Toggle controlsGuideToggle;
    public Button difficultyButton;
    public Button restartButton;
    public Button closeSettingsButton;

    private const string BaitLabelsPrefKey = "FishBalatro.Settings.ShowBaitLabels";
    private const string ControlsGuidePrefKey = "FishBalatro.Settings.ShowControlsGuide";
    private float settingsMusicVolume = 1f;
    private float settingsSfxVolume = 1f;
    private float settingsUiScale = 1f;
    private bool showBaitLabels = true;
    private bool showControlsGuide = true;

    private void Awake()
    {
        if (!Application.isPlaying)
        {
            SetCanvasRendering(false);
            return;
        }

        SetCanvasRendering(true);
        EnsureEventSystemInputModule();
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
        if (game.IsVictory)
        {
            SetText(gameOverText, "CONGRATULATIONS!\nCommercial fishing ship defeated\nFinal Score: " + game.TotalScore + "\nPress R to play again");
        }
        else
        {
            SetText(gameOverText, "CAUGHT BY " + game.CaptureToolName.ToUpperInvariant() + "\nFinal Score: " + game.TotalScore + "\nPress R to restart");
        }

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
            gameOverPanel.SetActive(game.IsRunEnded);
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
        gearButtonSprite = gearButtonSprite != null ? gearButtonSprite : CreateGearSprite();
        settingsPanelSprite = settingsPanelSprite != null ? settingsPanelSprite : CreatePanelSprite(280, 220, new Color32(96, 173, 222, 255));

        RectTransform scoreRoot = EnsureContainer("Score UI", transform);
        RectTransform alertRoot = EnsureContainer("Alert UI", transform);
        RectTransform controlsRoot = EnsureContainer("Controls UI", transform);
        controlsRoot.gameObject.SetActive(false);

        scorePanelImage = EnsurePanelImage(scoreRoot, "Score Panel", scorePanelSprite, new Vector2(0f, 1f), new Vector2(24f, -18f), new Vector2(250f, 86f), TextAnchor.UpperLeft);
        multiplierPanelImage = EnsurePanelImage(scoreRoot, "Multiplier Panel", multiplierPanelSprite, new Vector2(0f, 1f), new Vector2(292f, -18f), new Vector2(250f, 86f), TextAnchor.UpperLeft);
        alertPanelImage = EnsurePanelImage(alertRoot, "Alert Meter Panel", alertPanelSprite, new Vector2(1f, 1f), new Vector2(-94f, -18f), new Vector2(500f, 72f), TextAnchor.UpperRight);
        controlsPanelImage = null;

        ConfigureText(totalScoreText, scoreRoot, new Vector2(0f, 1f), new Vector2(42f, -27f), new Vector2(214f, 68f), 28f, Color.white, TextAlignmentOptions.Center);
        ConfigureText(multiplierText, scoreRoot, new Vector2(0f, 1f), new Vector2(310f, -27f), new Vector2(214f, 68f), 28f, Color.white, TextAlignmentOptions.Center);
        ConfigureText(alertText, alertRoot, new Vector2(1f, 1f), new Vector2(-114f, -26f), new Vector2(430f, 30f), 24f, Color.white, TextAlignmentOptions.Center);
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
                bar.anchoredPosition = new Vector2(-114f, -54f);
                bar.sizeDelta = new Vector2(430f, 22f);
            }
        }

        EnsureWorldControlsHint();
        EnsureSettingsMenu();

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

    private void EnsureSettingsMenu()
    {
        LoadSettings();

        RectTransform settingsRoot = EnsureContainer("Settings UI", transform);
        settingsButton = EnsureIconButton(settingsRoot, "Settings Gear Button", gearButtonSprite, new Vector2(1f, 1f), new Vector2(-24f, -18f), new Vector2(58f, 58f));
        settingsButton.onClick.RemoveAllListeners();
        settingsButton.onClick.AddListener(ToggleSettingsPanel);

        bool keepOpen = settingsPanel != null && settingsPanel.activeSelf;
        settingsPanel = EnsureSettingsPanel(settingsRoot);
        RemoveSettingsChild(settingsPanel.transform, "Reduce Flashing Toggle");
        settingsPanel.SetActive(keepOpen);

        settingsTitleText = EnsureSettingsText(settingsPanel.transform, "Title", "SETTINGS", new Vector2(0f, 1f), new Vector2(24f, -20f), new Vector2(370f, 34f), 28f, TextAlignmentOptions.Left);
        EnsureSettingsText(settingsPanel.transform, "Difficulty Label", "Difficulty", new Vector2(0f, 1f), new Vector2(24f, -66f), new Vector2(180f, 30f), 23f, TextAlignmentOptions.Left);
        difficultyButton = EnsureTextButton(settingsPanel.transform, "Difficulty Button", "", new Vector2(1f, 1f), new Vector2(-24f, -62f), new Vector2(160f, 40f));

        EnsureSettingsText(settingsPanel.transform, "Music Volume Label", "Music volume", new Vector2(0f, 1f), new Vector2(24f, -118f), new Vector2(210f, 30f), 23f, TextAlignmentOptions.Left);
        musicVolumeValueText = EnsureSettingsText(settingsPanel.transform, "Music Volume Value", "", new Vector2(1f, 1f), new Vector2(-24f, -118f), new Vector2(110f, 30f), 23f, TextAlignmentOptions.Right);
        musicVolumeSlider = EnsureSlider(settingsPanel.transform, "Music Volume Slider", new Vector2(0f, 1f), new Vector2(24f, -158f), new Vector2(382f, 30f));

        EnsureSettingsText(settingsPanel.transform, "SFX Volume Label", "SFX volume", new Vector2(0f, 1f), new Vector2(24f, -204f), new Vector2(210f, 30f), 23f, TextAlignmentOptions.Left);
        sfxVolumeValueText = EnsureSettingsText(settingsPanel.transform, "SFX Volume Value", "", new Vector2(1f, 1f), new Vector2(-24f, -204f), new Vector2(110f, 30f), 23f, TextAlignmentOptions.Right);
        sfxVolumeSlider = EnsureSlider(settingsPanel.transform, "SFX Volume Slider", new Vector2(0f, 1f), new Vector2(24f, -244f), new Vector2(382f, 30f));

        EnsureSettingsText(settingsPanel.transform, "UI Scale Label", "UI scale", new Vector2(0f, 1f), new Vector2(24f, -290f), new Vector2(210f, 30f), 23f, TextAlignmentOptions.Left);
        uiScaleValueText = EnsureSettingsText(settingsPanel.transform, "UI Scale Value", "", new Vector2(1f, 1f), new Vector2(-24f, -290f), new Vector2(110f, 30f), 23f, TextAlignmentOptions.Right);
        uiScaleSlider = EnsureSlider(settingsPanel.transform, "UI Scale Slider", new Vector2(0f, 1f), new Vector2(24f, -330f), new Vector2(382f, 30f));
        uiScaleSlider.minValue = 0.85f;
        uiScaleSlider.maxValue = 1.25f;

        baitLabelsToggle = EnsureToggle(settingsPanel.transform, "Bait Labels Toggle", "Bait labels", new Vector2(0f, 1f), new Vector2(24f, -382f), new Vector2(380f, 34f));
        controlsGuideToggle = EnsureToggle(settingsPanel.transform, "Controls Guide Toggle", "Controls guide", new Vector2(0f, 1f), new Vector2(24f, -426f), new Vector2(380f, 34f));
        restartButton = EnsureTextButton(settingsPanel.transform, "Restart Button", "RESTART", new Vector2(0f, 1f), new Vector2(24f, -482f), new Vector2(170f, 44f));
        closeSettingsButton = EnsureTextButton(settingsPanel.transform, "Close Button", "CLOSE", new Vector2(1f, 1f), new Vector2(-24f, -482f), new Vector2(170f, 44f));

        volumeValueText = musicVolumeValueText;
        volumeSlider = musicVolumeSlider;
        musicVolumeSlider.SetValueWithoutNotify(settingsMusicVolume);
        sfxVolumeSlider.SetValueWithoutNotify(settingsSfxVolume);
        uiScaleSlider.SetValueWithoutNotify(settingsUiScale);
        baitLabelsToggle.SetIsOnWithoutNotify(showBaitLabels);
        controlsGuideToggle.SetIsOnWithoutNotify(showControlsGuide);
        UpdateSettingsText();

        difficultyButton.onClick.RemoveAllListeners();
        difficultyButton.onClick.AddListener(CycleDifficulty);
        musicVolumeSlider.onValueChanged.RemoveAllListeners();
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.RemoveAllListeners();
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        uiScaleSlider.onValueChanged.RemoveAllListeners();
        uiScaleSlider.onValueChanged.AddListener(SetUiScale);
        baitLabelsToggle.onValueChanged.RemoveAllListeners();
        baitLabelsToggle.onValueChanged.AddListener(SetBaitLabelsVisible);
        controlsGuideToggle.onValueChanged.RemoveAllListeners();
        controlsGuideToggle.onValueChanged.AddListener(SetControlsGuideVisible);
        restartButton.onClick.RemoveAllListeners();
        restartButton.onClick.AddListener(RestartFromSettings);
        closeSettingsButton.onClick.RemoveAllListeners();
        closeSettingsButton.onClick.AddListener(() => SetSettingsPanelVisible(false));

        ApplySettings();
    }

    private GameObject EnsureSettingsPanel(RectTransform parent)
    {
        Transform existing = parent.Find("Settings Panel");
        GameObject panel = existing != null ? existing.gameObject : new GameObject("Settings Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-24f, -86f);
        rect.sizeDelta = new Vector2(430f, 546f);

        Image image = panel.GetComponent<Image>();
        image.sprite = settingsPanelSprite;
        image.type = settingsPanelSprite != null && settingsPanelSprite.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
        image.color = Color.white;
        return panel;
    }

    private static void RemoveSettingsChild(Transform parent, string childName)
    {
        Transform child = parent.Find(childName);
        if (child == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(child.gameObject);
        }
        else
        {
            DestroyImmediate(child.gameObject);
        }
    }

    private Button EnsureIconButton(Transform parent, string name, Sprite sprite, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject buttonObject = EnsureUiObject(parent, name, typeof(Image), typeof(Button));
        ConfigureUiRect(buttonObject.transform, anchor, anchoredPosition, size);

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Simple;
        image.color = Color.white;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        return button;
    }

    private Button EnsureTextButton(Transform parent, string name, string label, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject buttonObject = EnsureUiObject(parent, name, typeof(Image), typeof(Button));
        ConfigureUiRect(buttonObject.transform, anchor, anchoredPosition, size);

        Image image = buttonObject.GetComponent<Image>();
        image.sprite = settingsPanelSprite;
        image.type = settingsPanelSprite != null && settingsPanelSprite.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
        image.color = new Color(0.12f, 0.2f, 0.28f, 0.95f);

        TMP_Text text = EnsureSettingsText(buttonObject.transform, "Label", label, new Vector2(0.5f, 0.5f), Vector2.zero, size - new Vector2(16f, 8f), 22f, TextAlignmentOptions.Center);
        text.color = Color.white;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        return button;
    }

    private Slider EnsureSlider(Transform parent, string name, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject sliderObject = EnsureUiObject(parent, name, typeof(Slider));
        ConfigureUiRect(sliderObject.transform, anchor, anchoredPosition, size);

        Image background = EnsureChildImage(sliderObject.transform, "Background", new Color(0.08f, 0.1f, 0.13f, 0.95f));
        ConfigureStretchRect(background.transform, Vector2.zero, Vector2.zero);

        Image fill = EnsureChildImage(sliderObject.transform, "Fill", new Color(0.42f, 0.9f, 1f, 0.95f));
        ConfigureStretchRect(fill.transform, new Vector2(4f, 7f), new Vector2(-4f, -7f));

        Image handle = EnsureChildImage(sliderObject.transform, "Handle", new Color(1f, 0.9f, 0.28f, 1f));
        RectTransform handleRect = handle.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0.5f, 0.5f);
        handleRect.anchorMax = new Vector2(0.5f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.anchoredPosition = Vector2.zero;
        handleRect.sizeDelta = new Vector2(22f, 34f);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.direction = Slider.Direction.LeftToRight;
        slider.fillRect = fill.rectTransform;
        slider.handleRect = handleRect;
        slider.targetGraphic = handle;
        return slider;
    }

    private Toggle EnsureToggle(Transform parent, string name, string label, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject toggleObject = EnsureUiObject(parent, name, typeof(Toggle));
        ConfigureUiRect(toggleObject.transform, anchor, anchoredPosition, size);

        Image box = EnsureChildImage(toggleObject.transform, "Box", new Color(0.08f, 0.1f, 0.13f, 0.95f));
        RectTransform boxRect = box.GetComponent<RectTransform>();
        boxRect.anchorMin = new Vector2(0f, 0.5f);
        boxRect.anchorMax = new Vector2(0f, 0.5f);
        boxRect.pivot = new Vector2(0f, 0.5f);
        boxRect.anchoredPosition = Vector2.zero;
        boxRect.sizeDelta = new Vector2(30f, 30f);

        Image check = EnsureChildImage(toggleObject.transform, "Check", new Color(0.42f, 0.9f, 1f, 1f));
        RectTransform checkRect = check.GetComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0f, 0.5f);
        checkRect.anchorMax = new Vector2(0f, 0.5f);
        checkRect.pivot = new Vector2(0f, 0.5f);
        checkRect.anchoredPosition = new Vector2(6f, 0f);
        checkRect.sizeDelta = new Vector2(18f, 18f);

        TMP_Text labelText = EnsureSettingsText(toggleObject.transform, "Label", label, new Vector2(0f, 0.5f), new Vector2(44f, 0f), new Vector2(size.x - 44f, 30f), 22f, TextAlignmentOptions.Left);
        labelText.color = Color.white;

        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.targetGraphic = box;
        toggle.graphic = check;
        return toggle;
    }

    private TMP_Text EnsureSettingsText(Transform parent, string name, string value, Vector2 anchor, Vector2 anchoredPosition, Vector2 size, float fontSize, TextAlignmentOptions alignment)
    {
        Transform existing = parent.Find(name);
        GameObject textObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        if (text == null)
        {
            text = textObject.AddComponent<TextMeshProUGUI>();
        }

        ConfigureUiRect(textObject.transform, anchor, anchoredPosition, size);
        text.font = TMP_Settings.defaultFontAsset;
        text.text = value;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.richText = true;
        text.raycastTarget = false;
        return text;
    }

    private static Image EnsureChildImage(Transform parent, string name, Color color)
    {
        Transform existing = parent.Find(name);
        GameObject imageObject = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);

        Image image = imageObject.GetComponent<Image>();
        image.sprite = null;
        image.color = color;
        image.type = Image.Type.Simple;
        return image;
    }

    private static GameObject EnsureUiObject(Transform parent, string name, params System.Type[] components)
    {
        Transform existing = parent.Find(name);
        GameObject obj = existing != null ? existing.gameObject : new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        for (int i = 0; i < components.Length; i++)
        {
            if (obj.GetComponent(components[i]) == null)
            {
                obj.AddComponent(components[i]);
            }
        }

        return obj;
    }

    private static void ConfigureUiRect(Transform transformToConfigure, Vector2 anchor, Vector2 anchoredPosition, Vector2 size)
    {
        RectTransform rect = transformToConfigure.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;
    }

    private static void ConfigureStretchRect(Transform transformToConfigure, Vector2 offsetMin, Vector2 offsetMax)
    {
        RectTransform rect = transformToConfigure.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    private void LoadSettings()
    {
        FishGameSettings.EnsureLoaded();
        settingsMusicVolume = FishGameSettings.MusicVolume;
        settingsSfxVolume = FishGameSettings.SfxVolume;
        settingsUiScale = FishGameSettings.UiScale;
        showBaitLabels = PlayerPrefs.GetInt(BaitLabelsPrefKey, 1) == 1;
        showControlsGuide = PlayerPrefs.GetInt(ControlsGuidePrefKey, 1) == 1;
    }

    private void ApplySettings()
    {
        BaitPickup.SetLabelsVisible(showBaitLabels);
        ApplyHudScale();

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(showControlsGuide);
        }
    }

    private void ApplyHudScale()
    {
        float scale = settingsUiScale;
        SetRectLocalScale(scorePanelImage, scale);
        SetRectLocalScale(multiplierPanelImage, scale);
        SetRectLocalScale(alertPanelImage, scale);
        SetRectLocalScale(totalScoreText, scale);
        SetRectLocalScale(multiplierText, scale);
        SetRectLocalScale(alertText, scale);
        SetRectLocalScale(comboText, scale);
        SetRectLocalScale(netSweepPanel, scale);

        if (alertFill != null && alertFill.transform.parent != null)
        {
            alertFill.transform.parent.localScale = Vector3.one * scale;
        }

        if (controlsPanel != null)
        {
            controlsPanel.transform.localScale = Vector3.one * scale;
        }
    }

    private static void SetRectLocalScale(Component component, float scale)
    {
        if (component != null)
        {
            component.transform.localScale = Vector3.one * scale;
        }
    }

    private static void SetRectLocalScale(GameObject obj, float scale)
    {
        if (obj != null)
        {
            obj.transform.localScale = Vector3.one * scale;
        }
    }

    private void ToggleSettingsPanel()
    {
        SetSettingsPanelVisible(settingsPanel == null || !settingsPanel.activeSelf);
    }

    private void SetSettingsPanelVisible(bool visible)
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(visible);
        }
    }

    private void SetMusicVolume(float value)
    {
        settingsMusicVolume = Mathf.Clamp01(value);
        FishGameSettings.SetMusicVolume(settingsMusicVolume);
        UpdateSettingsText();
    }

    private void SetSfxVolume(float value)
    {
        settingsSfxVolume = Mathf.Clamp01(value);
        FishGameSettings.SetSfxVolume(settingsSfxVolume);
        UpdateSettingsText();
    }

    private void SetUiScale(float value)
    {
        settingsUiScale = Mathf.Clamp(value, 0.85f, 1.25f);
        FishGameSettings.SetUiScale(settingsUiScale);
        ApplyHudScale();
        UpdateSettingsText();
    }

    private void CycleDifficulty()
    {
        FishDifficulty nextDifficulty = FishGameSettings.Difficulty == FishDifficulty.Hard
            ? FishDifficulty.Easy
            : (FishDifficulty)((int)FishGameSettings.Difficulty + 1);
        FishGameSettings.SetDifficulty(nextDifficulty);
        UpdateSettingsText();
    }

    private void SetBaitLabelsVisible(bool visible)
    {
        showBaitLabels = visible;
        PlayerPrefs.SetInt(BaitLabelsPrefKey, visible ? 1 : 0);
        PlayerPrefs.Save();
        BaitPickup.SetLabelsVisible(visible);
    }

    private void SetControlsGuideVisible(bool visible)
    {
        showControlsGuide = visible;
        PlayerPrefs.SetInt(ControlsGuidePrefKey, visible ? 1 : 0);
        PlayerPrefs.Save();

        if (controlsPanel != null)
        {
            controlsPanel.SetActive(visible);
        }
    }

    private void UpdateSettingsText()
    {
        SetText(musicVolumeValueText, Mathf.RoundToInt(settingsMusicVolume * 100f) + "%");
        SetText(sfxVolumeValueText, Mathf.RoundToInt(settingsSfxVolume * 100f) + "%");
        SetText(uiScaleValueText, Mathf.RoundToInt(settingsUiScale * 100f) + "%");
        SetText(difficultyValueText, FishGameSettings.GetDifficultyLabel());

        if (difficultyButton != null)
        {
            TMP_Text label = difficultyButton.GetComponentInChildren<TMP_Text>();
            SetText(label, FishGameSettings.GetDifficultyLabel());
        }
    }

    private void RestartFromSettings()
    {
        FishGameManager manager = FishGameManager.Instance;
        if (manager != null)
        {
            manager.RestartGame();
        }
    }

    private static void EnsureEventSystemInputModule()
    {
        EventSystem eventSystem = FindFirstObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            eventSystem = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        }

        if (eventSystem.GetComponent<BaseInputModule>() == null)
        {
            eventSystem.gameObject.AddComponent<StandaloneInputModule>();
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

    private static Sprite CreateGearSprite()
    {
        int size = 64;
        Color32[] pixels = new Color32[size * size];
        FillRect(pixels, size, size, 0, 0, size, size, new Color32(0, 0, 0, 0));

        Vector2 center = new Vector2(31.5f, 31.5f);
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center.x;
                float dy = y - center.y;
                float radius = Mathf.Sqrt(dx * dx + dy * dy);
                float angle = Mathf.Atan2(dy, dx);
                bool tooth = radius >= 20f && radius <= 29f && Mathf.Abs(Mathf.Cos(angle * 4f)) > 0.78f;
                bool outerRing = radius >= 13f && radius <= 21f;
                bool hub = radius <= 6f;
                bool darkCenter = radius <= 3f;

                if (tooth || outerRing || hub)
                {
                    pixels[y * size + x] = new Color32(218, 242, 250, 255);
                }

                if (darkCenter)
                {
                    pixels[y * size + x] = new Color32(13, 23, 34, 255);
                }
            }
        }

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
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
        text.fontSize = 42f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.Normal;
        text.raycastTarget = false;

        gameOverPanel = panel;
        gameOverText = text;
        gameOverPanel.SetActive(false);
    }
}
