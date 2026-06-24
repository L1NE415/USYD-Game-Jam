using System.Collections;
using UnityEngine;

// Fires claw shots from the boat toward the sea floor. The volley uses three
// readable angles: 45 degrees down-right, 90 degrees straight down, and 135
// degrees down-left.
public class ClawShotHazard : MonoBehaviour
{
    public Vector3 pivotPosition = new Vector3(0f, 3.45f, 0f);
    public Sprite clawSprite;
    public float shotLength = 11f;
    public float warningSeconds = 0.45f;
    public float shotSeconds = 0.75f;
    public float betweenShotsSeconds = 0.18f;
    public float hitRadius = 0.48f;
    public int sortingOrder = 48;

    private readonly float[] shotAngles = { 45f, 90f, 135f };
    private LineRenderer pathLine;
    private SpriteRenderer clawRenderer;
    private bool isPlaying;

    public bool CaughtPlayer { get; private set; }
    public float Progress { get; private set; }

    public static ClawShotHazard CreateRuntimeClaw()
    {
        GameObject root = new GameObject("Claw Shot Hazard");
        return root.AddComponent<ClawShotHazard>();
    }

    private void Awake()
    {
        BuildVisualIfNeeded();
        if (!isPlaying)
        {
            Hide();
        }
    }

    public IEnumerator PlayVolley(FishPlayerController player, int level)
    {
        isPlaying = true;
        BuildVisualIfNeeded();
        gameObject.SetActive(true);
        transform.position = Vector3.zero;
        CaughtPlayer = false;
        Progress = 0f;

        int angleOffset = Mathf.Abs(level - 1) % shotAngles.Length;
        for (int i = 0; i < shotAngles.Length; i++)
        {
            float angle = shotAngles[(i + angleOffset) % shotAngles.Length];
            yield return PlaySingleShot(player, angle, i, shotAngles.Length);

            if (CaughtPlayer)
            {
                yield break;
            }

            yield return new WaitForSeconds(betweenShotsSeconds);
        }

        Hide();
    }

    public void Hide()
    {
        isPlaying = false;
        Progress = 0f;
        if (pathLine != null)
        {
            pathLine.positionCount = 0;
        }

        if (clawRenderer != null)
        {
            clawRenderer.enabled = false;
        }

        gameObject.SetActive(false);
    }

    private IEnumerator PlaySingleShot(FishPlayerController player, float angle, int shotIndex, int shotCount)
    {
        Vector2 start = pivotPosition;
        Vector2 direction = AngleToDownwardDirection(angle);
        Vector2 end = start + direction * shotLength;
        Collider2D playerCollider = player != null ? player.GetComponent<Collider2D>() : null;

        SetLine(start, end, new Color(1f, 0.85f, 0.25f, 0.5f), 0.045f);
        SetClaw(start, direction, new Color(1f, 0.85f, 0.25f, 0.75f));

        float elapsed = 0f;
        while (elapsed < warningSeconds)
        {
            elapsed += Time.deltaTime;
            Progress = (shotIndex + Mathf.Clamp01(elapsed / warningSeconds) * 0.25f) / shotCount;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < shotSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / shotSeconds));
            Vector2 currentEnd = Vector2.Lerp(start, end, t);
            SetLine(start, currentEnd, new Color(0.88f, 0.92f, 0.95f, 0.96f), 0.07f);
            SetClaw(currentEnd, direction, Color.white);
            Progress = (shotIndex + t) / shotCount;

            if (IsPlayerHit(playerCollider, player, start, currentEnd))
            {
                CaughtPlayer = true;
                Progress = 1f;
                SetLine(start, currentEnd, new Color(1f, 0.15f, 0.1f, 0.95f), 0.1f);
                SetClaw(currentEnd, direction, new Color(1f, 0.25f, 0.18f));
                yield break;
            }

            yield return null;
        }
    }

    private bool IsPlayerHit(Collider2D playerCollider, FishPlayerController player, Vector2 start, Vector2 end)
    {
        if (playerCollider != null)
        {
            return DistancePointToSegment(playerCollider.bounds.center, start, end) <= hitRadius;
        }

        return player != null && DistancePointToSegment(player.transform.position, start, end) <= hitRadius;
    }

    private void BuildVisualIfNeeded()
    {
        if (pathLine != null)
        {
            return;
        }

        Transform lineTransform = transform.Find("Claw Path");
        if (lineTransform == null)
        {
            GameObject lineObject = new GameObject("Claw Path");
            lineObject.transform.SetParent(transform, false);
            lineTransform = lineObject.transform;
        }

        pathLine = lineTransform.GetComponent<LineRenderer>();
        if (pathLine == null)
        {
            pathLine = lineTransform.gameObject.AddComponent<LineRenderer>();
        }

        pathLine.useWorldSpace = true;
        pathLine.positionCount = 0;
        pathLine.sortingOrder = sortingOrder;
        pathLine.numCapVertices = 3;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            pathLine.sharedMaterial = new Material(shader);
        }

        Transform clawTransform = transform.Find("Claw Head");
        if (clawTransform == null)
        {
            GameObject clawObject = new GameObject("Claw Head");
            clawObject.transform.SetParent(transform, false);
            clawTransform = clawObject.transform;
        }

        clawRenderer = clawTransform.GetComponent<SpriteRenderer>();
        if (clawRenderer == null)
        {
            clawRenderer = clawTransform.gameObject.AddComponent<SpriteRenderer>();
        }

        if (clawRenderer.sprite == null)
        {
            clawRenderer.sprite = clawSprite != null ? clawSprite : CreateProceduralClawSprite();
        }

        clawRenderer.sortingOrder = sortingOrder + 1;
        clawRenderer.enabled = false;
    }

    private void SetLine(Vector2 start, Vector2 end, Color color, float width)
    {
        pathLine.positionCount = 2;
        pathLine.SetPosition(0, start);
        pathLine.SetPosition(1, end);
        pathLine.startColor = color;
        pathLine.endColor = color;
        pathLine.startWidth = width;
        pathLine.endWidth = width;
    }

    private void SetClaw(Vector2 position, Vector2 direction, Color color)
    {
        clawRenderer.enabled = true;
        clawRenderer.transform.position = position;
        clawRenderer.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f);
        clawRenderer.color = color;
    }

    private static Vector2 AngleToDownwardDirection(float angle)
    {
        float radians = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(radians), -Mathf.Sin(radians)).normalized;
    }

    private static float DistancePointToSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        float lengthSquared = segment.sqrMagnitude;
        if (lengthSquared <= Mathf.Epsilon)
        {
            return Vector2.Distance(point, start);
        }

        float t = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared);
        Vector2 projection = start + segment * t;
        return Vector2.Distance(point, projection);
    }

    private static Sprite CreateProceduralClawSprite()
    {
        const int size = 24;
        Color32[] pixels = new Color32[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color32(0, 0, 0, 0);
        }

        Color32 metal = new Color32(224, 230, 235, 255);
        DrawLine(pixels, size, 12, 22, 12, 7, metal);
        DrawLine(pixels, size, 12, 7, 5, 2, metal);
        DrawLine(pixels, size, 12, 7, 19, 2, metal);
        DrawLine(pixels, size, 5, 2, 4, 8, metal);
        DrawLine(pixels, size, 19, 2, 20, 8, metal);
        DrawLine(pixels, size, 9, 6, 15, 6, new Color32(180, 188, 196, 255));

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 16f);
    }

    private static void DrawLine(Color32[] pixels, int size, int x0, int y0, int x1, int y1, Color32 color)
    {
        int dx = Mathf.Abs(x1 - x0);
        int sx = x0 < x1 ? 1 : -1;
        int dy = -Mathf.Abs(y1 - y0);
        int sy = y0 < y1 ? 1 : -1;
        int err = dx + dy;

        while (true)
        {
            if (x0 >= 0 && x0 < size && y0 >= 0 && y0 < size)
            {
                pixels[y0 * size + x0] = color;
            }

            if (x0 == x1 && y0 == y1)
            {
                break;
            }

            int e2 = 2 * err;
            if (e2 >= dy)
            {
                err += dy;
                x0 += sx;
            }
            if (e2 <= dx)
            {
                err += dx;
                y0 += sy;
            }
        }
    }
}
