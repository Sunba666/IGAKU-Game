using UnityEngine;

public class LocalFighterController2D : MonoBehaviour
{
    public enum LocalPlayerSlot
    {
        Player1,
        Player2
    }

    [Header("玩家")]
    [SerializeField] private LocalPlayerSlot playerSlot = LocalPlayerSlot.Player1;

    [Header("对象引用")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform groundCheck;

    [Header("移动")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpSpeed = 11f;

    [Header("地面检测")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.12f;

    [Header("攻击")]
    [SerializeField] private float attackInputLockTime = 0.35f;
    [SerializeField] private FighterCombat2D combat;

    [Header("初始朝向")]
    [SerializeField] private bool startFacingRight = true;

    private Rigidbody2D body;
    private float horizontalInput;
    private float attackLockTimer;
    private bool isGrounded;
    private bool facingRight;

    private static readonly int SpeedHash =
        Animator.StringToHash("Speed");

    private static readonly int VerticalSpeedHash =
        Animator.StringToHash("VerticalSpeed");

    private static readonly int GroundedHash =
        Animator.StringToHash("Grounded");

    private static readonly int AttackLightHash =
        Animator.StringToHash("AttackLight");

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        if (combat == null)
        {
            combat = GetComponent<FighterCombat2D>();
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        facingRight = startFacingRight;
        ApplyFacingDirection();
    }

    private void Update()
    {
        ReadInput();
        CheckGround();
        UpdateAnimator();

        if (attackLockTimer > 0f)
        {
            attackLockTimer -= Time.deltaTime;
        }
    }

    private void FixedUpdate()
    {
        MoveCharacter();
    }

    private void ReadInput()
    {
        horizontalInput = 0f;

        if (playerSlot == LocalPlayerSlot.Player1)
        {
            if (Input.GetKey(KeyCode.A))
            {
                horizontalInput -= 1f;
            }

            if (Input.GetKey(KeyCode.D))
            {
                horizontalInput += 1f;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                TryJump();
            }

            if (Input.GetKeyDown(KeyCode.J))
            {
                TryAttackLight();
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                horizontalInput -= 1f;
            }

            if (Input.GetKey(KeyCode.RightArrow))
            {
                horizontalInput += 1f;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                TryJump();
            }

            if (Input.GetKeyDown(KeyCode.Keypad1))
            {
                TryAttackLight();
            }
        }

        if (attackLockTimer > 0f)
        {
            horizontalInput = 0f;
        }

        UpdateFacingDirection();
    }

    private void MoveCharacter()
    {
        if (body == null)
        {
            return;
        }

        Vector2 currentVelocity = GetVelocity();
        currentVelocity.x = horizontalInput * moveSpeed;
        SetVelocity(currentVelocity);
    }

    private void TryJump()
    {
        if (!isGrounded || body == null)
        {
            return;
        }

        Vector2 currentVelocity = GetVelocity();
        currentVelocity.y = jumpSpeed;
        SetVelocity(currentVelocity);
    }

    private void TryAttackLight()
    {
        if (animator == null || attackLockTimer > 0f)
        {
            return;
        }

        animator.ResetTrigger(AttackLightHash);
        animator.SetTrigger(AttackLightHash);

        if (combat != null)
        {
            combat.BeginLightAttack();
        }

        attackLockTimer = attackInputLockTime;
    }

    private void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        ) != null;
    }

    private void UpdateAnimator()
    {
        if (animator == null || body == null)
        {
            return;
        }

        Vector2 currentVelocity = GetVelocity();

        animator.SetFloat(SpeedHash, Mathf.Abs(currentVelocity.x));
        animator.SetFloat(VerticalSpeedHash, currentVelocity.y);
        animator.SetBool(GroundedHash, isGrounded);
    }

    private void UpdateFacingDirection()
    {
        if (horizontalInput > 0.01f && !facingRight)
        {
            facingRight = true;
            ApplyFacingDirection();
        }
        else if (horizontalInput < -0.01f && facingRight)
        {
            facingRight = false;
            ApplyFacingDirection();
        }
    }

    private void ApplyFacingDirection()
    {
        if (visualRoot == null)
        {
            return;
        }

        Vector3 visualScale = visualRoot.localScale;
        visualScale.x = Mathf.Abs(visualScale.x) *
                        (facingRight ? 1f : -1f);

        visualRoot.localScale = visualScale;
    }

    private Vector2 GetVelocity()
    {
#if UNITY_6000_0_OR_NEWER
        return body.linearVelocity;
#else
        return body.velocity;
#endif
    }

    private void SetVelocity(Vector2 value)
    {
#if UNITY_6000_0_OR_NEWER
        body.linearVelocity = value;
#else
        body.velocity = value;
#endif
    }
    public void ConfigurePlayer(
    LocalPlayerSlot slot,
    bool shouldFaceRight
)
    {
        playerSlot = slot;
        startFacingRight = shouldFaceRight;

        ResetForRound(shouldFaceRight);
    }
    public void ResetForRound(bool shouldFaceRight)
    {
        horizontalInput = 0f;
        attackLockTimer = 0f;
        facingRight = shouldFaceRight;

        ApplyFacingDirection();

        if (body != null)
        {
#if UNITY_6000_0_OR_NEWER
        body.linearVelocity = Vector2.zero;
#else
            body.velocity = Vector2.zero;
#endif

            body.angularVelocity = 0f;
        }

        if (animator != null)
        {
            animator.ResetTrigger(AttackLightHash);
            animator.SetFloat(SpeedHash, 0f);
            animator.SetFloat(VerticalSpeedHash, 0f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }
}