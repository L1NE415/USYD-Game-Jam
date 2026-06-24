using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
// The big fish is the player's paid level-transition action. It shows whether
// the player can afford an attack and plays the short attack movement.
public class BigFishAlly : MonoBehaviour
{
    public FishGameManager gameManager;
    public TMP_Text promptText;
    public SpriteRenderer bodyRenderer;
    public GameObject idleHeadVisual;
    public float promptFontSize = 5f;
    public Vector2 promptBoxSize = new Vector2(8.5f, 3.2f);
    public Vector3 promptLocalPosition = new Vector3(0f, 1.25f, 0f);
    public float attackDuration = 0.55f;
    public float returnDuration = 0.45f;

    private Vector3 homePosition;
    private bool attackVisualsActive;

    private void Awake()
    {
        homePosition = transform.position;
        ResolveVisualReferences();
        SetAttackVisuals(false);
        ConfigurePromptText();
        SetPrompt(false);
    }

    private void OnValidate()
    {
        ConfigurePromptText();
    }

    private void Update()
    {
        if (gameManager == null)
        {
            gameManager = FishGameManager.Instance;
        }

        bool canAttack = gameManager != null && gameManager.CanCallBigFish;
        SetPrompt(gameManager != null);

        if (promptText != null && gameManager != null)
        {
            ConfigurePromptText();
            promptText.text = canAttack
                ? "E: ATTACK\nCost " + gameManager.AttackCost
                : "Need " + gameManager.AttackCost + " score\nPress E when ready";
            promptText.color = canAttack ? new Color(0.8f, 1f, 0.9f) : new Color(1f, 0.72f, 0.72f);
        }
    }

    public IEnumerator PlayAttack(Vector3 targetPosition)
    {
        // Rush toward the fisherman, pause for impact, then return home.
        SetAttackVisuals(true);
        Vector3 start = homePosition;
        Vector3 end = new Vector3(targetPosition.x - 0.8f, targetPosition.y - 0.3f, transform.position.z);
        yield return MoveTo(start, end, attackDuration);
        yield return new WaitForSeconds(0.12f);
        yield return MoveTo(transform.position, homePosition, returnDuration);
        SetAttackVisuals(false);
    }

    private IEnumerator MoveTo(Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            transform.position = Vector3.LerpUnclamped(start, end, t);
            yield return null;
        }

        transform.position = end;
    }

    private void SetPrompt(bool visible)
    {
        if (promptText != null)
        {
            promptText.gameObject.SetActive(visible);
        }
    }

    private void ConfigurePromptText()
    {
        if (promptText == null)
        {
            return;
        }

        promptText.fontSize = promptFontSize;
        promptText.enableAutoSizing = false;
        promptText.alignment = TextAlignmentOptions.Center;
        promptText.textWrappingMode = TextWrappingModes.NoWrap;
        promptText.richText = true;
        promptText.rectTransform.sizeDelta = promptBoxSize;

        if (promptText.transform.parent == transform)
        {
            promptText.transform.localPosition = promptLocalPosition;
        }
    }

    private void ResolveVisualReferences()
    {
        if (bodyRenderer == null)
        {
            bodyRenderer = GetComponent<SpriteRenderer>();
        }

        if (idleHeadVisual == null)
        {
            Transform childHead = transform.Find("Big Fish Head Peek");
            if (childHead != null)
            {
                idleHeadVisual = childHead.gameObject;
            }
        }

        if (idleHeadVisual == null)
        {
            GameObject sceneHead = GameObject.Find("Big Fish Head Peek");
            if (sceneHead != null)
            {
                idleHeadVisual = sceneHead;
            }
        }
    }

    private void SetAttackVisuals(bool active)
    {
        attackVisualsActive = active;
        ResolveVisualReferences();

        if (bodyRenderer != null)
        {
            // The cave mouth is transparent, so the full body stays rendered
            // behind the cave during idle and then moves out for the attack.
            bodyRenderer.enabled = true;
        }

        if (idleHeadVisual != null)
        {
            idleHeadVisual.SetActive(false);
        }
    }
}
