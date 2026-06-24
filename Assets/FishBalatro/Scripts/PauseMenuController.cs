using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    [SerializeField] private HighScoreController highScoreController;

    private VisualElement root;
    private Button resumeButton;
    private Button quitButton;

    private bool isPaused = false;

    private void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        resumeButton = root.Q<Button>("resume-button");
        quitButton = root.Q<Button>("quit-button");

        if (resumeButton != null)
        {
            resumeButton.clicked += OnResumeButtonClicked;
        }

        if (quitButton != null)
        {
            quitButton.clicked += OnQuitButtonClicked;
        }

        HidePauseMenu();

    }

    private void OnDisable()
    {
        if (resumeButton != null)
            resumeButton.clicked += OnResumeButtonClicked;

        if (quitButton != null)
            quitButton.clicked += OnQuitButtonClicked;

        Time.timeScale = 1f;
        UnlockCursor();
    }

    private void Update()
    {
        if (highScoreController != null && highScoreController.IsShowing)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    private void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;

        root.style.display = DisplayStyle.Flex;

        UnlockCursor();

        Debug.Log("Game Paused");
    }

    private void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        root.style.display = DisplayStyle.None;

        LockCursorIfNeeded();

        Debug.Log("Resume Game");
    }

    private void HidePauseMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;

        root.style.display = DisplayStyle.None;
    }

    private void OnResumeButtonClicked()
    {
        ResumeGame();
    }

    private void OnQuitButtonClicked()
    {
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    private void UnlockCursor()
    {
        UnityEngine.Cursor.visible = true;
        UnityEngine.Cursor.lockState = CursorLockMode.None;
    }

    private void LockCursorIfNeeded()
    {
        //UnityEngine.Cursor.visible = false;
        //UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }
}