using UnityEngine;

[DisallowMultipleComponent]
public sealed class MainMenuIdleMotion : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    [SerializeField] private Vector2 positionAmplitude = new Vector2(0f, 3f);
    [SerializeField, Min(0.1f)] private float cycleSeconds = 3.4f;
    [SerializeField, Range(0f, 1f)] private float phase = 0f;
    [SerializeField, Range(0f, 0.08f)] private float scaleAmplitude = 0f;

    private Vector2 basePosition;
    private Vector3 baseScale;

    private void Awake()
    {
        if (target == null)
            target = transform as RectTransform;
    }

    private void OnEnable()
    {
        if (target == null)
            return;

        basePosition = target.anchoredPosition;
        baseScale = target.localScale;
    }

    private void Update()
    {
        if (target == null)
            return;

        float angle = (Time.unscaledTime / cycleSeconds + phase) * Mathf.PI * 2f;
        float wave = Mathf.Sin(angle);
        float secondary = Mathf.Sin(angle * 0.73f + 1.2f);

        target.anchoredPosition = basePosition + new Vector2(
            positionAmplitude.x * secondary,
            positionAmplitude.y * wave);

        float scale = 1f + scaleAmplitude * (wave * 0.5f + 0.5f);
        target.localScale = baseScale * scale;
    }

    private void OnDisable()
    {
        if (target == null)
            return;

        target.anchoredPosition = basePosition;
        target.localScale = baseScale;
    }
}

