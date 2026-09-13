using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("移动参数")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float jumpForce = 12f;

    [Header("地面检测")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;

    private float moveInput;
    private bool jumpRequested;
    private bool isGrounded;
    private bool isCrouching;

    // 初始化组件
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // 每一帧读取输入
    private void Update()
    {
        ReadInput();
        CheckGround();


    }

    // 固定时间处理物理
    private void FixedUpdate()
    {
        Move();
        Jump();
    }

    // 读取键盘输入
    private void ReadInput()
    {
        bool pressA = Input.GetKey(KeyCode.A);
        bool pressD = Input.GetKey(KeyCode.D);

        // A、D同时按下，或者都没按：静止
        if (pressA == pressD)
        {
            moveInput = 0f;
        }
        else if (pressA)
        {
            moveInput = -1f;
        }
        else
        {
            moveInput = 1f;
        }

        // W：跳跃
        if (Input.GetKeyDown(KeyCode.W) && isGrounded && !isCrouching)
        {
            jumpRequested = true;
        }

        // S：下蹲
        isCrouching = Input.GetKey(KeyCode.S);
    }
    // 左右移动
    private void Move()
    {
        // 下蹲时禁止移动
        if (isCrouching)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
            return;
        }

        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
    }

    // 跳跃
    private void Jump()
    {
        if (!jumpRequested)
            return;

        rb.velocity = new Vector2(rb.velocity.x, jumpForce);

        jumpRequested = false;
    }

    // 检测是否站在地面
    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    // 提供给其他脚本使用
    public bool IsGrounded => isGrounded;
    public bool IsCrouching => isCrouching;
    public float MoveInput => moveInput;
}