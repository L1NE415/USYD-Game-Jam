using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

public class HighScoreController : MonoBehaviour
{
    [Header("UI Document")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Return Target")]

    [SerializeField] public WinMenuController winMenuUI;

    private VisualElement root;

    private Label score1Label;
    private Label score2Label;
    private Label score3Label;
    private Label score4Label;
    private Label score5Label;

    private UnityEngine.UIElements.Button backButton;
    private UnityEngine.UIElements.Button clearButton;

    private bool isShowing;
    public bool IsShowing => isShowing;

    private const int MaxScores = 5;
    private const string ScoreKeyPrefix = "HighScore_";

    private void OnEnable()
    {
        if (uiDocument == null)
        {
            uiDocument = GetComponent<UIDocument>();
        }

        root = uiDocument.rootVisualElement;

        score1Label = root.Q<Label>("score-1");
        score2Label = root.Q<Label>("score-2");
        score3Label = root.Q<Label>("score-3");
        score4Label = root.Q<Label>("score-4");
        score5Label = root.Q<Label>("score-5");

        backButton = root.Q<UnityEngine.UIElements.Button>("back-button");
        clearButton = root.Q<UnityEngine.UIElements.Button>("clear-button");

        if (backButton != null)
        {
            backButton.clicked += ReturnToQuitMenu;
        }
        if (clearButton != null)
        {
            clearButton.clicked += OnClearButtonClick;
        }

        Hide();
    }

    private void OnDisable()
    {
        if (backButton != null)
        {
            backButton.clicked -= ReturnToQuitMenu;
        }
        if (clearButton != null)
        {
            clearButton.clicked -= OnClearButtonClick;
        }
    }

    private void Update()
    {
        if (!isShowing)
        {
            return;
        }

        if (ReadBackPressed())
        {
            ReturnToQuitMenu();
        }
    }

    private void OnClearButtonClick()
    {
        ClearScores();
        RefreshScoreLabels();
    }

    public void Show()
    {
        isShowing = true;

        if (root != null)
        {
            root.style.display = DisplayStyle.Flex;
        }

        RefreshScoreLabels();
    }

    public void Hide()
    {
        isShowing = false;

        if (root != null)
        {
            root.style.display = DisplayStyle.None;
        }
    }

    private void ReturnToQuitMenu()
    {
        Hide();
        winMenuUI.Show();

    }

    public static void AddScore(int newScore)
    {
        List<int> scores = LoadScores();

        scores.Add(newScore);
        scores.Sort((a, b) => b.CompareTo(a));

        while (scores.Count > MaxScores)
        {
            scores.RemoveAt(scores.Count - 1);
        }

        for (int i = 0; i < MaxScores; i++)
        {
            int value = i < scores.Count ? scores[i] : 0;
            PlayerPrefs.SetInt(ScoreKeyPrefix + i, value);
        }

        PlayerPrefs.Save();
    }

    private static List<int> LoadScores()
    {
        List<int> scores = new List<int>();

        for (int i = 0; i < MaxScores; i++)
        {
            scores.Add(PlayerPrefs.GetInt(ScoreKeyPrefix + i, 0));
        }

        scores.Sort((a, b) => b.CompareTo(a));
        return scores;
    }

    private void RefreshScoreLabels()
    {
        List<int> scores = LoadScores();

        SetScoreLabel(score1Label, 1, scores[0]);
        SetScoreLabel(score2Label, 2, scores[1]);
        SetScoreLabel(score3Label, 3, scores[2]);
        SetScoreLabel(score4Label, 4, scores[3]);
        SetScoreLabel(score5Label, 5, scores[4]);
    }

    private void SetScoreLabel(Label label, int rank, int score)
    {
        if (label == null)
        {
            return;
        }

        label.text = rank + ". " + score;
    }

    public static void ClearScores()
    {
        for (int i = 0; i < MaxScores; i++)
        {
            PlayerPrefs.DeleteKey(ScoreKeyPrefix + i);
        }

        PlayerPrefs.Save();
    }

    private static bool ReadBackPressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Keyboard keyboard = Keyboard.current;

        return keyboard != null &&
               (keyboard.escapeKey.wasPressedThisFrame ||
                keyboard.backspaceKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }
}