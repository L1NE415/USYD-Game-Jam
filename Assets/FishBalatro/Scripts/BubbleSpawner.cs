using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    public GameObject bubblePrefab;
    public float spawnInterval = 0.4f;
    public float spawnRangeX = 0.5f;

    void Start()
    {
        InvokeRepeating(nameof(SpawnBubble), 0f, spawnInterval);
    }

    void SpawnBubble()
    {
        Vector3 spawnPos = transform.position;
        spawnPos.x += Random.Range(-spawnRangeX, spawnRangeX);

        GameObject bubble = Instantiate(
            bubblePrefab,
            spawnPos,
            Quaternion.identity
        );

        float randomScale = Random.Range(0.6f, 1.2f);
        bubble.transform.localScale = Vector3.one * randomScale;
    }
}