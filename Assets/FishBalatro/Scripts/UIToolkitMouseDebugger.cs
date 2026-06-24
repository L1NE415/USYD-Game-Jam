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

            // Unity 鼠标坐标原点在左下，UI Toolkit panel 坐标原点在左上
            Vector2 panelPosition = new Vector2(
                mousePosition.x,
                Screen.height - mousePosition.y
            );

            VisualElement pickedElement = uiDocument.rootVisualElement.panel.Pick(panelPosition);

            if (pickedElement != null)
            {
                Debug.Log(
                    "UI Toolkit 点到了: " +
                    pickedElement.name +
                    " | 类型: " +
                    pickedElement.GetType().Name
                );
            }
            else
            {
                Debug.Log("UI Toolkit 没有点到任何元素");
            }
        }
    }
}