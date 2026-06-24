using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Large pendulum-style net hazard used when Alert reaches 100.
// The hazard is driven by a normal SpriteRenderer so artists can replace
// net.png or add an Animator to the "Net Sprite" child without touching code.
public class NetSweepHazard : MonoBehaviour
{
    public Vector3 pivotPosition = new Vector3(0f, 3.7f, 0f);
    public Vector2 netSize = new Vector2(8f, 4.5f);
    public Vector2 netLocalOffset = new Vector2(0f, -3.9f);
    public float swingAngle = 62f;
    public float warningSeconds = 0.7f;
    public float sweepSeconds = 3.1f;
    public float recoverSeconds = 0.25f;
    public int sortingOrder = 45;
    public Sprite netSprite;
    public SpriteRenderer netRenderer;
    public bool useLineFallback = true;

    private readonly List<LineRenderer> netLines = new List<LineRenderer>();
    private Transform netTransform;
    private BoxCollider2D hitbox;
    private bool isPlaying;

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

    public IEnumerator PlaySweep(FishPlayerController player, int level)
    {
        isPlaying = true;
        BuildVisualIfNeeded();

        gameObject.SetActive(true);
        transform.position = pivotPosition;
        CaughtPlayer = false;
        Progress = 0f;

        Collider2D playerCollider = player != null ? player.GetComponent<Collider2D>() : null;
        float direction = level % 2 == 0 ? -1f : 1f;
        float startAngle = -swingAngle * direction;
        float endAngle = swingAngle * direction;

        hitbox.enabled = false;
        SetNetColor(new Color(1f, 0.9f, 0.35f, 0.55f));
        transform.rotation = Quaternion.Euler(0f, 0f, startAngle);

        float elapsed = 0f;
        while (elapsed < warningSeconds)
        {
            elapsed += Time.deltaTime;
            Progress = Mathf.Clamp01(elapsed / warningSeconds) * 0.18f;
            yield return null;
        }

        hitbox.enabled = true;
        SetNetColor(new Color(0.62f, 0.95f, 1f, 0.86f));

        elapsed = 0f;
        while (elapsed < sweepSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / sweepSeconds);
            float eased = Mathf.SmoothStep(0f, 1f, t);
            transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(startAngle, endAngle, eased));
            Progress = Mathf.Lerp(0.18f, 1f, t);

            if (playerCollider != null)
            {
                Physics2D.SyncTransforms();
                if (hitbox.Distance(playerCollider).isOverlapped)
                {
                    CaughtPlayer = true;
                    Progress = 1f;
                    hitbox.enabled = false;
                    SetNetColor(new Color(1f, 0.15f, 0.1f, 0.95f));
                    break;
                }
            }

            yield return null;
        }

        if (CaughtPlayer)
        {
            yield break;
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

        gameObject.SetActive(false);
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
            Transform child = transform.Find("Net Sprite");
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
            GameObject netObject = new GameObject("Net Sprite");
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
