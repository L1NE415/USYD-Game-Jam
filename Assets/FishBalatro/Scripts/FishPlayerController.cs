using UnityEngine;

#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
// Handles only fish movement input and physics. Game rules such as scoring,
// net sweeps, and big fish attacks stay in FishGameManager.
public class FishPlayerController : MonoBehaviour
{
    public static readonly Vector2 DefaultArenaMin = new Vector2(-8f, -4.65f);
    public static readonly Vector2 DefaultArenaMax = new Vector2(8f, 3.55f);

    public FishGameManager gameManager;
    public float normalSpeed = 5.6f;
    public float dashSpeed = 10.5f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 0.65f;
    public Vector2 arenaMin = DefaultArenaMin;
    public Vector2 arenaMax = DefaultArenaMax;
    public bool useBoundaryObjects = true;
    public Transform leftWallBoundary;
    public Transform rightWallBoundary;
    public Transform seaFloorBoundary;
    public Transform waterSurfaceBoundary;
    public Vector4 boundaryArenaOffsets = new Vector4(0.94f, -0.94f, 0.45f, -0.05f);

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private Vector2 dashDirection = Vector2.right;
    private float dashTimer;
    private float dashCooldownTimer;

    public Vector2 MoveInput => moveInput;
    public bool IsDashing => dashTimer > 0f;

    private void Reset()
    {
        // Only used when the component is first added. Existing Inspector
        // tweaks stay serialized in the scene and are not overwritten at play.
        arenaMin = DefaultArenaMin;
        arenaMax = DefaultArenaMax;
        useBoundaryObjects = true;
        boundaryArenaOffsets = new Vector4(0.94f, -0.94f, 0.45f, -0.05f);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        if (gameManager == null)
        {
            gameManager = FishGameManager.Instance;
        }

        RefreshArenaFromBoundaryObjects();
    }

    private void Update()
    {
        if (gameManager == null)
        {
            gameManager = FishGameManager.Instance;
        }
        // Freeze the small fish during the big fish attack cutscene so the
        // level transition has a clear, readable moment.
        bool movementLocked = gameManager != null
            && (gameManager.State == FishGameState.BigFishAttack || gameManager.State == FishGameState.Caught);
        moveInput = movementLocked ? Vector2.zero : ReadMovementInput();
        if (moveInput.x < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
        } else if (moveInput.x > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;   
        }
        if(moveInput.magnitude == 0)
        {
            animator.SetBool("Moving", false);
        } else
        {
            animator.SetBool("Moving", true);
        }
        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }

        if (moveInput.sqrMagnitude > 0.05f)
        {
            dashDirection = moveInput.normalized;
        }

        dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - Time.deltaTime);
        dashTimer = Mathf.Max(0f, dashTimer - Time.deltaTime);

        if (!movementLocked && ReadDashPressed() && dashCooldownTimer <= 0f)
        {
            dashTimer = dashDuration;
            dashCooldownTimer = dashCooldown;

            // The manager can react to dash timing during special hazards.
            if (gameManager != null)
            {
                gameManager.OnPlayerBurstDash();
            }
        }
    }

    private void FixedUpdate()
    {
        RefreshArenaFromBoundaryObjects();

        Vector2 velocity = moveInput * normalSpeed;

        if (dashTimer > 0f)
        {
            velocity += dashDirection * dashSpeed;
        }

        rb.linearVelocity = velocity;
        ClampToArena();
    }

    public void ResetTo(Vector3 position)
    {
        transform.position = position;
        rb.linearVelocity = Vector2.zero;
        dashTimer = 0f;
        dashCooldownTimer = 0f;
    }

    private void ClampToArena()
    {
        // Keep the player inside the playable water rectangle. These bounds are
        // either read from the visible boundary objects or tuned directly from
        // this component if useBoundaryObjects is disabled.
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, arenaMin.x, arenaMax.x);
        position.y = Mathf.Clamp(position.y, arenaMin.y, arenaMax.y);
        transform.position = position;
    }

    private void RefreshArenaFromBoundaryObjects()
    {
        if (!useBoundaryObjects)
        {
            return;
        }

        ResolveBoundaryReferences();

        float minX = leftWallBoundary != null ? leftWallBoundary.position.x + boundaryArenaOffsets.x : arenaMin.x;
        float maxX = rightWallBoundary != null ? rightWallBoundary.position.x + boundaryArenaOffsets.y : arenaMax.x;
        float minY = seaFloorBoundary != null ? seaFloorBoundary.position.y + boundaryArenaOffsets.z : arenaMin.y;
        float maxY = waterSurfaceBoundary != null ? waterSurfaceBoundary.position.y + boundaryArenaOffsets.w : arenaMax.y;

        if (minX < maxX && minY < maxY)
        {
            arenaMin = new Vector2(minX, minY);
            arenaMax = new Vector2(maxX, maxY);
        }
    }

    private void ResolveBoundaryReferences()
    {
        leftWallBoundary = leftWallBoundary != null ? leftWallBoundary : FindBoundaryTransform("Wall Left");
        rightWallBoundary = rightWallBoundary != null ? rightWallBoundary : FindBoundaryTransform("Wall Right");
        seaFloorBoundary = seaFloorBoundary != null ? seaFloorBoundary : FindBoundaryTransform("Sea Floor");
        waterSurfaceBoundary = waterSurfaceBoundary != null ? waterSurfaceBoundary : FindBoundaryTransform("Water Surface");
    }

    private static Transform FindBoundaryTransform(string objectName)
    {
        GameObject boundary = GameObject.Find(objectName);
        return boundary != null ? boundary.transform : null;
    }

    private static Vector2 ReadMovementInput()
    {
        // Supports both Unity's newer Input System and the old Input Manager.
        // This lets the jam project run even if teammates have different input
        // package settings after pulling the repository.
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector2.zero;
        }

        Vector2 input = Vector2.zero;
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            input.x -= 1f;
        }
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            input.x += 1f;
        }
        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            input.y -= 1f;
        }
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            input.y += 1f;
        }
        return input;
#else
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
#endif
    }

    private static bool ReadDashPressed()
    {
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        Keyboard keyboard = Keyboard.current;
        return keyboard != null && (keyboard.leftShiftKey.wasPressedThisFrame || keyboard.rightShiftKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
#endif
    }
}
