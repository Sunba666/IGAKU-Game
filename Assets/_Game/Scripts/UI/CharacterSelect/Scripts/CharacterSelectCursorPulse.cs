using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public sealed class CharacterSelectCursorPulse : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float pulseSpeed = 2.4f;
    [SerializeField, Range(0f, 0.2f)] private float scaleAmplitude = 0.035f;
    [SerializeField, Range(0f, 1f)] private float minimumAlpha = 0.72f;
    [SerializeField, Range(0f, 1f)] private float maximumAlpha = 1f;
    [SerializeField, Range(0f, 1f)] private float phaseOffset;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 baseScale;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        canvasGroup = GetComponent<CanvasGroup>();
        baseScale = rectTransform != null ? rectTransform.localScale : Vector3.one;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        if (rectTransform != null)
            baseScale = rectTransform.localScale;
    }

    private void Update()
    {
        float wave = (Mathf.Sin(
            (Time.unscaledTime * pulseSpeed + phaseOffset) * Mathf.PI * 2f) + 1f) * 0.5f;

        if (rectTransform != null)
            rectTransform.localScale = baseScale * (1f + wave * scaleAmplitude);

        if (canvasGroup != null)
            canvasGroup.alpha = Mathf.Lerp(minimumAlpha, maximumAlpha, wave);
    }

    private void OnDisable()
    {
        if (rectTransform != null)
            rectTransform.localScale = baseScale;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }
}

