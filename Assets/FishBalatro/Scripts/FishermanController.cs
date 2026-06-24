using System.Collections;
using TMPro;
using UnityEngine;

// Owns the fisherman presentation: notice flash, danger tint, optional line
// anchor position, and the flee/return animation between levels.
//
// Artist note: the three child roots named "Claw Fisherman", "Electric
// Fisherman", and "Net Fisherman" are intentionally real scene entities.
// Replace their sprites or add Animators there when making final fisherman art.
[ExecuteAlways]
public class FishermanController : MonoBehaviour
{
    [System.Serializable]
    public class FishermanVariantEntity
    {
        public FishFishermanType type;
        public GameObject root;
        public Transform lineAnchor;
        public Transform toolPropAnchor;
        public SpriteRenderer fishermanRenderer;
        public SpriteRenderer boatRenderer;
        public TMP_Text exclamationText;
        public TMP_Text nameText;
    }

    public Transform lineAnchor;
    public Transform idleTackleVisual;
    public SpriteRenderer fishermanRenderer;
    public SpriteRenderer boatRenderer;
    public TMP_Text exclamationText;
    public TMP_Text nameText;
    public FishermanVariantEntity clawFisherman = new FishermanVariantEntity { type = FishFishermanType.Claw };
    public FishermanVariantEntity electricFisherman = new FishermanVariantEntity { type = FishFishermanType.Electric };
    public FishermanVariantEntity netFisherman = new FishermanVariantEntity { type = FishFishermanType.Net };

    private Vector3 homePosition;
    private Color homeColor = Color.white;
    private FishermanVariantEntity activeEntity;
    private bool noticed;
    private bool reelWarning;

    public FishFishermanType Variant { get; private set; } = FishFishermanType.Claw;
    public Vector3 LineAnchorPosition => activeEntity != null && activeEntity.lineAnchor != null ? activeEntity.lineAnchor.position : transform.position;

    private void Awake()
    {
        homePosition = transform.position;
        EnsureVariantEntities();

        activeEntity = GetEntity(Variant);
        if (activeEntity != null && activeEntity.fishermanRenderer != null)
        {
            homeColor = activeEntity.fishermanRenderer.color;
        }

        SetNotice(false);
    }

    private void OnEnable()
    {
        if (!Application.isPlaying && gameObject.scene.IsValid())
        {
            EnsureVariantEntities();
            SetVariant(Variant, 1);
            SetNotice(false);
        }
    }

    private void OnValidate()
    {
        SetDefaultEntityTypes();

        if (!Application.isPlaying && gameObject.scene.IsValid())
        {
            EnsureVariantEntities();
            SetVariant(Variant, 1);
            SetNotice(false);
        }
    }

    public void SetVariant(FishFishermanType variant, int level)
    {
        EnsureVariantEntities();
        Variant = variant;
        homeColor = GetVariantColor(variant);
        activeEntity = GetEntity(variant);
        SetOnlyActiveEntity(activeEntity);
        CopyActiveEntityToLegacyFields(activeEntity);

        if (activeEntity != null && activeEntity.fishermanRenderer != null)
        {
            activeEntity.fishermanRenderer.color = noticed ? new Color(1f, 0.45f, 0.45f) : homeColor;
        }

        if (activeEntity != null && activeEntity.boatRenderer != null)
        {
            // Keep the boat shape the same while adding a slight tint so the
            // whole rig reads as a different fisherman type.
            activeEntity.boatRenderer.color = reelWarning ? new Color(1f, 0.45f, 0.35f) : Color.Lerp(Color.white, homeColor, 0.18f);
        }

        if (activeEntity != null && activeEntity.nameText != null)
        {
            activeEntity.nameText.text = GetVariantName(variant) + " " + level;
        }
    }

    public void SetNotice(bool noticed)
    {
        this.noticed = noticed;
        FishermanVariantEntity entity = activeEntity ?? GetEntity(Variant);

        // Red tint and exclamation mark are the readable "danger" state.
        if (entity != null && entity.exclamationText != null)
        {
            entity.exclamationText.gameObject.SetActive(noticed);
            entity.exclamationText.text = noticed ? "!" : string.Empty;
        }

        if (entity != null && entity.fishermanRenderer != null)
        {
            entity.fishermanRenderer.color = noticed ? new Color(1f, 0.45f, 0.45f) : homeColor;
        }
    }

    public void SetReelWarning(bool warning)
    {
        reelWarning = warning;
        FishermanVariantEntity entity = activeEntity ?? GetEntity(Variant);
        if (entity != null && entity.boatRenderer != null)
        {
            entity.boatRenderer.color = warning ? new Color(1f, 0.45f, 0.35f) : Color.Lerp(Color.white, homeColor, 0.18f);
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

    public bool EnsureVariantEntities()
    {
        SetDefaultEntityTypes();
        Sprite boatSprite = boatRenderer != null ? boatRenderer.sprite : null;
        Sprite bodySprite = fishermanRenderer != null ? fishermanRenderer.sprite : null;
        bool changed = false;

        changed |= EnsureEntity(ref clawFisherman, FishFishermanType.Claw, "Claw Fisherman", boatSprite, bodySprite);
        changed |= EnsureEntity(ref electricFisherman, FishFishermanType.Electric, "Electric Fisherman", boatSprite, bodySprite);
        changed |= EnsureEntity(ref netFisherman, FishFishermanType.Net, "Net Fisherman", boatSprite, bodySprite);
        changed |= DisableLegacyTemplateRenderer(boatRenderer, "Boat");
        changed |= DisableLegacyTemplateRenderer(fishermanRenderer, "Fisherman");

        activeEntity = GetEntity(Variant);
        SetOnlyActiveEntity(activeEntity);
        CopyActiveEntityToLegacyFields(activeEntity);
        return changed;
    }

    private void SetDefaultEntityTypes()
    {
        if (clawFisherman == null)
        {
            clawFisherman = new FishermanVariantEntity();
        }
        if (electricFisherman == null)
        {
            electricFisherman = new FishermanVariantEntity();
        }
        if (netFisherman == null)
        {
            netFisherman = new FishermanVariantEntity();
        }

        clawFisherman.type = FishFishermanType.Claw;
        electricFisherman.type = FishFishermanType.Electric;
        netFisherman.type = FishFishermanType.Net;
    }

    private bool EnsureEntity(ref FishermanVariantEntity entity, FishFishermanType type, string rootName, Sprite boatSprite, Sprite bodySprite)
    {
        bool changed = false;
        if (entity == null)
        {
            entity = new FishermanVariantEntity();
            changed = true;
        }

        entity.type = type;
        if (entity.root == null)
        {
            Transform existing = transform.Find(rootName);
            if (existing == null)
            {
                GameObject rootObject = new GameObject(rootName);
                rootObject.transform.SetParent(transform, false);
                existing = rootObject.transform;
                changed = true;
            }

            entity.root = existing.gameObject;
        }

        entity.root.transform.localPosition = Vector3.zero;
        entity.root.transform.localRotation = Quaternion.identity;
        entity.root.transform.localScale = Vector3.one;

        changed |= EnsureVariantSprite(ref entity.boatRenderer, entity.root.transform, "Boat", new Vector3(0f, 0f, 0f), boatSprite, 10, Color.Lerp(Color.white, GetVariantColor(type), 0.18f));
        changed |= EnsureVariantSprite(ref entity.fishermanRenderer, entity.root.transform, "Fisherman Body", new Vector3(0.38f, 0.68f, 0f), bodySprite, 12, GetVariantColor(type));
        changed |= EnsureMarker(ref entity.lineAnchor, entity.root.transform, "Line Anchor", new Vector3(1.25f, 0.22f, 0f));
        changed |= EnsureMarker(ref entity.toolPropAnchor, entity.root.transform, GetToolPropName(type), new Vector3(0.72f, 0.66f, 0f));
        changed |= EnsureText(ref entity.exclamationText, entity.root.transform, "Notice", "!", new Vector3(0.5f, 1.55f, 0f), 3.8f, Color.red, 70);
        changed |= EnsureText(ref entity.nameText, entity.root.transform, "FishermanName", GetVariantName(type), new Vector3(-0.85f, 1.48f, 0f), 0.85f, Color.white, 65);

        return changed;
    }

    private static bool EnsureVariantSprite(ref SpriteRenderer renderer, Transform parent, string name, Vector3 localPosition, Sprite sprite, int sortingOrder, Color color)
    {
        bool changed = false;
        if (renderer == null)
        {
            Transform existing = parent.Find(name);
            if (existing == null)
            {
                GameObject obj = new GameObject(name);
                obj.transform.SetParent(parent, false);
                existing = obj.transform;
                changed = true;
            }

            renderer = existing.GetComponent<SpriteRenderer>();
            if (renderer == null)
            {
                renderer = existing.gameObject.AddComponent<SpriteRenderer>();
                changed = true;
            }
        }

        renderer.transform.localPosition = localPosition;
        renderer.transform.localRotation = Quaternion.identity;
        renderer.transform.localScale = Vector3.one;
        renderer.sortingOrder = sortingOrder;
        renderer.color = color;
        if (renderer.sprite == null && sprite != null)
        {
            renderer.sprite = sprite;
            changed = true;
        }

        return changed;
    }

    private static bool EnsureMarker(ref Transform marker, Transform parent, string name, Vector3 localPosition)
    {
        bool changed = false;
        if (marker == null)
        {
            Transform existing = parent.Find(name);
            if (existing == null)
            {
                GameObject obj = new GameObject(name);
                obj.transform.SetParent(parent, false);
                existing = obj.transform;
                changed = true;
            }

            marker = existing;
        }

        marker.localPosition = localPosition;
        marker.localRotation = Quaternion.identity;
        marker.localScale = Vector3.one;
        return changed;
    }

    private static bool EnsureText(ref TMP_Text text, Transform parent, string name, string value, Vector3 localPosition, float fontSize, Color color, int sortingOrder)
    {
        bool changed = false;
        if (text == null)
        {
            Transform existing = parent.Find(name);
            if (existing == null)
            {
                GameObject obj = new GameObject(name);
                obj.transform.SetParent(parent, false);
                existing = obj.transform;
                changed = true;
            }

            text = existing.GetComponent<TextMeshPro>();
            if (text == null)
            {
                text = existing.gameObject.AddComponent<TextMeshPro>();
                changed = true;
            }
        }

        text.transform.localPosition = localPosition;
        text.transform.localRotation = Quaternion.identity;
        text.transform.localScale = Vector3.one;
        text.text = value;
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.richText = true;

        MeshRenderer meshRenderer = text.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = sortingOrder;
        }

        return changed;
    }

    private bool DisableLegacyTemplateRenderer(SpriteRenderer renderer, string expectedName)
    {
        if (renderer == null || renderer.transform.parent != transform || renderer.gameObject.name != expectedName || !renderer.enabled)
        {
            return false;
        }

        renderer.enabled = false;
        return true;
    }

    private FishermanVariantEntity GetEntity(FishFishermanType type)
    {
        switch (type)
        {
            case FishFishermanType.Claw:
                return clawFisherman;
            case FishFishermanType.Electric:
                return electricFisherman;
            default:
                return netFisherman;
        }
    }

    private void SetOnlyActiveEntity(FishermanVariantEntity entity)
    {
        SetEntityVisible(clawFisherman, entity);
        SetEntityVisible(electricFisherman, entity);
        SetEntityVisible(netFisherman, entity);
    }

    private static void SetEntityVisible(FishermanVariantEntity entity, FishermanVariantEntity active)
    {
        if (entity != null && entity.root != null)
        {
            entity.root.SetActive(entity == active);
        }
    }

    private void CopyActiveEntityToLegacyFields(FishermanVariantEntity entity)
    {
        if (entity == null)
        {
            return;
        }

        lineAnchor = entity.lineAnchor;
        idleTackleVisual = entity.toolPropAnchor;
        fishermanRenderer = entity.fishermanRenderer;
        boatRenderer = entity.boatRenderer;
        exclamationText = entity.exclamationText;
        nameText = entity.nameText;
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

    private static string GetToolPropName(FishFishermanType variant)
    {
        switch (variant)
        {
            case FishFishermanType.Claw:
                return "Claw Tool Prop";
            case FishFishermanType.Electric:
                return "Electric Tool Prop";
            default:
                return "Net Tool Prop";
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
