using UnityEngine;
using System.Collections.Generic;

public class BubbleSpawner : MonoBehaviour
{
    public GameObject bubblePrefab;
    public float spawnInterval = 0.85f;
    public float spawnRangeX = 0.5f;
    public Vector2 spawnXRange = new Vector2(-8.4f, 8.4f);
    public float seaFloorY = -4.85f;
    public float waterSurfaceY = 3.55f;
    public Vector2 riseSpeedRange = new Vector2(0.42f, 0.62f);
    public Vector2 bubbleLifetimeRange = new Vector2(5f, 8f);
    public Vector2 bubbleScaleRange = new Vector2(0.65f, 1.05f);
    public Vector2 swayAmountRange = new Vector2(0.08f, 0.16f);
    public Vector2 swaySpeedRange = new Vector2(0.9f, 1.4f);
    public int maxActiveBubbles = 26;
    public bool useSingleSharedSpawner = true;

    private static BubbleSpawner activeSharedSpawner;
    private readonly List<GameObject> activeBubbles = new List<GameObject>();

    void Start()
    {
        if (useSingleSharedSpawner)
        {
            if (activeSharedSpawner != null && activeSharedSpawner != this)
            {
                enabled = false;
                return;
            }

            activeSharedSpawner = this;
        }

        InvokeRepeating(nameof(SpawnBubble), Random.Range(0f, spawnInterval), spawnInterval);
    }

    private void OnDisable()
    {
        CancelInvoke();
        if (activeSharedSpawner == this)
        {
            activeSharedSpawner = null;
        }
    }

    void SpawnBubble()
    {
        if (bubblePrefab == null)
        {
            return;
        }

        RemoveDestroyedBubbles();
        if (activeBubbles.Count >= maxActiveBubbles)
        {
            return;
        }

        Vector3 spawnPos = new Vector3(
            Random.Range(spawnXRange.x, spawnXRange.y),
            seaFloorY,
            transform.position.z
        );

        GameObject bubble = Instantiate(
            bubblePrefab,
            spawnPos,
            Quaternion.identity
        );

        float randomScale = Random.Range(bubbleScaleRange.x, bubbleScaleRange.y);
        bubble.transform.localScale = Vector3.one * randomScale;
        activeBubbles.Add(bubble);

        Bubble bubbleMotion = bubble.GetComponent<Bubble>();
        if (bubbleMotion != null)
        {
            float bubbleLifetime = Random.Range(bubbleLifetimeRange.x, bubbleLifetimeRange.y);
            bubbleMotion.lifeTime = bubbleLifetime;
            bubbleMotion.riseSpeed = Mathf.Max(0.1f, (waterSurfaceY - seaFloorY) / bubbleLifetime);
            bubbleMotion.swayAmount = Random.Range(swayAmountRange.x, swayAmountRange.y);
            bubbleMotion.swaySpeed = Random.Range(swaySpeedRange.x, swaySpeedRange.y);
            bubbleMotion.surfaceY = waterSurfaceY;
        }
    }

    private void RemoveDestroyedBubbles()
    {
        for (int i = activeBubbles.Count - 1; i >= 0; i--)
        {
            if (activeBubbles[i] == null)
            {
                activeBubbles.RemoveAt(i);
            }
        }
    }
}
