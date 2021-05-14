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
			      #pragma multi_compile EDITOR PLAYMODE
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
                OUT.worldpos = mul(unity_ObjectToWorld, IN.vertex);

                #ifdef PIXELSNAP_ON
                OUT.vertex = UnityPixelSnap (OUT.vertex);
                #endif

                return OUT;
            }
            
            fixed4 DrossSpriteFrag(v2f IN) : SV_Target
            {
                fixed4 c = SampleSpriteTexture (IN.texcoord) * IN.color;
                float zDistanceFromCamera = IN.worldpos.z - _WorldSpaceCameraPos.z;
                if (zDistanceFromCamera < 0) {
                  zDistanceFromCamera = 0;
                }
                // c.a *= zDistanceFromCamera;
                c= lerp(c,
                 _OverrideColor, fixed4 (
                   _OverrideColor.a,
                   _OverrideColor.a,
                   _OverrideColor.a,
                   _OverrideColor.a)) * c.a;
                   // Beyond 1 floor away from camera, begin to fade.
                   // Beyond 4 floors away from the camera, we should be completely opaque.
                   // Interpolate linearly between those.
                   // So if we're at 2.5 floors away we should be at 50%.
                   // camera at 0, floor at 4:
                   // (4 - 1) / 4 = .75
                   // rgb *= 1 / .75
                // #ifdef PLAYMODE
                //   if (zDistanceFromCamera > 1) {
                //     float darknessMultiplier = (zDistanceFromCamera - 1) / 3;
                //     if (darknessMultiplier > 1) {
                //       darknessMultiplier = 1;
                //     }
                //     c.rgb *= (1 - darknessMultiplier);
                //   }
	        			// #endif
                // else {
                //   c.a *= zDistanceFromCamera;
                // }
                // c= lerp(c,
                //  _OverrideColor, fixed4 (
                //    _OverrideColor.a,
                //    _OverrideColor.a,
                //    _OverrideColor.a,
                //    _OverrideColor.a)) * zDistanceFromCamera;
                return c;
            }

        ENDCG
        }
    }
}
