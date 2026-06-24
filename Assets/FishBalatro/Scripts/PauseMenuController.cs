using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private UIDocument uiDocument;

    [Header("Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

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
            Debug.Log("继续游戏按钮绑定成功");
        }
        else
        {
            Debug.LogError("找不到 resume-button，请检查 UI Builder 里的按钮 Name");
        }

        if (quitButton != null)
        {
            quitButton.clicked += OnQuitButtonClicked;
            Debug.Log("退出游戏按钮绑定成功");
        }
        else
        {
            Debug.LogError("找不到 quit-button，请检查 UI Builder 里的按钮 Name");
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

        Debug.Log("游戏暂停，菜单显示");
    }

    private void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;

        root.style.display = DisplayStyle.None;

        LockCursorIfNeeded();

        Debug.Log("继续游戏，菜单隐藏");
    }

    private void HidePauseMenu()
    {
        isPaused = false;
        Time.timeScale = 1f;

        root.style.display = DisplayStyle.None;
    }

    private void OnResumeButtonClicked()
    {
        Debug.Log("点击了继续游戏按钮");
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
        // 如果你的游戏不需要锁鼠标，可以把这里两行注释掉
        //UnityEngine.Cursor.visible = false;
        //UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }
}