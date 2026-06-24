using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    [SerializeField] private string gameSceneName = "GameScene";

    private Button startButton;
    private Button quitButton;

    private void OnEnable()
    {
        var root = uiDocument.rootVisualElement;

        startButton = root.Q<Button>("start-button");
        quitButton = root.Q<Button>("quit-button");

        startButton.clicked += OnStartClicked;
        quitButton.clicked += OnQuitClicked;
    }

    private void OnDisable()
    {
        if (startButton != null)
            startButton.clicked -= OnStartClicked;

        if (quitButton != null)
            quitButton.clicked -= OnQuitClicked;
    }

    private void OnStartClicked()
    {
        SceneManager.LoadScene("Main");
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}