using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class WinMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    [Header("Return Target")]
    [SerializeField] private HighScoreController scoreMenuUI;

    [SerializeField] private FishGameManager game;

    [SerializeField] private FishPlayerController fishPlayerController;

    private UnityEngine.UIElements.Button restartButton;
    private UnityEngine.UIElements.Button scoreButton;
    private UnityEngine.UIElements.Button quitButton;
    private Label titleLabel;
    private Label subtitleLabel;
    private VisualElement root;

    private bool isShowing;
    public bool IsShowing => isShowing;

    private void Awake()
    {

        isShowing = false;
    }

    private void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        restartButton = root.Q<UnityEngine.UIElements.Button>("restart-button");
        scoreButton = root.Q<UnityEngine.UIElements.Button>("score-button");
        quitButton = root.Q<UnityEngine.UIElements.Button>("quit-button");
        titleLabel = root.Q<Label>("title-label");
        subtitleLabel = root.Q<Label>("subtitle-label");

        restartButton.clicked += OnRestartClicked;
        scoreButton.clicked += OnScoreClicked;
        quitButton.clicked += OnQuitClicked;

        root.style.display = DisplayStyle.None;
    }

    private void OnDisable()
    {
        if (restartButton != null)
            restartButton.clicked -= OnRestartClicked;

        if (scoreButton != null)
            scoreButton.clicked -= OnScoreClicked;

        if (quitButton != null)
            quitButton.clicked -= OnQuitClicked;
    }

    private void Update()
    {
        if (fishPlayerController.gameManager.State == FishGameState.Victory)
        {
            subtitleLabel.text = "Your Score: " + game.FinalScore;
            root.style.display = DisplayStyle.Flex;
            isShowing = true;
        }
    }

    private void OnRestartClicked()
    {
        game.RestartGame();
    }

    private void OnScoreClicked()
    {
        root.style.display = DisplayStyle.None;
        scoreMenuUI.Show();
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void Show()
    {
        root.style.display = DisplayStyle.Flex;
    }
}