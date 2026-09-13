using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class MainMenuSelectableMotion : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    ISelectHandler,
    IDeselectHandler
{
    [SerializeField] private RectTransform target;
    [SerializeField] private Graphic graphic;
    [SerializeField] private CanvasGroup hoverBar;
    [SerializeField] private Color normalColor = new Color32(187, 198, 199, 184);
    [SerializeField] private Color selectedColor = new Color32(255, 99, 168, 255);
    [SerializeField, Range(1f, 1.15f)] private float selectedScale = 1.035f;
    [SerializeField, Min(1f)] private float response = 14f;

    private bool selected;
    private Vector3 baseScale;

    private void Awake()
    {
        if (target == null)
            target = transform as RectTransform;

        if (graphic == null)
            graphic = GetComponent<Graphic>();

        baseScale = target != null ? target.localScale : Vector3.one;
        ApplyImmediate();
    }

    private void Update()
    {
        float t = 1f - Mathf.Exp(-response * Time.unscaledDeltaTime);

        if (target != null)
        {
            Vector3 wantedScale = baseScale * (selected ? selectedScale : 1f);
            target.localScale = Vector3.Lerp(target.localScale, wantedScale, t);
        }

        if (graphic != null)
        {
            Color wantedColor = selected ? selectedColor : normalColor;
            graphic.color = Color.Lerp(graphic.color, wantedColor, t);
        }

        if (hoverBar != null)
        {
            float wantedAlpha = selected ? 1f : 0f;
            hoverBar.alpha = Mathf.Lerp(hoverBar.alpha, wantedAlpha, t);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        selected = true;

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        selected = false;
    }

    public void OnSelect(BaseEventData eventData)
    {
        selected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        selected = false;
    }

    public void SetSelected(bool value)
    {
        selected = value;
    }

    private void ApplyImmediate()
    {
        if (target != null)
            target.localScale = baseScale * (selected ? selectedScale : 1f);

        if (graphic != null)
            graphic.color = selected ? selectedColor : normalColor;

        if (hoverBar != null)
            hoverBar.alpha = selected ? 1f : 0f;
    }
}
