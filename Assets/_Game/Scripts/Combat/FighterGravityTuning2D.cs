using UnityEngine;

[DefaultExecutionOrder(50)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public sealed class FighterGravityTuning2D :
    MonoBehaviour
{
    [Header("下降手感")]
    [SerializeField, Min(1f)]
    private float fallGravityMultiplier = 1.65f;

    [SerializeField, Min(1f)]
    private float maximumFallSpeed = 25f;

    [SerializeField]
    private float fallStartThreshold = -0.05f;

    private Rigidbody2D body;

    private void Awake()
    {
        body =
            GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (body == null ||
            !body.simulated)
        {
            return;
        }

        Vector2 currentVelocity =
            GetVelocity();

        // 上升阶段使用 Rigidbody2D 原始重力。
        // 只有确认开始下降后，才追加额外重力。
        if (currentVelocity.y >=
            fallStartThreshold)
        {
            return;
        }

        float extraGravity =
            Physics2D.gravity.y *
            body.gravityScale *
            (fallGravityMultiplier - 1f);

        currentVelocity.y +=
            extraGravity *
            Time.fixedDeltaTime;

        currentVelocity.y =
            Mathf.Max(
                currentVelocity.y,
                -maximumFallSpeed
            );

        SetVelocity(currentVelocity);
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

    private void OnValidate()
    {
        fallGravityMultiplier =
            Mathf.Max(
                1f,
                fallGravityMultiplier
            );

        maximumFallSpeed =
            Mathf.Max(
                1f,
                maximumFallSpeed
            );
    }
}
