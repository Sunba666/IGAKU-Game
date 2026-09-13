using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class HitSpark2D : MonoBehaviour
{
    private sealed class SparkPiece
    {
        public Transform PieceTransform;
        public SpriteRenderer Renderer;
        public Vector3 TargetScale;
        public Color StartColor;
        public float RotationSpeed;
        public bool IsCore;
    }

    private static Sprite whiteSprite;

    private readonly List<SparkPiece> pieces =
        new List<SparkPiece>();

    private float lifetime;
    private float elapsedTime;

    public static void Spawn(
        Vector3 worldPosition,
        Color sparkColor,
        bool isKO
    )
    {
        GameObject effectObject =
            new GameObject(
                isKO
                    ? "HitSpark_KO"
                    : "HitSpark"
            );

        effectObject.transform.position =
            new Vector3(
                worldPosition.x,
                worldPosition.y,
                0f
            );

        HitSpark2D effect =
            effectObject.AddComponent<HitSpark2D>();

        effect.BuildEffect(
            sparkColor,
            isKO
        );
    }

    private void BuildEffect(
        Color sparkColor,
        bool isKO
    )
    {
        lifetime = isKO ? 0.26f : 0.16f;

        int rayCount = isKO ? 14 : 8;

        float coreSize =
            isKO ? 0.48f : 0.30f;

        CreateCore(
            Color.white,
            coreSize
        );

        for (int index = 0;
             index < rayCount;
             index++)
        {
            float baseAngle =
                360f / rayCount * index;

            float randomAngle =
                Random.Range(-12f, 12f);

            float angle =
                baseAngle + randomAngle;

            float length = isKO
                ? Random.Range(0.75f, 1.45f)
                : Random.Range(0.38f, 0.85f);

            float thickness = isKO
                ? Random.Range(0.055f, 0.11f)
                : Random.Range(0.035f, 0.075f);

            Color rayColor =
                index % 3 == 0
                    ? Color.white
                    : sparkColor;

            CreateRay(
                rayColor,
                angle,
                length,
                thickness
            );
        }
    }

    private void CreateCore(
        Color color,
        float size
    )
    {
        GameObject coreObject =
            new GameObject("Core");

        coreObject.transform.SetParent(
            transform,
            false
        );

        coreObject.transform.localRotation =
            Quaternion.Euler(0f, 0f, 45f);

        SpriteRenderer renderer =
            coreObject.AddComponent<SpriteRenderer>();

        renderer.sprite = GetWhiteSprite();
        renderer.color = color;
        renderer.sortingLayerName = "Fighters";
        renderer.sortingOrder = 201;

        Vector3 targetScale =
            new Vector3(size, size, 1f);

        coreObject.transform.localScale =
            targetScale * 0.15f;

        pieces.Add(
            new SparkPiece
            {
                PieceTransform =
                    coreObject.transform,

                Renderer = renderer,
                TargetScale = targetScale,
                StartColor = color,
                RotationSpeed = 180f,
                IsCore = true
            }
        );
    }

    private void CreateRay(
        Color color,
        float angle,
        float length,
        float thickness
    )
    {
        GameObject rayObject =
            new GameObject("Ray");

        rayObject.transform.SetParent(
            transform,
            false
        );

        rayObject.transform.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                angle
            );

        SpriteRenderer renderer =
            rayObject.AddComponent<SpriteRenderer>();

        renderer.sprite = GetWhiteSprite();
        renderer.color = color;
        renderer.sortingLayerName = "Fighters";
        renderer.sortingOrder = 200;

        Vector3 targetScale =
            new Vector3(
                length,
                thickness,
                1f
            );

        rayObject.transform.localScale =
            new Vector3(
                length * 0.05f,
                thickness,
                1f
            );

        pieces.Add(
            new SparkPiece
            {
                PieceTransform =
                    rayObject.transform,

                Renderer = renderer,
                TargetScale = targetScale,
                StartColor = color,

                RotationSpeed =
                    Random.Range(-80f, 80f),

                IsCore = false
            }
        );
    }

    private void Update()
    {
        elapsedTime +=
            Time.unscaledDeltaTime;

        float normalizedTime =
            lifetime > 0f
                ? Mathf.Clamp01(
                    elapsedTime / lifetime
                )
                : 1f;

        float growth =
            1f -
            Mathf.Pow(
                1f - normalizedTime,
                3f
            );

        float fade =
            1f -
            Mathf.SmoothStep(
                0f,
                1f,
                normalizedTime
            );

        foreach (SparkPiece piece in pieces)
        {
            if (piece == null ||
                piece.PieceTransform == null ||
                piece.Renderer == null)
            {
                continue;
            }

            if (piece.IsCore)
            {
                float pulse =
                    Mathf.Sin(
                        normalizedTime *
                        Mathf.PI
                    );

                float coreScale =
                    Mathf.Lerp(
                        0.25f,
                        1.15f,
                        pulse
                    );

                piece.PieceTransform.localScale =
                    piece.TargetScale *
                    coreScale;
            }
            else
            {
                piece.PieceTransform.localScale =
                    new Vector3(
                        piece.TargetScale.x *
                        growth,

                        piece.TargetScale.y *
                        Mathf.Lerp(
                            1f,
                            0.45f,
                            normalizedTime
                        ),

                        1f
                    );
            }

            piece.PieceTransform.Rotate(
                0f,
                0f,
                piece.RotationSpeed *
                Time.unscaledDeltaTime
            );

            Color currentColor =
                piece.StartColor;

            currentColor.a =
                piece.StartColor.a * fade;

            piece.Renderer.color =
                currentColor;
        }

        if (normalizedTime >= 1f)
        {
            Destroy(gameObject);
        }
    }

    private static Sprite GetWhiteSprite()
    {
        if (whiteSprite != null)
        {
            return whiteSprite;
        }

        Texture2D texture =
            new Texture2D(
                1,
                1,
                TextureFormat.RGBA32,
                false
            );

        texture.name =
            "RuntimeHitSparkTexture";

        texture.filterMode =
            FilterMode.Point;

        texture.wrapMode =
            TextureWrapMode.Clamp;

        texture.SetPixel(
            0,
            0,
            Color.white
        );

        texture.Apply();

        texture.hideFlags =
            HideFlags.HideAndDontSave;

        whiteSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, 1f, 1f),
            new Vector2(0.5f, 0.5f),
            1f
        );

        whiteSprite.name =
            "RuntimeHitSparkSprite";

        whiteSprite.hideFlags =
            HideFlags.HideAndDontSave;

        return whiteSprite;
    }
}
