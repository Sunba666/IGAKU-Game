using UnityEngine;

public class LocalFighterController2D : MonoBehaviour
{
    public enum LocalPlayerSlot
    {
        Player1,
        Player2
    }

    [Header("玩家")]
    [SerializeField]
    private LocalPlayerSlot playerSlot =
        LocalPlayerSlot.Player1;

    [Header("对象引用")]
    [SerializeField]
    private Transform visualRoot;

    [SerializeField]
    private Animator animator;

    [SerializeField]
    private Transform groundCheck;

    [Header("移动")]
    [SerializeField]
    private float moveSpeed = 6f;

    [SerializeField]
    private float jumpSpeed = 11f;

    [Header("地面检测")]
    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private float groundCheckRadius = 0.12f;

    [Header("攻击")]

    [SerializeField]
    private FighterCombat2D combat;

    [Header("自动面向对手")]
    [SerializeField]
    private bool autoFaceOpponent = true;

    [SerializeField]
    private bool lockFacingWhileAttacking = true;

    [SerializeField, Min(0f)]
    private float facingDeadZone = 0.05f;

    [Header("初始朝向")]
    [SerializeField]
    private bool startFacingRight = true;

    private Rigidbody2D body;
    private Transform opponentTarget;

    private float horizontalInput;

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

    public bool FacingRight =>
        facingRight;

    public Transform OpponentTarget =>
        opponentTarget;

    private void Awake()
    {
        body =
            GetComponent<Rigidbody2D>();

        if (combat == null)
        {
            combat =
                GetComponent<FighterCombat2D>();
        }

        if (animator == null)
        {
            animator =
                GetComponentInChildren<Animator>();
        }

        facingRight =
            startFacingRight;

        ApplyFacingDirection();
    }

    private void Update()
    {
        ReadInput();
        CheckGround();
        UpdateFacingDirection();
        UpdateAnimator();

    }

    private void FixedUpdate()
    {
        MoveCharacter();
    }

    private void ReadInput()
    {
        horizontalInput = 0f;

        if (playerSlot ==
            LocalPlayerSlot.Player1)
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
            if (Input.GetKey(
                KeyCode.LeftArrow
            ))
            {
                horizontalInput -= 1f;
            }

            if (Input.GetKey(
                KeyCode.RightArrow
            ))
            {
                horizontalInput += 1f;
            }

            if (Input.GetKeyDown(
                KeyCode.UpArrow
            ))
            {
                TryJump();
            }

            if (Input.GetKeyDown(
                KeyCode.Keypad1
            ))
            {
                TryAttackLight();
            }
        }

        if (combat != null && combat.IsAttacking)
        {
            horizontalInput = 0f;
        }
    }
    private void MoveCharacter()
    {
        if (body == null)
        {
            return;
        }

        Vector2 currentVelocity =
            GetVelocity();

        currentVelocity.x =
            horizontalInput *
            moveSpeed;

        SetVelocity(currentVelocity);
    }

    private void TryJump()
    {
        if (!isGrounded ||
            body == null)
        {
            return;
        }

        Vector2 currentVelocity =
            GetVelocity();

        currentVelocity.y =
            jumpSpeed;

        SetVelocity(currentVelocity);
    }

    private void TryAttackLight()
    {
        if (animator == null ||
            combat == null)
        {
            return;
        }

        if (combat.IsAttacking)
        {
            return;
        }

        bool attackStarted =
            combat.BeginLightAttack();

        if (!attackStarted)
        {
            return;
        }

        animator.ResetTrigger(
            AttackLightHash
        );

        animator.SetTrigger(
            AttackLightHash
        );
    }
    private void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded =
            Physics2D.OverlapCircle(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            ) != null;
    }

    private void UpdateAnimator()
    {
        if (animator == null ||
            body == null)
        {
            return;
        }

        Vector2 currentVelocity =
            GetVelocity();

        animator.SetFloat(
            SpeedHash,
            Mathf.Abs(currentVelocity.x)
        );

        animator.SetFloat(
            VerticalSpeedHash,
            currentVelocity.y
        );

        animator.SetBool(
            GroundedHash,
            isGrounded
        );
    }

    private void UpdateFacingDirection()
    {
        if (autoFaceOpponent &&
            opponentTarget != null)
        {
            if (lockFacingWhileAttacking &&
                combat != null &&
                combat.IsAttacking)
            {
                return;
            }

            float opponentDirection =
                opponentTarget.position.x -
                transform.position.x;

            if (Mathf.Abs(opponentDirection) <=
                facingDeadZone)
            {
                return;
            }

            SetFacingDirection(
                opponentDirection > 0f
            );

            return;
        }

        // 没有连接对手时使用原来的移动朝向，
        // 方便单独测试角色预制体。
        if (horizontalInput > 0.01f)
        {
            SetFacingDirection(true);
        }
        else if (horizontalInput < -0.01f)
        {
            SetFacingDirection(false);
        }
    }

    private void SetFacingDirection(
        bool shouldFaceRight
    )
    {
        if (facingRight ==
            shouldFaceRight)
        {
            return;
        }

        facingRight =
            shouldFaceRight;

        ApplyFacingDirection();
    }

    private void ApplyFacingDirection()
    {
        if (visualRoot == null)
        {
            return;
        }

        Vector3 visualScale =
            visualRoot.localScale;

        // 只改变 X 的正负，不改变角色原本尺寸。
        float absoluteScaleX =
            Mathf.Abs(visualScale.x);

        visualScale.x =
            absoluteScaleX *
            (facingRight ? 1f : -1f);

        visualRoot.localScale =
            visualScale;
    }

    private Vector2 GetVelocity()
    {
#if UNITY_6000_0_OR_NEWER
        return body.linearVelocity;
#else
        return body.velocity;
#endif
    }

    private void SetVelocity(
        Vector2 value
    )
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
        startFacingRight =
            shouldFaceRight;

        ResetForRound(
            shouldFaceRight
        );
    }

    public void ConfigureOpponent(
        Transform newOpponent
    )
    {
        opponentTarget =
            newOpponent;

        UpdateFacingDirection();
    }

    public void ResetForRound(
        bool shouldFaceRight
    )
    {
        if (combat != null)
        {
            combat.CancelCurrentAttack();
        }

        horizontalInput = 0f;
        facingRight =
            shouldFaceRight;

        ApplyFacingDirection();

        if (body != null)
        {
#if UNITY_6000_0_OR_NEWER
            body.linearVelocity =
                Vector2.zero;
#else
            body.velocity =
                Vector2.zero;
#endif

            body.angularVelocity = 0f;
        }

        if (animator != null)
        {
            animator.ResetTrigger(
                AttackLightHash
            );

            animator.SetFloat(
                SpeedHash,
                0f
            );

            animator.SetFloat(
                VerticalSpeedHash,
                0f
            );
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null)
        {
            return;
        }

        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            groundCheck.position,
            groundCheckRadius
        );
    }

    private void OnValidate()
    {
        facingDeadZone =
            Mathf.Max(
                0f,
                facingDeadZone
            );
    }
}