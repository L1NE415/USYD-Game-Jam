using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Electric fisherman hazard. Horizontal electric lines activate from the upper
// water downward, leaving fish-sized gaps between each layer.
public class ElectricWaveHazard : MonoBehaviour
{
    public float xMin = -8f;
    public float xMax = 8f;
    public float topY = 2.8f;
    public float bottomY = -3.2f;
    public float waveSpacing = 1.35f;
    public float warningSeconds = 0.35f;
    public float activeSeconds = 0.35f;
    public float afterWaveSeconds = 0.18f;
    public int sortingOrder = 47;

    private readonly List<LineRenderer> waveLines = new List<LineRenderer>();

    public bool CaughtPlayer { get; private set; }
    public float Progress { get; private set; }

    public static ElectricWaveHazard CreateRuntimeElectric()
    {
        GameObject root = new GameObject("Electric Wave Hazard");
        return root.AddComponent<ElectricWaveHazard>();
    }

    private void Awake()
    {
        Hide();
    }

    public IEnumerator PlayWaves(FishPlayerController player, int level)
    {
        gameObject.SetActive(true);
        CaughtPlayer = false;
        Progress = 0f;

        int waveCount = Mathf.Max(1, Mathf.FloorToInt((topY - bottomY) / waveSpacing) + 1);
        EnsureLineCount(waveCount);
        HideLines();

        Collider2D playerCollider = player != null ? player.GetComponent<Collider2D>() : null;
        for (int i = 0; i < waveCount; i++)
        {
            float y = topY - i * waveSpacing;
            LineRenderer line = waveLines[i];
            ConfigureLine(line, y, new Color(1f, 0.92f, 0.25f, 0.55f), 0.06f);

            float elapsed = 0f;
            while (elapsed < warningSeconds)
            {
                elapsed += Time.deltaTime;
                Progress = (i + Mathf.Clamp01(elapsed / warningSeconds) * 0.25f) / waveCount;
                yield return null;
            }

            ConfigureLine(line, y, new Color(0.38f, 0.95f, 1f, 1f), 0.11f);

            elapsed = 0f;
            while (elapsed < activeSeconds)
            {
                elapsed += Time.deltaTime;
                Progress = (i + Mathf.Clamp01(elapsed / activeSeconds)) / waveCount;

                if (IsPlayerHit(playerCollider, player, y))
                {
                    CaughtPlayer = true;
                    Progress = 1f;
                    ConfigureLine(line, y, new Color(1f, 0.12f, 0.08f, 1f), 0.16f);
                    yield break;
                }

                yield return null;
            }

            ConfigureLine(line, y, new Color(0.38f, 0.95f, 1f, 0.22f), 0.06f);
            yield return new WaitForSeconds(afterWaveSeconds);
        }

        Hide();
    }

    public void Hide()
    {
        Progress = 0f;
        HideLines();
        gameObject.SetActive(false);
    }

    private bool IsPlayerHit(Collider2D playerCollider, FishPlayerController player, float y)
    {
        if (playerCollider != null)
        {
            Bounds bounds = playerCollider.bounds;
            return bounds.max.x >= xMin
                && bounds.min.x <= xMax
                && y >= bounds.min.y
                && y <= bounds.max.y;
        }

        if (player == null)
        {
            return false;
        }

        Vector3 position = player.transform.position;
        return position.x >= xMin && position.x <= xMax && Mathf.Abs(position.y - y) <= 0.45f;
    }

    private void EnsureLineCount(int count)
    {
        while (waveLines.Count < count)
        {
            GameObject lineObject = new GameObject("Electric Wave " + (waveLines.Count + 1));
            lineObject.transform.SetParent(transform, false);
            LineRenderer line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.positionCount = 0;
            line.numCapVertices = 3;
            line.sortingOrder = sortingOrder;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                line.sharedMaterial = new Material(shader);
            }

            waveLines.Add(line);
        }
    }

    private void ConfigureLine(LineRenderer line, float y, Color color, float width)
    {
        line.positionCount = 2;
        line.SetPosition(0, new Vector3(xMin, y, 0f));
        line.SetPosition(1, new Vector3(xMax, y, 0f));
        line.startColor = color;
        line.endColor = color;
        line.startWidth = width;
        line.endWidth = width;
    }

    private void HideLines()
    {
        for (int i = 0; i < waveLines.Count; i++)
        {
            if (waveLines[i] != null)
            {
                waveLines[i].positionCount = 0;
            }
        }
    }
}
