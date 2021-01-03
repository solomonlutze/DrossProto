// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

//this ain't the default anymore
// shit belongs to the bugs now a-holes
Shader "Sprites/Default"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        _RendererColor ("RendererColor", Color) = (1,1,1,1)
        _OverrideColor ("OverrideColor  ", Color) = (1,1,1,0)
        _AlphaCutoff ("AlphaCutoff  ", Float) = 0
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
            #pragma vertex DrossSpriteVert
            #pragma fragment DrossSpriteFrag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #pragma multi_compile_local _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA
            #pragma multi_compile_fwdbase nolightmap
            #include "UnitySprites.cginc"
            
// Material Color.
fixed4 _OverrideColor;
            v2f DrossSpriteVert(appdata_t IN)
            {
                v2f OUT;

                UNITY_SETUP_INSTANCE_ID (IN);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.vertex = UnityFlipSprite(IN.vertex, _Flip);
                OUT.vertex = UnityObjectToClipPos(OUT.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color * _RendererColor;

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }
            
            fixed4 DrossSpriteFrag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                c= lerp(c,
                 _OverrideColor, fixed4 (
                   _OverrideColor.a,
                   _OverrideColor.a,
                   _OverrideColor.a,
                   _OverrideColor.a)) * c.a;
                return c;
            }

        ENDCG
        }
    }
}
