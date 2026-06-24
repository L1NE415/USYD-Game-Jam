using UnityEngine;

public class WavingSeaweed : MonoBehaviour
{
    public float waveAngle = 4.5f;
    public float waveSpeed = 1.05f;
    public float phaseStep = 0.58f;
    public float rootDriftAmount = 0f;
    public float bottomAnchorInset = 0.02f;

    private Transform[] stems;
    private Quaternion[] baseLocalRotations;
    private Vector3[] baseLocalScales;
    private Vector3[] localBottomAnchors;
    private Vector3[] parentBottomAnchors;

    private void OnEnable()
    {
        CacheStems();
    }

    private void OnTransformChildrenChanged()
    {
        CacheStems();
    }

    private void Update()
    {
        if (stems == null || stems.Length != transform.childCount)
        {
            CacheStems();
        }

        float time = Time.time * waveSpeed;
        for (int i = 0; i < stems.Length; i++)
        {
            if (stems[i] == null)
            {
                continue;
            }

            float phase = i * phaseStep;
            float wave = Mathf.Sin(time + phase) * waveAngle;
            float rootDrift = Mathf.Sin(time * 0.45f + phase) * rootDriftAmount;
            Quaternion targetRotation = baseLocalRotations[i] * Quaternion.Euler(0f, 0f, wave);
            Vector3 scaledAnchor = Vector3.Scale(localBottomAnchors[i], baseLocalScales[i]);

            stems[i].localRotation = targetRotation;
            stems[i].localPosition = parentBottomAnchors[i] - targetRotation * scaledAnchor + new Vector3(rootDrift, 0f, 0f);
        }
    }

    private void CacheStems()
    {
        int childCount = transform.childCount;
        stems = new Transform[childCount];
        baseLocalRotations = new Quaternion[childCount];
        baseLocalScales = new Vector3[childCount];
        localBottomAnchors = new Vector3[childCount];
        parentBottomAnchors = new Vector3[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = transform.GetChild(i);
            Vector3 localBottomAnchor = GetLocalBottomAnchor(child);
            Vector3 scaledAnchor = Vector3.Scale(localBottomAnchor, child.localScale);

            stems[i] = child;
            baseLocalRotations[i] = child.localRotation;
            baseLocalScales[i] = child.localScale;
            localBottomAnchors[i] = localBottomAnchor;
            parentBottomAnchors[i] = child.localPosition + child.localRotation * scaledAnchor;
        }
    }

    private Vector3 GetLocalBottomAnchor(Transform child)
    {
        SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null || spriteRenderer.sprite == null)
        {
            return Vector3.zero;
        }

        Bounds bounds = spriteRenderer.sprite.bounds;
        float inset = Mathf.Clamp01(bottomAnchorInset) * bounds.size.y;
        return new Vector3(bounds.center.x, bounds.min.y + inset, 0f);
    }
}
