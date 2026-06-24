using System.Collections;
using TMPro;
using UnityEngine;

// Owns the fisherman presentation: notice flash, danger tint, optional line
// anchor position, and the flee/return animation between levels.
public class FishermanController : MonoBehaviour
{
    public Transform lineAnchor;
    public Transform idleTackleVisual;
    public SpriteRenderer fishermanRenderer;
    public SpriteRenderer boatRenderer;
    public TMP_Text exclamationText;
    public TMP_Text nameText;

    private Vector3 homePosition;
    private Color homeColor = Color.white;

    public FishFishermanType Variant { get; private set; } = FishFishermanType.Net;
    public Vector3 LineAnchorPosition => lineAnchor != null ? lineAnchor.position : transform.position;

    private void Awake()
    {
        homePosition = transform.position;

        if (fishermanRenderer != null)
        {
            homeColor = fishermanRenderer.color;
        }

        SetNotice(false);
    }

    public void SetVariant(FishFishermanType variant, int level)
    {
        Variant = variant;
        homeColor = GetVariantColor(variant);

        if (fishermanRenderer != null)
        {
            fishermanRenderer.color = homeColor;
        }

        if (boatRenderer != null)
        {
            // Keep the boat shape the same while adding a slight tint so the
            // whole rig reads as a different fisherman type.
            boatRenderer.color = Color.Lerp(Color.white, homeColor, 0.18f);
        }

        if (nameText != null)
        {
            nameText.text = GetVariantName(variant) + " " + level;
        }
    }

    public void SetNotice(bool noticed)
    {
        // Red tint and exclamation mark are the readable "danger" state.
        if (exclamationText != null)
        {
            exclamationText.gameObject.SetActive(noticed);
            exclamationText.text = noticed ? "!" : string.Empty;
        }

        if (fishermanRenderer != null)
        {
            fishermanRenderer.color = noticed ? new Color(1f, 0.45f, 0.45f) : homeColor;
        }
    }

    public void SetReelWarning(bool warning)
    {
        if (boatRenderer != null)
        {
            boatRenderer.color = warning ? new Color(1f, 0.45f, 0.35f) : Color.Lerp(Color.white, homeColor, 0.18f);
        }
    }

    public IEnumerator FleeAndReturn(int nextLevel, FishFishermanType nextVariant)
    {
        // Called after the big fish attack. The current fisherman leaves the
        // screen and a new numbered fisherman slides in for the next level.
        SetNotice(true);

        if (nameText != null)
        {
            nameText.text = "Fisherman flees!";
        }

        Vector3 start = transform.position;
        Vector3 fleeTarget = start + new Vector3(11f, 0.7f, 0f);
        yield return MoveTo(start, fleeTarget, 0.75f);

        SetNotice(false);
        transform.position = homePosition + new Vector3(-11f, 0.55f, 0f);
        SetVariant(nextVariant, nextLevel);

        yield return MoveTo(transform.position, homePosition, 0.8f);
    }

    public static string GetVariantName(FishFishermanType variant)
    {
        switch (variant)
        {
            case FishFishermanType.Claw:
                return "Claw Fisherman";
            case FishFishermanType.Electric:
                return "Electric Fisherman";
            default:
                return "Net Fisherman";
        }
    }

    private static Color GetVariantColor(FishFishermanType variant)
    {
        switch (variant)
        {
            case FishFishermanType.Claw:
                return new Color(1f, 0.58f, 0.22f);
            case FishFishermanType.Electric:
                return new Color(1f, 0.92f, 0.28f);
            default:
                return new Color(0.35f, 0.9f, 1f);
        }
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
}
