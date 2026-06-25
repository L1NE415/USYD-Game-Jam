using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Scene")]
    [SerializeField] private QuitMenuController quitMenu;
    [SerializeField] private WinMenuController winMenu;
    [SerializeField] private HighScoreController scoreMenu;

    private VisualElement root;
    private Button resumeButton;
    private Button restartButton;
    private Button quitButton;

    private bool isPaused = false;

    private void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        resumeButton = root.Q<Button>("resume-button");
        restartButton = root.Q<Button>("restart-button");
        quitButton = root.Q<Button>("quit-button");

        if (resumeButton != null)
        {
            resumeButton.clicked += OnResumeButtonClicked;
        }

        if (restartButton != null)
        {
            restartButton.clicked += OnRestartButtonClicked;
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
            resumeButton.clicked -= OnResumeButtonClicked;

        if (restartButton != null)
            restartButton.clicked -= OnRestartButtonClicked;

        if (quitButton != null)
            quitButton.clicked -= OnQuitButtonClicked;

        Time.timeScale = 1f;
        UnlockCursor();
    }

    private void Update()
    {
        if ((quitMenu != null && quitMenu.IsShowing) || (winMenu != null && winMenu.IsShowing) || (scoreMenu != null && scoreMenu.IsShowing))
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

        //Debug.Log("Game Paused");
    }

    private void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        root.style.display = DisplayStyle.None;

        LockCursorIfNeeded();

        //Debug.Log("Resume Game");
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
    private void OnRestartButtonClicked()
    {
        RestartGame();
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