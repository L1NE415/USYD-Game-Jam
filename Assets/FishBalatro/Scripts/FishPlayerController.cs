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
    public FishGameManager gameManager;
    public float normalSpeed = 5.6f;
    public float dashSpeed = 10.5f;
    public float dashDuration = 0.12f;
    public float dashCooldown = 0.65f;
    public Vector2 arenaMin = new Vector2(-8f, -4.2f);
    public Vector2 arenaMax = new Vector2(8f, 3.55f);

    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 moveInput;
    private Vector2 dashDirection = Vector2.right;
    private float dashTimer;
    private float dashCooldownTimer;

    public Vector2 MoveInput => moveInput;
    public bool IsDashing => dashTimer > 0f;

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
        // intentionally simple so the level can be tuned from the Inspector.
        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, arenaMin.x, arenaMax.x);
        position.y = Mathf.Clamp(position.y, arenaMin.y, arenaMax.y);
        transform.position = position;
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
