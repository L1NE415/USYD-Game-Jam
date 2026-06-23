using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
// Optional warning line renderer between the fisherman and the player. The
// current net-sweep design keeps it hidden, but the helper remains useful if a
// later hazard wants a simple line telegraph.
public class FishingLineView : MonoBehaviour
{
    public FishGameManager gameManager;
    public FishermanController fisherman;
    public FishPlayerController player;
    public Color normalColor = new Color(0.85f, 0.95f, 1f, 0.95f);
    public Color warningColor = new Color(1f, 0.1f, 0.08f, 1f);

    private LineRenderer line;
    private bool visible;
    private bool warning;

    private void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 0;
        line.useWorldSpace = true;
        line.startWidth = 0.035f;
        line.endWidth = 0.025f;
    }

    private void LateUpdate()
    {
        // LateUpdate keeps the line endpoints synced after player/fisherman move.
        if (!visible || fisherman == null || player == null)
        {
            line.positionCount = 0;
            return;
        }

        line.positionCount = 2;
        line.SetPosition(0, fisherman.LineAnchorPosition);
        line.SetPosition(1, player.transform.position);

        Color color = warning ? warningColor : normalColor;
        line.startColor = color;
        line.endColor = color;
    }

    public void SetLineVisible(bool value)
    {
        visible = value;
        if (!visible)
        {
            warning = false;
        }
    }

    public void SetWarning(bool value)
    {
        warning = value;
    }
}
