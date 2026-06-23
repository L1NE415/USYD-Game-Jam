using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshPro))]
// Small world-space popup used for score, bait effects, and hook feedback.
// It creates itself at runtime so designers do not need a prefab for every popup.
public class FloatingText : MonoBehaviour
{
    public float lifetime = 0.8f;
    public Vector3 drift = new Vector3(0f, 0.85f, 0f);

    private TMP_Text text;
    private Color startColor;
    private float age;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
        startColor = text.color;
    }

    public void Init(string message, Color color)
    {
        if (text == null)
        {
            text = GetComponent<TMP_Text>();
        }

        text.text = message;
        text.color = color;
        startColor = color;
    }

    private void Update()
    {
        // Drift upward while fading out, then clean up the temporary object.
        age += Time.deltaTime;
        transform.position += drift * Time.deltaTime;

        float t = Mathf.Clamp01(age / lifetime);
        Color faded = startColor;
        faded.a = 1f - t;
        text.color = faded;

        if (age >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    public static FloatingText Spawn(Vector3 position, string message, Color color, TMP_FontAsset font)
    {
        GameObject popup = new GameObject("FloatingText");
        popup.transform.position = position;

        TextMeshPro text = popup.AddComponent<TextMeshPro>();
        text.font = font;
        text.fontSize = 3.4f;
        text.alignment = TextAlignmentOptions.Center;
        text.text = message;
        text.color = color;
        text.textWrappingMode = TextWrappingModes.NoWrap;

        MeshRenderer renderer = popup.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 80;

        FloatingText floatingText = popup.AddComponent<FloatingText>();
        floatingText.Init(message, color);
        return floatingText;
    }
}
