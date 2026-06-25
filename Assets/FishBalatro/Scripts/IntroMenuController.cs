using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class IntroSequenceController : MonoBehaviour
{
    [Header("Intro Pages")]
    [SerializeField] private UIDocument introPage1;
    [SerializeField] private UIDocument introPage2;
    [SerializeField] private UIDocument introPage3;

    [Header("After Intro")]
    [SerializeField] private MainMenuController mainMenu;

    private VisualElement page1Root;
    private VisualElement page2Root;
    private VisualElement page3Root;

    private int currentPage = 0;

    private void Start()
    {
        page1Root = introPage1.rootVisualElement;
        page2Root = introPage2.rootVisualElement;
        page3Root = introPage3.rootVisualElement;

        currentPage = 0;
        ShowCurrentPage();

        if (mainMenu != null)
        {
            mainMenu.HideMenu();
        }
    }

    private void Update()
    {
        if (ReadNextPressed())
        {
            currentPage++;

            if (currentPage >= 3)
            {
                FinishIntro();
            }
            else
            {
                ShowCurrentPage();
            }
        }
    }

    private void ShowCurrentPage()
    {
        page1Root.style.display = currentPage == 0 ? DisplayStyle.Flex : DisplayStyle.None;
        page2Root.style.display = currentPage == 1 ? DisplayStyle.Flex : DisplayStyle.None;
        page3Root.style.display = currentPage == 2 ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void FinishIntro()
    {
        page1Root.style.display = DisplayStyle.None;
        page2Root.style.display = DisplayStyle.None;
        page3Root.style.display = DisplayStyle.None;

        if (mainMenu != null)
        {
            mainMenu.ShowMenu();
        }

        enabled = false;
    }

    private static bool ReadNextPressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && keyboard.spaceKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Space);
#endif
    }
}