using UnityEngine;

[DisallowMultipleComponent]
public sealed class BattleParallaxLayer : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Parallax")]
    [Tooltip("0 = 固定在世界中；1 = 完全跟随镜头。远景通常比中景更接近 1。")]
    [SerializeField] private Vector2 cameraFollow = new Vector2(0.9f, 0.9f);

    [Header("Optional Idle Drift")]
    [SerializeField] private Vector2 idleAmplitude = Vector2.zero;
    [SerializeField] private Vector2 idleFrequency = new Vector2(0.05f, 0.035f);
    [SerializeField] private float phaseOffset;

    [Header("Pixel Stability")]
    [SerializeField] private bool snapToPixelGrid = true;
    [SerializeField, Min(1f)] private float pixelsPerUnit = 100f;

    private Vector3 layerStartPosition;
    private Vector3 cameraStartPosition;
    private bool originCaptured;

    private void OnEnable()
    {
        ResolveCamera();
        CaptureOrigin();
    }

    private void LateUpdate()
    {
        ResolveCamera();

        if (targetCamera == null)
            return;

        if (!originCaptured)
            CaptureOrigin();

        Vector3 cameraDelta = targetCamera.transform.position - cameraStartPosition;
        float time = Time.unscaledTime + phaseOffset;

        Vector2 idleOffset = new Vector2(
            Mathf.Sin(time * idleFrequency.x * Mathf.PI * 2f) * idleAmplitude.x,
            Mathf.Sin(time * idleFrequency.y * Mathf.PI * 2f) * idleAmplitude.y);

        Vector3 targetPosition = layerStartPosition + new Vector3(
            cameraDelta.x * cameraFollow.x + idleOffset.x,
            cameraDelta.y * cameraFollow.y + idleOffset.y,
            0f);

        if (snapToPixelGrid)
        {
            targetPosition.x = Mathf.Round(targetPosition.x * pixelsPerUnit) / pixelsPerUnit;
            targetPosition.y = Mathf.Round(targetPosition.y * pixelsPerUnit) / pixelsPerUnit;
        }

        transform.position = targetPosition;
    }

    [ContextMenu("Capture Current Origin")]
    public void CaptureOrigin()
    {
        ResolveCamera();
        if (targetCamera == null)
        {
            originCaptured = false;
            return;
        }

        layerStartPosition = transform.position;
        cameraStartPosition = targetCamera.transform.position;
        originCaptured = true;
    }

    private void ResolveCamera()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }
}
