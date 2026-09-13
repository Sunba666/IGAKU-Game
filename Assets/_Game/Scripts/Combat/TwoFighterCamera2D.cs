using UnityEngine;

[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public sealed class TwoFighterCamera2D : MonoBehaviour
{
    [Header("动态角色来源")]
    [SerializeField] private BattleFighterSpawner fighterSpawner;

    [Header("可选：手动指定角色")]
    [SerializeField] private Transform player1Target;
    [SerializeField] private Transform player2Target;

    [Header("镜头跟随")]
    [SerializeField]
    private Vector2 midpointOffset =
        new Vector2(0f, 3f);

    [SerializeField, Min(0.01f)]
    private float followSmoothTime = 0.15f;

    [SerializeField, Min(0.1f)]
    private float maximumFollowSpeed = 30f;

    [Header("镜头缩放")]
    [SerializeField, Min(0.1f)]
    private float minimumOrthographicSize = 4.7f;

    [SerializeField, Min(0.1f)]
    private float maximumOrthographicSize = 5.15f;

    [SerializeField, Min(0f)]
    private float horizontalPadding = 2.4f;

    [SerializeField, Min(0f)]
    private float verticalPadding = 3f;

    [SerializeField, Min(0.01f)]
    private float zoomSmoothTime = 0.18f;

    [Header("镜头边界")]
    [SerializeField] private BoxCollider2D cameraBounds;

    [Header("初始化")]
    [SerializeField] private bool snapOnFirstFrame = true;

    private Camera controlledCamera;
    private Vector3 followVelocity;
    private float zoomVelocity;
    private bool initialPositionApplied;

    // 不直接在震动时覆盖基础镜头位置，
    // 否则震动偏移会被下一帧的跟随计算累计。
    private Vector3 baseCameraPosition;

    private float shakeRemainingTime;
    private float shakeTotalDuration;
    private float shakeStrength;

    private void Awake()
    {
        controlledCamera = GetComponent<Camera>();

        if (controlledCamera != null)
        {
            controlledCamera.orthographic = true;
        }

        baseCameraPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (controlledCamera == null)
        {
            return;
        }

        if (!ResolveTargets())
        {
            return;
        }

        if (snapOnFirstFrame && !initialPositionApplied)
        {
            ApplyCameraImmediately();
            initialPositionApplied = true;
            return;
        }

        float desiredSize =
            CalculateDesiredSize();

        float nextSize = Mathf.SmoothDamp(
            controlledCamera.orthographicSize,
            desiredSize,
            ref zoomVelocity,
            zoomSmoothTime
        );

        controlledCamera.orthographicSize =
            nextSize;

        Vector3 desiredPosition =
            CalculateDesiredPosition(nextSize);

        baseCameraPosition = Vector3.SmoothDamp(
            baseCameraPosition,
            desiredPosition,
            ref followVelocity,
            followSmoothTime,
            maximumFollowSpeed,
            Time.deltaTime
        );

        Vector3 shakeOffset =
            CalculateShakeOffset();

        transform.position =
            baseCameraPosition + shakeOffset;
    }

    public void ConfigureTargets(
        Transform newPlayer1Target,
        Transform newPlayer2Target
    )
    {
        player1Target = newPlayer1Target;
        player2Target = newPlayer2Target;

        initialPositionApplied = false;
        followVelocity = Vector3.zero;
        zoomVelocity = 0f;
        baseCameraPosition = transform.position;
    }

    public void RequestShake(
        float duration,
        float strength
    )
    {
        if (duration <= 0f || strength <= 0f)
        {
            return;
        }

        shakeRemainingTime = Mathf.Max(
            shakeRemainingTime,
            duration
        );

        shakeTotalDuration = Mathf.Max(
            shakeTotalDuration,
            duration
        );

        shakeStrength = Mathf.Max(
            shakeStrength,
            strength
        );
    }

    private Vector3 CalculateShakeOffset()
    {
        if (shakeRemainingTime <= 0f)
        {
            shakeRemainingTime = 0f;
            shakeTotalDuration = 0f;
            shakeStrength = 0f;

            return Vector3.zero;
        }

        // 使用未缩放时间。
        // 即使 Hit Stop 把 timeScale 设置为 0，
        // 镜头震动仍然可以正常播放。
        shakeRemainingTime -=
            Time.unscaledDeltaTime;

        float fade = 0f;

        if (shakeTotalDuration > 0f)
        {
            fade = Mathf.Clamp01(
                shakeRemainingTime /
                shakeTotalDuration
            );
        }

        Vector2 randomOffset =
            Random.insideUnitCircle *
            shakeStrength *
            fade;

        return new Vector3(
            randomOffset.x,
            randomOffset.y,
            0f
        );
    }

    private bool ResolveTargets()
    {
        if (fighterSpawner == null)
        {
            fighterSpawner =
                FindObjectOfType<BattleFighterSpawner>();
        }

        if (fighterSpawner != null)
        {
            GameObject player1Instance =
                fighterSpawner.Player1Instance;

            GameObject player2Instance =
                fighterSpawner.Player2Instance;

            if (player1Instance != null)
            {
                player1Target =
                    player1Instance.transform;
            }

            if (player2Instance != null)
            {
                player2Target =
                    player2Instance.transform;
            }
        }

        return player1Target != null &&
               player2Target != null;
    }

    private float CalculateDesiredSize()
    {
        Vector3 player1Position =
            player1Target.position;

        Vector3 player2Position =
            player2Target.position;

        float horizontalDistance = Mathf.Abs(
            player1Position.x -
            player2Position.x
        );

        float verticalDistance = Mathf.Abs(
            player1Position.y -
            player2Position.y
        );

        float safeAspect = Mathf.Max(
            controlledCamera.aspect,
            0.01f
        );

        float sizeForHorizontalDistance =
            (horizontalDistance * 0.5f +
             horizontalPadding) /
            safeAspect;

        float sizeForVerticalDistance =
            verticalDistance * 0.5f +
            verticalPadding;

        float desiredSize = Mathf.Max(
            minimumOrthographicSize,
            sizeForHorizontalDistance,
            sizeForVerticalDistance
        );

        return Mathf.Clamp(
            desiredSize,
            minimumOrthographicSize,
            maximumOrthographicSize
        );
    }

    private Vector3 CalculateDesiredPosition(
        float orthographicSize
    )
    {
        Vector3 midpoint =
            (player1Target.position +
             player2Target.position) *
            0.5f;

        Vector3 desiredPosition =
            midpoint +
            new Vector3(
                midpointOffset.x,
                midpointOffset.y,
                0f
            );

        desiredPosition.z =
            baseCameraPosition.z;

        return ClampPositionToBounds(
            desiredPosition,
            orthographicSize
        );
    }

    private Vector3 ClampPositionToBounds(
        Vector3 desiredPosition,
        float orthographicSize
    )
    {
        if (cameraBounds == null)
        {
            return desiredPosition;
        }

        Bounds bounds =
            cameraBounds.bounds;

        float cameraHalfHeight =
            orthographicSize;

        float cameraHalfWidth =
            orthographicSize *
            controlledCamera.aspect;

        float minimumX =
            bounds.min.x + cameraHalfWidth;

        float maximumX =
            bounds.max.x - cameraHalfWidth;

        float minimumY =
            bounds.min.y + cameraHalfHeight;

        float maximumY =
            bounds.max.y - cameraHalfHeight;

        if (minimumX <= maximumX)
        {
            desiredPosition.x = Mathf.Clamp(
                desiredPosition.x,
                minimumX,
                maximumX
            );
        }
        else
        {
            desiredPosition.x =
                bounds.center.x;
        }

        if (minimumY <= maximumY)
        {
            desiredPosition.y = Mathf.Clamp(
                desiredPosition.y,
                minimumY,
                maximumY
            );
        }
        else
        {
            desiredPosition.y =
                bounds.center.y;
        }

        return desiredPosition;
    }

    private void ApplyCameraImmediately()
    {
        float desiredSize =
            CalculateDesiredSize();

        controlledCamera.orthographicSize =
            desiredSize;

        baseCameraPosition =
            CalculateDesiredPosition(desiredSize);

        transform.position =
            baseCameraPosition;

        followVelocity = Vector3.zero;
        zoomVelocity = 0f;
    }

    private void OnValidate()
    {
        minimumOrthographicSize = Mathf.Max(
            0.1f,
            minimumOrthographicSize
        );

        maximumOrthographicSize = Mathf.Max(
            minimumOrthographicSize,
            maximumOrthographicSize
        );

        followSmoothTime = Mathf.Max(
            0.01f,
            followSmoothTime
        );

        zoomSmoothTime = Mathf.Max(
            0.01f,
            zoomSmoothTime
        );
    }
}