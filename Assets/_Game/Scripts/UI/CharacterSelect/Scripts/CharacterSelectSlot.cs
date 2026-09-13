using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class CharacterSelectSlot : MonoBehaviour
{
    [Header("Character")]
    [SerializeField] private CharacterSelectCharacterData characterData;

    [Header("Required Visual References")]
    [SerializeField] private Image portraitImage;
    [SerializeField] private GameObject lockedOverlay;

    [Header("Optional Visual References")]
    [SerializeField] private TMP_Text lockedLabel;

    [Header("Locked Appearance")]
    [SerializeField, Range(0f, 1f)] private float lockedPortraitBrightness = 0.18f;

    public CharacterSelectCharacterData CharacterData => characterData;
    public bool IsSelectable => characterData != null && characterData.Implemented;
    public RectTransform RectTransform => (RectTransform)transform;

    private void Awake()
    {
        RefreshVisual();
    }

    private void OnValidate()
    {
        RefreshVisual();
    }

    public void RefreshVisual()
    {
        if (portraitImage != null)
        {
            Sprite portrait = characterData != null ? characterData.Portrait : null;
            portraitImage.sprite = portrait;
            portraitImage.enabled = portrait != null;
            portraitImage.preserveAspect = true;

            float brightness = IsSelectable ? 1f : lockedPortraitBrightness;
            portraitImage.color = new Color(brightness, brightness, brightness, 1f);
            portraitImage.raycastTarget = false;
        }

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!IsSelectable);

        if (lockedLabel != null)
        {
            lockedLabel.raycastTarget = false;
        }
    }
}
