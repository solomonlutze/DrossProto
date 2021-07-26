// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "TestShader1"
{
	Properties
	{
		_masktexture("mask texture", 2D) = "white" {}
		_Maintexture("Main texture", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "IsEmissive" = "true"  }
		Cull Back
		Stencil
		{
			Ref 0
			Comp Equal
		}
		CGPROGRAM
		#pragma target 3.5
		#pragma surface surf Unlit alpha:fade keepalpha noshadow exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Maintexture;
		uniform float4 _Maintexture_ST;
		uniform sampler2D _masktexture;
		uniform float4 _masktexture_ST;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_Maintexture = i.uv_texcoord * _Maintexture_ST.xy + _Maintexture_ST.zw;
			float4 tex2DNode1 = tex2D( _Maintexture, uv_Maintexture );
			o.Emission = tex2DNode1.rgb;
			float2 uv_masktexture = i.uv_texcoord * _masktexture_ST.xy + _masktexture_ST.zw;
			o.Alpha = ( tex2DNode1.a * tex2D( _masktexture, uv_masktexture ) ).r;
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16100
848;92;793;573;1721.995;395.436;2.153202;True;False
Node;AmplifyShaderEditor.TexturePropertyNode;14;-1120.914,-22.00833;Float;True;Property;_Maintexture;Main texture;1;0;Create;True;0;0;False;0;None;84508b93f15f2b64386ec07486afc7a3;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;26;-1191.953,448.3895;Float;True;Property;_masktexture;mask texture;0;0;Create;True;0;0;False;0;None;84508b93f15f2b64386ec07486afc7a3;False;white;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;19;-776.7015,86.88084;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;27;-847.7407,557.2787;Float;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;28;-581.2065,456.4621;Float;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;False;0;e9742c575b8f4644fb9379e7347ff62e;16d574e53541bba44a84052fa38778df;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;1;-510.1674,-13.93575;Float;True;Property;_MainTexture;Main Texture;0;0;Create;True;0;0;False;0;e9742c575b8f4644fb9379e7347ff62e;16d574e53541bba44a84052fa38778df;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-174.0111,237.3762;Float;False;2;2;0;FLOAT;0;False;1;COLOR;1,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;21;-51.97579,-239.5406;Float;False;True;3;Float;ASEMaterialInspector;0;0;Unlit;TestShader1;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;True;0;False;-1;255;False;-1;255;False;-1;5;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;19;2;14;0
WireConnection;27;2;26;0
WireConnection;28;0;26;0
WireConnection;28;1;27;0
WireConnection;1;0;14;0
WireConnection;1;1;19;0
WireConnection;25;0;1;4
WireConnection;25;1;28;0
WireConnection;21;2;1;0
WireConnection;21;9;25;0
ASEEND*/
//CHKSM=EDCA606CDEF05C3C2DC2E1B31F1011409BEF5BAF