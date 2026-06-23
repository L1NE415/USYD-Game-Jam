using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
// Draws the visible fishing line only while the player is hooked. It does not
// apply physics; FishGameManager calculates the pull force.
public class FishingLineView : MonoBehaviour
{
    public FishGameManager gameManager;
    public FishermanController fisherman;
    public FishPlayerController player;
    public Color normalColor = new Color(0.85f, 0.95f, 1f, 0.95f);
    public Color warningColor = new Color(1f, 0.1f, 0.08f, 1f);

    private LineRenderer line;
    private bool hooked;
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
        if (!hooked || fisherman == null || player == null)
        {
            line.positionCount = 0;
            return;
        }

        line.positionCount = 2;
        line.SetPosition(0, fisherman.HookAnchorPosition);
        line.SetPosition(1, player.transform.position);

        Color color = warning ? warningColor : normalColor;
        line.startColor = color;
        line.endColor = color;
    }

    public void SetHooked(bool value)
    {
        hooked = value;
        if (!hooked)
        {
            warning = false;
        }
    }

    public void SetWarning(bool value)
    {
        warning = value;
    }
}
