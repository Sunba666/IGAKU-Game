using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public sealed class CharacterSelectReadyMotion : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float duration = 0.20f;
    [SerializeField, Range(0.1f, 1f)] private float startScale = 0.72f;
    [SerializeField, Range(1f, 1.4f)] private float overshootScale = 1.10f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector3 restingScale;
    private Coroutine motionRoutine;

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        canvasGroup = GetComponent<CanvasGroup>();
        restingScale = rectTransform != null ? rectTransform.localScale : Vector3.one;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private void OnEnable()
    {
        if (rectTransform == null || canvasGroup == null)
            return;

        restingScale = rectTransform.localScale;

        if (motionRoutine != null)
            StopCoroutine(motionRoutine);

        motionRoutine = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        float elapsed = 0f;
        Vector3 smallScale = restingScale * startScale;
        Vector3 largeScale = restingScale * overshootScale;

        rectTransform.localScale = smallScale;
        canvasGroup.alpha = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = t;

            if (t < 0.7f)
            {
                float firstPart = t / 0.7f;
                rectTransform.localScale = Vector3.LerpUnclamped(
                    smallScale,
                    largeScale,
                    firstPart);
            }
            else
            {
                float secondPart = (t - 0.7f) / 0.3f;
                rectTransform.localScale = Vector3.LerpUnclamped(
                    largeScale,
                    restingScale,
                    secondPart);
            }

            yield return null;
        }

        rectTransform.localScale = restingScale;
        canvasGroup.alpha = 1f;
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
            rectTransform.localScale = restingScale;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }
}
