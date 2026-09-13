Shader "Igaku/Chroma Key Sprite V2"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _KeyColor ("Key Color", Color) = (0,1,0,1)
        _ColorTolerance ("Color Tolerance", Range(0,0.8)) = 0.24
        _GreenDominance ("Green Dominance", Range(0,0.5)) = 0.08
        _MinimumGreen ("Minimum Green", Range(0,1)) = 0.16
        _EdgeSoftness ("Edge Softness", Range(0.001,0.3)) = 0.035
        _Despill ("Green Despill", Range(0,1)) = 0.9
        _SpillLimit ("Despill Color Allowance", Range(0,0.25)) = 0.035
        [MaterialToggle] PixelSnap ("Pixel Snap", Float) = 0
        [HideInInspector] _RendererColor ("Renderer Color", Color) = (1,1,1,1)
        [HideInInspector] _Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData] _AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData] _EnableExternalAlpha ("Enable External Alpha", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex SpriteVert
            #pragma fragment ChromaFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #include "UnitySprites.cginc"

            fixed4 _KeyColor;
            float _ColorTolerance;
            float _GreenDominance;
            float _MinimumGreen;
            float _EdgeSoftness;
            float _Despill;
            float _SpillLimit;

            fixed4 ChromaFrag(v2f IN) : SV_Target
            {
                fixed4 color = SampleSpriteTexture(IN.texcoord) * IN.color;
                float softness = max(_EdgeSoftness, 0.0001);

                // Remove pixels close to the selected key color.
                float keyDistance = distance(color.rgb, _KeyColor.rgb);
                float colorKey = 1.0 - smoothstep(
                    _ColorTolerance,
                    _ColorTolerance + softness,
                    keyDistance);

                // Also remove darker/anti-aliased key pixels when green is
                // clearly stronger than both red and blue.
                float strongestOther = max(color.r, color.b);
                float dominance = color.g - strongestOther;
                float dominanceKey = smoothstep(
                    _GreenDominance,
                    _GreenDominance + softness,
                    dominance);
                float enoughGreen = smoothstep(
                    _MinimumGreen,
                    _MinimumGreen + softness,
                    color.g);

                float keyAmount = saturate(max(
                    colorKey,
                    dominanceKey * enoughGreen));

                // Suppress residual green on semi-keyed edge pixels.
                float allowedGreen = strongestOther + _SpillLimit;
                float despilledGreen = min(color.g, allowedGreen);
                color.g = lerp(
                    color.g,
                    despilledGreen,
                    saturate(keyAmount * _Despill));

                color.a *= 1.0 - keyAmount;
                color.rgb *= color.a;
                return color;
            }
            ENDCG
        }
    }
}
