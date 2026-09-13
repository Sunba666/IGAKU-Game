using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(CanvasGroup))]
public sealed class MainMenuPulseAlpha : MonoBehaviour
{
    [SerializeField, Range(0f, 1f)] private float minimumAlpha = 0.25f;
    [SerializeField, Range(0f, 1f)] private float maximumAlpha = 0.85f;
    [SerializeField, Min(0.1f)] private float cycleSeconds = 1.4f;
    [SerializeField, Range(0f, 1f)] private float phase;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        float angle = (Time.unscaledTime / cycleSeconds + phase) * Mathf.PI * 2f;
        float wave01 = Mathf.Sin(angle) * 0.5f + 0.5f;
        canvasGroup.alpha = Mathf.Lerp(minimumAlpha, maximumAlpha, wave01);
    }
}
