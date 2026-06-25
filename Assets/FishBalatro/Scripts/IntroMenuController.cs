using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class IntroMenuController : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    [SerializeField] private IntroMenuController nextUI;
    [SerializeField] private MainMenuController mainMenu;

    [SerializeField] private string gameSceneName = "GameScene";

    private VisualElement root;

    private Button nextButton;

    public int introNum;

    private void OnEnable()
    {
        root = uiDocument.rootVisualElement;

        nextButton = root.Q<Button>("next-button");

        nextButton.clicked += OnNextClicked;

        if (introNum == 1)
        {
            root.style.display = DisplayStyle.Flex;
        }
        else
        {
            root.style.display = DisplayStyle.None;
        }
    }

    private void OnDisable()
    {
        if (nextButton != null)
            nextButton.clicked -= OnNextClicked;
    }

    private void OnNextClicked()
    {
        if (introNum != 3)
        {
            root.style.display = DisplayStyle.None;
            nextUI.root.style.display = DisplayStyle.Flex;
        }
        else
        {
            root.style.display = DisplayStyle.None;
            mainMenu.root.style.display = DisplayStyle.Flex;
        }

    }
}