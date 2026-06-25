using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Collider2D))]
// The big fish is the player's paid level-transition action. It shows whether
// the player can afford an attack and plays the short attack movement.
public class BigFishAlly : MonoBehaviour
{
#if UNITY_EDITOR
    private const string SharkAttackSpriteSheetPath = "Assets/Player/Shark_Attack.png";
    private const string SharkAttackSpritePrefix = "Shark_Movement_";
#endif

    public FishGameManager gameManager;
    public TMP_Text promptText;
    public SpriteRenderer bodyRenderer;
    public Animator attackAnimator;
    public Sprite[] attackFrames;
    public float attackFrameRate = 8f;
    public float attackScaleMultiplier = 5.2f;
    public GameObject idleHeadVisual;
    public float promptFontSize = 5f;
    public Vector2 promptBoxSize = new Vector2(8.5f, 3.2f);
    public Vector3 promptLocalPosition = new Vector3(0f, 1.25f, 0f);
    public float attackDuration = 0.55f;
    public float returnDuration = 0.45f;

    private Vector3 homePosition;
    private Vector3 homeScale;
    private Sprite idleSprite;
    private bool attackVisualsActive;
    private float attackFrameElapsed;
    private Transform promptParentBeforeAttack;
    private Vector3 promptLocalPositionBeforeAttack;
    private Quaternion promptLocalRotationBeforeAttack;
    private Vector3 promptLocalScaleBeforeAttack;
    private bool promptDetachedForAttack;

    private void Awake()
    {
        homePosition = transform.position;
        homeScale = transform.localScale;
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

        if (attackVisualsActive)
        {
            AdvanceAttackFrame(Time.deltaTime);
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

        if (attackAnimator == null)
        {
            attackAnimator = GetComponent<Animator>();
        }

        if (bodyRenderer != null && idleSprite == null)
        {
            idleSprite = bodyRenderer.sprite;
        }

#if UNITY_EDITOR
        if (!HasAttackFrames)
        {
            attackFrames = LoadAttackFramesInEditor();
        }
#endif

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

        if (active)
        {
            DetachPromptForAttack();
            transform.localScale = new Vector3(homeScale.x * attackScaleMultiplier, homeScale.y * attackScaleMultiplier, homeScale.z);
        }
        else
        {
            transform.localScale = homeScale;
            ReattachPromptAfterAttack();
        }

        if (bodyRenderer != null)
        {
            // The cave mouth is transparent, so the full body stays rendered
            // behind the cave during idle and then moves out for the attack.
            bodyRenderer.enabled = true;
            if (active && HasAttackFrames)
            {
                attackFrameElapsed = 0f;
                ApplyAttackFrame(0);
            }
            else if (!active && idleSprite != null)
            {
                bodyRenderer.sprite = idleSprite;
            }
        }

        if (attackAnimator != null)
        {
            bool useAnimator = active && !HasAttackFrames;
            attackAnimator.enabled = useAnimator;
            if (useAnimator)
            {
                attackAnimator.Play("Shark_Attack", 0, 0f);
            }
        }

        if (idleHeadVisual != null)
        {
            idleHeadVisual.SetActive(false);
        }
    }

    private bool HasAttackFrames => attackFrames != null && attackFrames.Length > 0;

    private void AdvanceAttackFrame(float deltaTime)
    {
        if (!HasAttackFrames || bodyRenderer == null)
        {
            return;
        }

        attackFrameElapsed += Mathf.Max(0f, deltaTime);
        int frameIndex = Mathf.FloorToInt(attackFrameElapsed * Mathf.Max(1f, attackFrameRate)) % attackFrames.Length;
        ApplyAttackFrame(frameIndex);
    }

    private void ApplyAttackFrame(int frameIndex)
    {
        if (!HasAttackFrames || bodyRenderer == null)
        {
            return;
        }

        Sprite frame = attackFrames[Mathf.Clamp(frameIndex, 0, attackFrames.Length - 1)];
        if (frame != null)
        {
            bodyRenderer.sprite = frame;
        }
    }

    private void DetachPromptForAttack()
    {
        if (promptText == null || promptDetachedForAttack)
        {
            return;
        }

        Transform promptTransform = promptText.transform;
        promptParentBeforeAttack = promptTransform.parent;
        promptLocalPositionBeforeAttack = promptTransform.localPosition;
        promptLocalRotationBeforeAttack = promptTransform.localRotation;
        promptLocalScaleBeforeAttack = promptTransform.localScale;
        promptTransform.SetParent(null, true);
        promptDetachedForAttack = true;
    }

    private void ReattachPromptAfterAttack()
    {
        if (promptText == null || !promptDetachedForAttack)
        {
            return;
        }

        Transform promptTransform = promptText.transform;
        promptTransform.SetParent(promptParentBeforeAttack != null ? promptParentBeforeAttack : transform, false);
        promptTransform.localPosition = promptLocalPositionBeforeAttack;
        promptTransform.localRotation = promptLocalRotationBeforeAttack;
        promptTransform.localScale = promptLocalScaleBeforeAttack;
        promptDetachedForAttack = false;
        ConfigurePromptText();
    }

#if UNITY_EDITOR
    private static Sprite[] LoadAttackFramesInEditor()
    {
        AssetDatabase.ImportAsset(SharkAttackSpriteSheetPath, ImportAssetOptions.ForceUpdate);
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(SharkAttackSpriteSheetPath);
        List<Sprite> frames = new List<Sprite>();
        for (int i = 0; i < assets.Length; i++)
        {
            if (assets[i] is Sprite sprite && sprite.name.StartsWith(SharkAttackSpritePrefix, System.StringComparison.Ordinal))
            {
                frames.Add(sprite);
            }
        }

        frames.Sort((left, right) => ExtractAttackFrameIndex(left.name).CompareTo(ExtractAttackFrameIndex(right.name)));
        return frames.ToArray();
    }

    private static int ExtractAttackFrameIndex(string spriteName)
    {
        string suffix = spriteName.Substring(SharkAttackSpritePrefix.Length);
        return int.TryParse(suffix, out int index) ? index : int.MaxValue;
    }
#endif
}
