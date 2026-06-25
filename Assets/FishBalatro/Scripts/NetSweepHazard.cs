using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Cast-net hazard used when Alert reaches 100.
// The hazard drops from the fisherman's tool anchor, expands while falling,
// and catches fish inside the growing landing area.
public class NetSweepHazard : MonoBehaviour
{
    public Vector3 pivotPosition = new Vector3(0f, 3.7f, 0f);
    public Vector2 netSize = new Vector2(3.2f, 3.2f);
    public Vector2 netLocalOffset = new Vector2(0.1f, -2.3f);
    public Vector2 castStartLocalOffset = new Vector2(0.05f, -0.15f);
    public Vector2 minCatchSize = new Vector2(0.8f, 0.8f);
    public Vector2 visualStartScale = new Vector2(0.35f, 0.35f);
    public Vector2 visualEndScale = Vector2.one;
    public float levelThreeSizeMultiplier = 1f;
    public float warningSeconds = 0.45f;
    public float sweepSeconds = 0.95f;
    public float lingerSeconds = 0.45f;
    public float recoverSeconds = 0.15f;
    public int sortingOrder = 45;
    public Sprite netSprite;
    public SpriteRenderer netRenderer;
    public bool useLineFallback = true;

    private readonly List<LineRenderer> netLines = new List<LineRenderer>();
    private Animator netAnimator;
    private Transform netTransform;
    private BoxCollider2D hitbox;
    private bool isPlaying;
    private Vector3 baseVisualScale = Vector3.one;
    private float activeSizeMultiplier = 1f;

    public bool CaughtPlayer { get; private set; }
    public float Progress { get; private set; }

    public static NetSweepHazard CreateRuntimeNet()
    {
        GameObject root = new GameObject("Net Sweep Pivot");
        return root.AddComponent<NetSweepHazard>();
    }

    private void Awake()
    {
        BuildVisualIfNeeded();
        if (!isPlaying)
        {
            Hide();
        }
    }

    public IEnumerator PlaySweep(FishPlayerController player, int level, Vector3? worldCastOrigin = null)
    {
        isPlaying = true;
        BuildVisualIfNeeded();

        gameObject.SetActive(true);
        transform.position = worldCastOrigin ?? pivotPosition;
        transform.rotation = Quaternion.identity;
        CaughtPlayer = false;
        Progress = 0f;

        Collider2D playerCollider = player != null ? player.GetComponent<Collider2D>() : null;
        float durationMultiplier = FishGameSettings.ToolDurationMultiplier;
        float tunedWarningSeconds = warningSeconds * durationMultiplier;
        float tunedSweepSeconds = sweepSeconds * durationMultiplier;
        float tunedLingerSeconds = lingerSeconds * durationMultiplier;
        activeSizeMultiplier = level == 3 ? Mathf.Max(1f, levelThreeSizeMultiplier) : 1f;

        hitbox.enabled = false;
        hitbox.size = minCatchSize * activeSizeMultiplier;
        SetNetColor(new Color(1f, 0.9f, 0.35f, 0.55f));
        SetVisualPose(0f);
        RestartAnimator(tunedSweepSeconds);

        float elapsed = 0f;
        while (elapsed < tunedWarningSeconds)
        {
            elapsed += Time.deltaTime;
            Progress = Mathf.Clamp01(elapsed / tunedWarningSeconds) * 0.15f;
            yield return null;
        }

        hitbox.enabled = true;
        SetNetColor(new Color(0.62f, 0.95f, 1f, 0.9f));

        elapsed = 0f;
        while (elapsed < tunedSweepSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / tunedSweepSeconds);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            SetVisualPose(eased);
            Progress = Mathf.Lerp(0.15f, 0.9f, t);

            if (TryCatchPlayer(playerCollider))
            {
                yield break;
            }

            yield return null;
        }

        FreezeAnimatorAtOpenState();
        elapsed = 0f;
        SetVisualPose(1f);
        while (elapsed < tunedLingerSeconds)
        {
            elapsed += Time.deltaTime;
            Progress = Mathf.Lerp(0.9f, 1f, Mathf.Clamp01(elapsed / tunedLingerSeconds));

            if (TryCatchPlayer(playerCollider))
            {
                yield break;
            }

            yield return null;
        }

        hitbox.enabled = false;
        yield return new WaitForSeconds(recoverSeconds);
        Hide();
    }

    public void Hide()
    {
        isPlaying = false;
        Progress = 0f;
        if (hitbox != null)
        {
            hitbox.enabled = false;
        }

        if (netAnimator != null)
        {
            netAnimator.speed = 1f;
        }

        gameObject.SetActive(false);
    }

    private bool TryCatchPlayer(Collider2D playerCollider)
    {
        if (playerCollider == null || hitbox == null)
        {
            return false;
        }

        Physics2D.SyncTransforms();
        if (!hitbox.Distance(playerCollider).isOverlapped)
        {
            return false;
        }

        CaughtPlayer = true;
        Progress = 1f;
        hitbox.enabled = false;
        SetNetColor(new Color(1f, 0.15f, 0.1f, 0.95f));
        return true;
    }

    private void BuildVisualIfNeeded()
    {
        if (netTransform != null)
        {
            return;
        }

        transform.position = pivotPosition;

        if (netRenderer != null)
        {
            netTransform = netRenderer.transform;
        }

        if (netTransform == null)
        {
            Transform child = transform.Find("Fishing Net");
            if (child == null)
            {
                child = transform.Find("Net Sprite");
            }
            if (child == null)
            {
                child = transform.Find("Sweeping Net");
            }

            if (child != null)
            {
                netTransform = child;
                netRenderer = child.GetComponent<SpriteRenderer>();
            }
        }

        if (netTransform == null)
        {
            GameObject netObject = new GameObject("Fishing Net");
            netObject.transform.SetParent(transform, false);
            netObject.transform.localPosition = netLocalOffset;
            netTransform = netObject.transform;
        }

        if (netRenderer == null && netSprite != null)
        {
            netRenderer = netTransform.gameObject.AddComponent<SpriteRenderer>();
        }

        if (netRenderer != null)
        {
            if (netRenderer.sprite == null)
            {
                netRenderer.sprite = netSprite;
            }

            netRenderer.sortingOrder = sortingOrder;
        }

        netAnimator = netTransform.GetComponent<Animator>();
        baseVisualScale = netTransform.localScale;

        hitbox = netTransform.GetComponent<BoxCollider2D>();
        if (hitbox == null)
        {
            hitbox = netTransform.gameObject.AddComponent<BoxCollider2D>();
        }

        hitbox.isTrigger = true;
        hitbox.size = netSize;
        hitbox.enabled = false;

        if ((netRenderer == null || netRenderer.sprite == null) && useLineFallback)
        {
            BuildNetLines();
        }
    }

    private void RestartAnimator(float tunedSweepSeconds)
    {
        if (netAnimator == null)
        {
            return;
        }

        float clipLength = 0.8333333f;
        RuntimeAnimatorController controller = netAnimator.runtimeAnimatorController;
        if (controller != null && controller.animationClips != null && controller.animationClips.Length > 0 && controller.animationClips[0] != null)
        {
            clipLength = Mathf.Max(0.01f, controller.animationClips[0].length);
        }

        netAnimator.speed = clipLength / Mathf.Max(0.05f, tunedSweepSeconds);
        netAnimator.Play(0, 0, 0f);
        netAnimator.Update(0f);
    }

    private void FreezeAnimatorAtOpenState()
    {
        if (netAnimator == null)
        {
            return;
        }

        netAnimator.Play(0, 0, 0.999f);
        netAnimator.Update(0f);
        netAnimator.speed = 0f;
    }

    private void SetVisualPose(float normalized)
    {
        if (netTransform == null)
        {
            return;
        }

        Vector2 position = Vector2.Lerp(castStartLocalOffset, netLocalOffset, normalized);
        netTransform.localPosition = new Vector3(position.x, position.y, 0f);

        Vector2 scaleFactor = Vector2.Lerp(visualStartScale, visualEndScale, normalized);
        netTransform.localScale = new Vector3(
            baseVisualScale.x * scaleFactor.x * activeSizeMultiplier,
            baseVisualScale.y * scaleFactor.y * activeSizeMultiplier,
            baseVisualScale.z);

        if (hitbox != null)
        {
            hitbox.size = Vector2.Lerp(minCatchSize, netSize, normalized) * activeSizeMultiplier;
        }
    }

    private void BuildNetLines()
    {
        float halfWidth = netSize.x * 0.5f;
        float halfHeight = netSize.y * 0.5f;
        Color lineColor = new Color(0.62f, 0.95f, 1f, 0.86f);

        AddLine("Net Top", new Vector2(-halfWidth, halfHeight), new Vector2(halfWidth, halfHeight), lineColor, 0.07f);
        AddLine("Net Bottom", new Vector2(-halfWidth, -halfHeight), new Vector2(halfWidth, -halfHeight), lineColor, 0.07f);
        AddLine("Net Left", new Vector2(-halfWidth, halfHeight), new Vector2(-halfWidth, -halfHeight), lineColor, 0.07f);
        AddLine("Net Right", new Vector2(halfWidth, halfHeight), new Vector2(halfWidth, -halfHeight), lineColor, 0.07f);

        for (int i = 1; i < 6; i++)
        {
            float x = Mathf.Lerp(-halfWidth, halfWidth, i / 6f);
            AddLine("Net Vertical " + i, new Vector2(x, halfHeight), new Vector2(x, -halfHeight), lineColor, 0.035f);
        }

        for (int i = 1; i < 4; i++)
        {
            float y = Mathf.Lerp(-halfHeight, halfHeight, i / 4f);
            AddLine("Net Horizontal " + i, new Vector2(-halfWidth, y), new Vector2(halfWidth, y), lineColor, 0.035f);
        }

        AddLine("Net Diagonal A", new Vector2(-halfWidth, halfHeight), new Vector2(halfWidth, -halfHeight), lineColor, 0.03f);
        AddLine("Net Diagonal B", new Vector2(halfWidth, halfHeight), new Vector2(-halfWidth, -halfHeight), lineColor, 0.03f);
    }

    private void AddLine(string lineName, Vector2 start, Vector2 end, Color color, float width)
    {
        GameObject lineObject = new GameObject(lineName);
        lineObject.transform.SetParent(netTransform, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        line.startWidth = width;
        line.endWidth = width;
        line.startColor = color;
        line.endColor = color;
        line.sortingOrder = sortingOrder;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            line.sharedMaterial = new Material(shader);
        }

        netLines.Add(line);
    }

    private void SetNetColor(Color color)
    {
        if (netRenderer != null)
        {
            netRenderer.color = color;
        }

        for (int i = 0; i < netLines.Count; i++)
        {
            netLines[i].startColor = color;
            netLines[i].endColor = color;
        }
    }
}
