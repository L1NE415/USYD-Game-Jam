using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

public class QuitMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    [Header("Return Target")]
    [SerializeField] private HighScoreController scoreMenuUI;

    [SerializeField] private string gameSceneName = "GameScene";

    [SerializeField] private FishGameManager game;

    [SerializeField] private FishPlayerController fishPlayerController;

    private UnityEngine.UIElements.Button restartButton;
    private UnityEngine.UIElements.Button scoreButton;
    private UnityEngine.UIElements.Button quitButton;
    private Label subtitleLabel;
    private VisualElement root;

    private void Awake()
    {
        if (game == null)
        {
            game = FindObjectOfType<FishGameManager>();
        }

        if (fishPlayerController == null && game != null)
        {
            fishPlayerController = game.player;
        }

        if (fishPlayerController == null)
        {
            fishPlayerController = FindObjectOfType<FishPlayerController>();
        }
    }

    private void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        restartButton = root.Q<UnityEngine.UIElements.Button>("restart-button");
        scoreButton = root.Q<UnityEngine.UIElements.Button>("score-button");
        quitButton = root.Q<UnityEngine.UIElements.Button>("quit-button");
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
        if(fishPlayerController.gameManager.State == FishGameState.Caught)
        {
            subtitleLabel.text = "Your Score: " + game.TotalScore;
            root.style.display = DisplayStyle.Flex;
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