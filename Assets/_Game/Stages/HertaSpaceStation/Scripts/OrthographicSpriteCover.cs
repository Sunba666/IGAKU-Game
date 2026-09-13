using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class OrthographicSpriteCover : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField, Min(1f)] private float overscan = 1.12f;

    private SpriteRenderer spriteRenderer;

    private void OnEnable()
    {
        FitToCamera();
    }

    private void OnValidate()
    {
        FitToCamera();
    }

    [ContextMenu("Fit Sprite To Camera")]
    public void FitToCamera()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null || !targetCamera.orthographic ||
            spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
            return;

        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;
        float scale = Mathf.Max(
            cameraWidth / spriteSize.x,
            cameraHeight / spriteSize.y) * overscan;

        float signX = transform.localScale.x < 0f ? -1f : 1f;
        float signY = transform.localScale.y < 0f ? -1f : 1f;

        transform.localScale = new Vector3(
            signX * scale,
            signY * scale,
            1f);
    }
}
