using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public sealed class CharacterSelectTransitionOverlay : MonoBehaviour
{
    [SerializeField, Min(0f)] private float readyHoldDuration = 0.25f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 0.35f;
    [SerializeField, Min(0f)] private float blackHoldDuration = 0.10f;
    [SerializeField] private UnityEvent onTransitionFinished;

    private CanvasGroup canvasGroup;
    private Coroutine transitionRoutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        ResetOverlay();
    }

    public void Play()
    {
        if (!isActiveAndEnabled || transitionRoutine != null)
            return;

        transitionRoutine = StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        canvasGroup.blocksRaycasts = true;

        if (readyHoldDuration > 0f)
            yield return WaitUnscaled(readyHoldDuration);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            canvasGroup.alpha = t * t * (3f - 2f * t);
            yield return null;
        }

        canvasGroup.alpha = 1f;

        if (blackHoldDuration > 0f)
            yield return WaitUnscaled(blackHoldDuration);

        transitionRoutine = null;
        onTransitionFinished?.Invoke();
    }

    public void ResetOverlay()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private static IEnumerator WaitUnscaled(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}
