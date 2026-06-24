using UnityEngine;
using UnityEngine.UIElements;

public class UIToolkitMouseDebugger : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;

    private void Update()
    {
        if (uiDocument == null)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Input.mousePosition;

            Vector2 panelPosition = new Vector2(
                mousePosition.x,
                Screen.height - mousePosition.y
            );

            VisualElement pickedElement = uiDocument.rootVisualElement.panel.Pick(panelPosition);

            if (pickedElement != null)
            {
                Debug.Log(
                    "UI Toolkit Clicked: " +
                    pickedElement.name +
                    " | Type: " +
                    pickedElement.GetType().Name
                );
            }
            else
            {
                Debug.Log("UI Toolkit No element was clicked");
            }
        }
    }
}