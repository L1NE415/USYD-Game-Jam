using UnityEngine;

[ExecuteAlways]
public class SkySeagull : MonoBehaviour
{
    [Header("Optional Art")]
    public Sprite bodySprite;
    public Sprite wingSprite;
    public SpriteRenderer bodyRenderer;
    public SpriteRenderer leftWingRenderer;
    public SpriteRenderer rightWingRenderer;

    [Header("Flight")]
    public Vector2 flightXRange = new Vector2(-9.6f, 9.6f);
    public float flightHeight = 4.45f;
    public float flightSpeed = 1.35f;
    public float bobAmplitude = 0.08f;
    public float bobFrequency = 1.1f;
    public bool startFacingRight = true;

    [Header("Wing Animation")]
    public float flapSpeed = 7.5f;
    public float flapAngle = 34f;
    public int sortingOrder = 18;

    private const float BodyPixelsPerUnit = 16f;
    private static Sprite generatedBodySprite;
    private static Sprite generatedWingSprite;
    private float phase;
    private int direction = 1;

    private void Reset()
    {
        transform.position = new Vector3(-8.8f, flightHeight, 0f);
        transform.localScale = new Vector3(0.34f, 0.34f, 1f);
    }

    private void OnEnable()
    {
        direction = startFacingRight ? 1 : -1;
        if (phase <= 0f)
        {
            phase = Mathf.Abs(transform.position.x) * 0.37f + Mathf.Abs(transform.position.y) * 0.19f;
        }

        EnsureParts();
        ApplyFacing();
        ApplyWingPose(GetAnimationTime());
    }

    private void Update()
    {
        EnsureParts();

        float time = GetAnimationTime();
        if (Application.isPlaying)
        {
            MoveAcrossSky(time);
        }

        ApplyFacing();
        ApplyWingPose(time);
    }

    private void MoveAcrossSky(float time)
    {
        Vector3 position = transform.position;
        position.x += direction * flightSpeed * Time.deltaTime;
        position.y = flightHeight + Mathf.Sin(time * bobFrequency + phase) * bobAmplitude;

        float minX = Mathf.Min(flightXRange.x, flightXRange.y);
        float maxX = Mathf.Max(flightXRange.x, flightXRange.y);
        float padding = 0.7f;

        if (direction > 0 && position.x > maxX + padding)
        {
            position.x = minX - padding;
        }
        else if (direction < 0 && position.x < minX - padding)
        {
            position.x = maxX + padding;
        }

        transform.position = position;
    }

    private void ApplyFacing()
    {
        Vector3 scale = transform.localScale;
        float xScale = Mathf.Abs(scale.x);
        transform.localScale = new Vector3(xScale * direction, Mathf.Abs(scale.y), scale.z == 0f ? 1f : scale.z);
    }

    private void ApplyWingPose(float time)
    {
        float flap = Mathf.Sin(time * flapSpeed + phase);
        float rightAngle = 10f + flap * flapAngle;
        float leftAngle = -10f - flap * flapAngle;
        float bodyTilt = Mathf.Sin(time * bobFrequency + phase) * 3.5f;

        if (bodyRenderer != null)
        {
            bodyRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, bodyTilt);
            bodyRenderer.sortingOrder = sortingOrder;
        }

        if (rightWingRenderer != null)
        {
            rightWingRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, rightAngle);
            rightWingRenderer.sortingOrder = sortingOrder + 1;
        }

        if (leftWingRenderer != null)
        {
            leftWingRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, leftAngle);
            leftWingRenderer.sortingOrder = sortingOrder - 1;
        }
    }

    private void EnsureParts()
    {
        Sprite resolvedBody = bodySprite != null ? bodySprite : GetGeneratedBodySprite();
        Sprite resolvedWing = wingSprite != null ? wingSprite : GetGeneratedWingSprite();

        bodyRenderer = EnsureRenderer(bodyRenderer, "Body", resolvedBody, Vector3.zero, Vector3.one, false);
        leftWingRenderer = EnsureRenderer(leftWingRenderer, "Left Wing", resolvedWing, new Vector3(-0.08f, 0.04f, 0f), new Vector3(-1f, 1f, 1f), true);
        rightWingRenderer = EnsureRenderer(rightWingRenderer, "Right Wing", resolvedWing, new Vector3(0.08f, 0.04f, 0f), Vector3.one, true);
    }

    private SpriteRenderer EnsureRenderer(SpriteRenderer renderer, string childName, Sprite sprite, Vector3 localPosition, Vector3 localScale, bool wing)
    {
        if (renderer == null)
        {
            Transform existing = transform.Find(childName);
            if (existing != null)
            {
                renderer = existing.GetComponent<SpriteRenderer>();
            }
        }

        if (renderer == null)
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(transform, false);
            renderer = child.AddComponent<SpriteRenderer>();
        }

        renderer.transform.localPosition = localPosition;
        renderer.transform.localScale = localScale;
        renderer.sprite = sprite;
        renderer.color = wing ? new Color(0.95f, 0.98f, 1f, 1f) : Color.white;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private static Sprite GetGeneratedBodySprite()
    {
        if (generatedBodySprite != null)
        {
            return generatedBodySprite;
        }

        Color32 clear = new Color32(0, 0, 0, 0);
        Color32 white = new Color32(240, 248, 252, 255);
        Color32 shadow = new Color32(164, 182, 194, 255);
        Color32 beak = new Color32(244, 178, 62, 255);
        Color32 eye = new Color32(18, 24, 32, 255);
        Texture2D texture = CreateTexture(34, 16, clear);
        Color32[] pixels = texture.GetPixels32();

        DrawTriangle(pixels, 34, 16, new Vector2Int(3, 8), new Vector2Int(10, 12), new Vector2Int(10, 4), shadow);
        DrawEllipse(pixels, 34, 16, 18, 8, 10, 5, white);
        DrawEllipse(pixels, 34, 16, 21, 8, 7, 3, new Color32(255, 255, 255, 255));
        DrawTriangle(pixels, 34, 16, new Vector2Int(28, 8), new Vector2Int(33, 10), new Vector2Int(33, 6), beak);
        DrawRect(pixels, 34, 16, 24, 9, 2, 2, eye);
        DrawRect(pixels, 34, 16, 12, 5, 10, 2, shadow);

        texture.SetPixels32(pixels);
        texture.Apply();
        generatedBodySprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), BodyPixelsPerUnit);
        generatedBodySprite.name = "Generated Seagull Body";
        return generatedBodySprite;
    }

    private static Sprite GetGeneratedWingSprite()
    {
        if (generatedWingSprite != null)
        {
            return generatedWingSprite;
        }

        Color32 clear = new Color32(0, 0, 0, 0);
        Color32 white = new Color32(245, 250, 253, 255);
        Color32 tip = new Color32(128, 148, 164, 255);
        Texture2D texture = CreateTexture(34, 16, clear);
        Color32[] pixels = texture.GetPixels32();

        DrawTriangle(pixels, 34, 16, new Vector2Int(2, 7), new Vector2Int(30, 14), new Vector2Int(18, 6), white);
        DrawTriangle(pixels, 34, 16, new Vector2Int(2, 7), new Vector2Int(26, 2), new Vector2Int(18, 6), new Color32(226, 238, 245, 255));
        DrawTriangle(pixels, 34, 16, new Vector2Int(24, 13), new Vector2Int(32, 15), new Vector2Int(27, 10), tip);
        DrawTriangle(pixels, 34, 16, new Vector2Int(23, 3), new Vector2Int(31, 1), new Vector2Int(27, 6), tip);

        texture.SetPixels32(pixels);
        texture.Apply();
        generatedWingSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.08f, 0.5f), BodyPixelsPerUnit);
        generatedWingSprite.name = "Generated Seagull Wing";
        return generatedWingSprite;
    }

    private static Texture2D CreateTexture(int width, int height, Color32 fill)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Color32[] pixels = new Color32[width * height];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = fill;
        }

        texture.SetPixels32(pixels);
        return texture;
    }

    private static void DrawRect(Color32[] pixels, int width, int height, int x, int y, int rectWidth, int rectHeight, Color32 color)
    {
        int minX = Mathf.Clamp(x, 0, width);
        int minY = Mathf.Clamp(y, 0, height);
        int maxX = Mathf.Clamp(x + rectWidth, 0, width);
        int maxY = Mathf.Clamp(y + rectHeight, 0, height);

        for (int py = minY; py < maxY; py++)
        {
            for (int px = minX; px < maxX; px++)
            {
                pixels[py * width + px] = color;
            }
        }
    }

    private static void DrawEllipse(Color32[] pixels, int width, int height, int centerX, int centerY, int radiusX, int radiusY, Color32 color)
    {
        for (int py = centerY - radiusY; py <= centerY + radiusY; py++)
        {
            for (int px = centerX - radiusX; px <= centerX + radiusX; px++)
            {
                if (px < 0 || px >= width || py < 0 || py >= height)
                {
                    continue;
                }

                float nx = (px - centerX) / (float)radiusX;
                float ny = (py - centerY) / (float)radiusY;
                if (nx * nx + ny * ny <= 1f)
                {
                    pixels[py * width + px] = color;
                }
            }
        }
    }

    private static void DrawTriangle(Color32[] pixels, int width, int height, Vector2Int a, Vector2Int b, Vector2Int c, Color32 color)
    {
        int minX = Mathf.Clamp(Mathf.Min(a.x, Mathf.Min(b.x, c.x)), 0, width - 1);
        int maxX = Mathf.Clamp(Mathf.Max(a.x, Mathf.Max(b.x, c.x)), 0, width - 1);
        int minY = Mathf.Clamp(Mathf.Min(a.y, Mathf.Min(b.y, c.y)), 0, height - 1);
        int maxY = Mathf.Clamp(Mathf.Max(a.y, Mathf.Max(b.y, c.y)), 0, height - 1);

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (PointInTriangle(new Vector2(x, y), a, b, c))
                {
                    pixels[y * width + x] = color;
                }
            }
        }
    }

    private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = Sign(p, a, b);
        float area2 = Sign(p, b, c);
        float area3 = Sign(p, c, a);
        bool hasNegative = area < 0f || area2 < 0f || area3 < 0f;
        bool hasPositive = area > 0f || area2 > 0f || area3 > 0f;
        return !(hasNegative && hasPositive);
    }

    private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }

    private static float GetAnimationTime()
    {
        return Application.isPlaying ? Time.time : Time.realtimeSinceStartup;
    }
}
