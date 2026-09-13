using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public sealed class BattleEnergyPulse : MonoBehaviour
{
    private static readonly int IntensityId = Shader.PropertyToID("_Intensity");

    [SerializeField, Min(0.01f)] private float pulseSpeed = 1.25f;
    [SerializeField, Range(0f, 1f)] private float minimumAlpha = 0.48f;
    [SerializeField, Range(0f, 1f)] private float maximumAlpha = 0.92f;
    [SerializeField, Min(0f)] private float minimumIntensity = 0.75f;
    [SerializeField, Min(0f)] private float maximumIntensity = 1.35f;
    [SerializeField, Range(0f, 0.3f)] private float flickerStrength = 0.06f;
    [SerializeField] private float phaseOffset;

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;
    private Color baseColor;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock();
        baseColor = spriteRenderer.color;
    }

    private void Update()
    {
        float time = Time.unscaledTime;
        float wave = (Mathf.Sin(
            (time * pulseSpeed + phaseOffset) * Mathf.PI * 2f) + 1f) * 0.5f;
        float flicker = (Mathf.PerlinNoise(time * 8.3f, phaseOffset + 0.37f) - 0.5f)
                        * 2f * flickerStrength;
        float value = Mathf.Clamp01(wave + flicker);

        Color color = baseColor;
        color.a = Mathf.Lerp(minimumAlpha, maximumAlpha, value);
        spriteRenderer.color = color;

        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(
            IntensityId,
            Mathf.Lerp(minimumIntensity, maximumIntensity, value));
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }

    private void OnDisable()
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = baseColor;
        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(IntensityId, 1f);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }
}
