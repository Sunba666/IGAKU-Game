using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public sealed class CharacterSelectPreviewMotion : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float duration = 0.18f;
    [SerializeField] private float horizontalOffset = 28f;
    [SerializeField, Range(0.5f, 1f)] private float startScale = 0.965f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 restingPosition;
    private Vector3 restingScale;
    private Coroutine motionRoutine;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        canvasGroup = GetComponent<CanvasGroup>();

        if (rectTransform != null)
        {
            restingPosition = rectTransform.anchoredPosition;
            restingScale = rectTransform.localScale;
        }

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public void Play()
    {
        if (!isActiveAndEnabled || rectTransform == null)
            return;

        if (motionRoutine != null)
            StopCoroutine(motionRoutine);

        motionRoutine = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        float elapsed = 0f;
        Vector2 startPosition = restingPosition + Vector2.right * horizontalOffset;
        Vector3 initialScale = restingScale * startScale;

        canvasGroup.alpha = 0f;
        rectTransform.anchoredPosition = startPosition;
        rectTransform.localScale = initialScale;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            canvasGroup.alpha = eased;
            rectTransform.anchoredPosition = Vector2.LerpUnclamped(
                startPosition,
                restingPosition,
                eased);
            rectTransform.localScale = Vector3.LerpUnclamped(
                initialScale,
                restingScale,
                eased);

            yield return null;
        }

        canvasGroup.alpha = 1f;
        rectTransform.anchoredPosition = restingPosition;
        rectTransform.localScale = restingScale;
        motionRoutine = null;
    }

    private void OnDisable()
    {
        if (motionRoutine != null)
        {
            StopCoroutine(motionRoutine);
            motionRoutine = null;
        }

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = restingPosition;
            rectTransform.localScale = restingScale;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }
}
