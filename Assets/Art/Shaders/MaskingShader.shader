// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "MaskingShader"
{
	Properties
	{
		[PerRendererData]_MaskingTexture("Masking Texture", 2D) = "white" {}
		_Cutoff( "Mask Clip Value", Float ) = 0.5
		[Gamma][PerRendererData]_Red0Texture("Red0 Texture", 2D) = "black" {}
		[PerRendererData]_Red0Scale("Red0 Scale", Vector) = (0,0,0,0)
		[PerRendererData]_Red0IntensityMin("Red0 Intensity Min", Range( 0 , 1)) = 0.2215246
		[PerRendererData]_Red0IntensityMax("Red0 Intensity Max", Range( 0 , 1)) = 0.9086623
		[PerRendererData]_Red0Opacity("Red0 Opacity", Range( 0 , 1)) = 1
		[PerRendererData]_Red0ScrollSpeed("Red0 Scroll Speed", Vector) = (0,0,0,0)
		[Gamma][PerRendererData]_Red50Texture("Red50 Texture", 2D) = "black" {}
		[PerRendererData]_Red50Scale("Red50 Scale", Vector) = (0,0,0,0)
		[PerRendererData]_Red50IntensityMin("Red50 Intensity Min", Range( 0 , 1)) = 1
		[PerRendererData]_Red50IntensityMax("Red50 Intensity Max", Range( 0 , 1)) = 0
		[PerRendererData]_Red50Opacity("Red50 Opacity", Range( 0 , 1)) = 0
		[PerRendererData]_Red50ScrollSpeed("Red50 Scroll Speed", Vector) = (0,0,0,0)
		[PerRendererData]_Red100Texture("Red100 Texture", 2D) = "black" {}
		[PerRendererData]_Red100Scale("Red100 Scale", Vector) = (0,0,0,0)
		[PerRendererData]_Red100IntensityMin("Red100 Intensity Min", Range( 0 , 1)) = 1
		[PerRendererData]_Red100IntensityMax("Red100 Intensity Max", Range( 0 , 1)) = 0
		[PerRendererData]_Red100Opacity("Red100 Opacity", Range( 0 , 1)) = 0
		[PerRendererData]_Red100ScrollSpeed("Red100 Scroll Speed", Vector) = (0,0,0,0)
		[Gamma][PerRendererData]_Red150Texture("Red150 Texture", 2D) = "black" {}
		[PerRendererData]_Red150Scale("Red150 Scale", Vector) = (0,0,0,0)
		[PerRendererData]_Red150IntensityMin("Red150 Intensity Min", Range( 0 , 1)) = 1
		[PerRendererData]_Red150IntensityMax("Red150 Intensity Max", Range( 0 , 1)) = 1
		[PerRendererData]_Red150Opacity("Red150 Opacity", Range( 0 , 1)) = 0
		[PerRendererData]_Red150ScrollSpeed("Red150 Scroll Speed", Vector) = (0,0,0,0)
		[Gamma][PerRendererData]_Green0Texture("Green0 Texture", 2D) = "black" {}
		[PerRendererData]_Green0Scale("Green0 Scale", Vector) = (1,1,0,0)
		[PerRendererData]_Green0IntensityMin("Green0 Intensity Min", Range( 0 , 1)) = 0.2215246
		[PerRendererData]_Green0IntensityMax("Green0 Intensity Max", Range( 0 , 1)) = 0
		[PerRendererData]_Green0Opacity("Green0 Opacity", Range( 0 , 1)) = 1
		[PerRendererData]_Green0ScrollSpeed("Green0 Scroll Speed", Vector) = (0,0,0,0)
		[PerRendererData]_Green50Texture("Green50 Texture", 2D) = "black" {}
		[PerRendererData]_Green50Scale("Green50 Scale", Vector) = (1,1,0,0)
		[PerRendererData]_Green50IntensityMin("Green50 Intensity Min", Range( 0 , 1)) = 1
		[PerRendererData]_Green50IntensityMax("Green50 Intensity Max", Range( 0 , 1)) = 0
		[PerRendererData]_Green50Opacity("Green50 Opacity", Range( 0 , 1)) = 1
		[PerRendererData]_Green50ScrollSpeed("Green50 ScrollSpeed", Vector) = (0,0,0,0)
		[PerRendererData]_Green100Texture("Green100 Texture", 2D) = "black" {}
		[PerRendererData]_Green100Scale("Green100 Scale", Vector) = (1,1,0,0)
		[PerRendererData]_Green100IntensityMin("Green100 Intensity Min", Range( 0 , 1)) = 1
		[PerRendererData]_Green100IntensityMax("Green100 Intensity Max", Range( 0 , 1)) = 0
		[PerRendererData]_Green100Opacity("Green100 Opacity", Range( 0 , 1)) = 1
		[PerRendererData]_Green100ScrollSpeed("Green100 Scroll Speed", Vector) = (0,0,0,0)
		[Gamma][PerRendererData]_Green150Texture("Green150 Texture", 2D) = "black" {}
		[PerRendererData]_Green150Scale("Green150 Scale", Vector) = (1,1,0,0)
		[PerRendererData]_Green150IntensityMin("Green150 Intensity Min", Range( 0 , 1)) = 1
		[PerRendererData]_Green150IntensityMax("Green150 Intensity Max", Range( 0 , 1)) = 1
		[PerRendererData]_Green150Opacity("Green150 Opacity", Range( 0 , 1)) = 1
		[PerRendererData]_Green150ScrollSpeed("Green150 Scroll Speed", Vector) = (0,0,0,0)
		[Gamma][PerRendererData]_Blue0Texture("Blue0 Texture", 2D) = "black" {}
		[PerRendererData]_Blue0Scale("Blue0 Scale", Vector) = (1,1,0,0)
		[PerRendererData]_Blue0IntensityMin("Blue0 Intensity Min", Range( 0 , 1)) = 1
		[PerRendererData]_Blue0IntensityMax("Blue0 Intensity Max", Range( 0 , 1)) = 1
		[PerRendererData]_Blue0Opacity("Blue0 Opacity", Range( 0 , 1)) = 1
		[PerRendererData]_Blue0ScrollSpeed("Blue0 Scroll Speed", Vector) = (0,0,0,0)
		[PerRendererData]_Blue50Texture("Blue50 Texture", 2D) = "black" {}
		[PerRendererData]_Blue50Scale("Blue50 Scale", Vector) = (1,1,0,0)
		[PerRendererData]_Blue50IntensityMin("Blue50 Intensity Min", Range( 0 , 1)) = 1
		[PerRendererData]_Blue50IntensityMax("Blue50 Intensity Max", Range( 0 , 1)) = 1
		[PerRendererData]_Blue50Opacity("Blue50 Opacity", Range( 0 , 1)) = 1
		[PerRendererData]_Blue50ScrollSpeed("Blue50 Scroll Speed", Vector) = (0,0,0,0)
		[PerRendererData]_Blue100Texture("Blue100 Texture", 2D) = "black" {}
		[PerRendererData]_Blue100Scale("Blue100 Scale", Vector) = (1,1,0,0)
		[PerRendererData]_Blue100IntensityMin("Blue100 Intensity Min", Range( 0 , 1)) = 1
		[PerRendererData]_Blue100IntensityMax("Blue100 Intensity Max", Range( 0 , 1)) = 1
		[PerRendererData]_Blue100ScrollSpeed("Blue100 Scroll Speed", Vector) = (0,0,0,0)
		[PerRendererData]_Blue100Opacity("Blue100 Opacity", Range( 0 , 1)) = 1
		[Gamma][PerRendererData]_Blue150Texture("Blue150 Texture", 2D) = "black" {}
		[PerRendererData]_Blue150Scale("Blue150 Scale", Vector) = (1,1,0,0)
		[PerRendererData]_Blue150IntensityMin("Blue150 Intensity Min", Range( 0 , 1)) = 1
		[PerRendererData]_Blue150IntensityMax("Blue150 Intensity Max", Range( 0 , 1)) = 1
		[PerRendererData]_Blue150Opacity("Blue150 Opacity", Range( 0 , 1)) = 1
		[PerRendererData]_Blue150ScrollSpeed("Blue150 Scroll Speed", Vector) = (0,0,0,0)
		_MaskRotation("MaskRotation", Float) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+0" "IsEmissive" = "true"  }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha addshadow fullforwardshadows exclude_path:deferred 
		struct Input
		{
			float2 uv_texcoord;
			float3 worldPos;
		};

		uniform sampler2D _MaskingTexture;
		uniform float4 _MaskingTexture_ST;
		uniform float _MaskRotation;
		uniform float _Red150Opacity;
		uniform sampler2D _Red150Texture;
		uniform float2 _Red150ScrollSpeed;
		uniform float2 _Red150Scale;
		uniform float _Red150IntensityMin;
		uniform float _Red150IntensityMax;
		uniform float _Red100Opacity;
		uniform sampler2D _Red100Texture;
		uniform float2 _Red100ScrollSpeed;
		uniform float2 _Red100Scale;
		uniform float _Red100IntensityMin;
		uniform float _Red100IntensityMax;
		uniform float _Red50Opacity;
		uniform sampler2D _Red50Texture;
		uniform float2 _Red50ScrollSpeed;
		uniform float2 _Red50Scale;
		uniform float _Red50IntensityMin;
		uniform float _Red50IntensityMax;
		uniform float _Red0Opacity;
		uniform sampler2D _Red0Texture;
		uniform float2 _Red0ScrollSpeed;
		uniform float2 _Red0Scale;
		uniform float _Red0IntensityMin;
		uniform float _Red0IntensityMax;
		uniform float _Green150Opacity;
		uniform sampler2D _Green150Texture;
		uniform float2 _Green150ScrollSpeed;
		uniform float2 _Green150Scale;
		uniform float _Green150IntensityMin;
		uniform float _Green150IntensityMax;
		uniform float _Green100Opacity;
		uniform sampler2D _Green100Texture;
		uniform float2 _Green100ScrollSpeed;
		uniform float2 _Green100Scale;
		uniform float _Green100IntensityMin;
		uniform float _Green100IntensityMax;
		uniform float _Green50Opacity;
		uniform sampler2D _Green50Texture;
		uniform float2 _Green50ScrollSpeed;
		uniform float2 _Green50Scale;
		uniform float _Green50IntensityMin;
		uniform float _Green50IntensityMax;
		uniform float _Green0Opacity;
		uniform sampler2D _Green0Texture;
		uniform float2 _Green0ScrollSpeed;
		uniform float2 _Green0Scale;
		uniform float _Green0IntensityMin;
		uniform float _Green0IntensityMax;
		uniform float _Blue0Opacity;
		uniform sampler2D _Blue0Texture;
		uniform float2 _Blue0ScrollSpeed;
		uniform float2 _Blue0Scale;
		uniform float _Blue0IntensityMin;
		uniform float _Blue0IntensityMax;
		uniform float _Blue50Opacity;
		uniform sampler2D _Blue50Texture;
		uniform float2 _Blue50ScrollSpeed;
		uniform float2 _Blue50Scale;
		uniform float _Blue50IntensityMin;
		uniform float _Blue50IntensityMax;
		uniform float _Blue100Opacity;
		uniform sampler2D _Blue100Texture;
		uniform float2 _Blue100ScrollSpeed;
		uniform float2 _Blue100Scale;
		uniform float _Blue100IntensityMin;
		uniform float _Blue100IntensityMax;
		uniform float _Blue150Opacity;
		uniform sampler2D _Blue150Texture;
		uniform float2 _Blue150ScrollSpeed;
		uniform float2 _Blue150Scale;
		uniform float _Blue150IntensityMin;
		uniform float _Blue150IntensityMax;
		uniform float _Cutoff = 0.5;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float temp_output_3_0_g26 = 0.0;
			float temp_output_3_0_g36 = ( temp_output_3_0_g26 + 50.0 );
			float temp_output_3_0_g42 = ( temp_output_3_0_g36 + 50.0 );
			float temp_output_3_0_g50 = ( temp_output_3_0_g42 + 50.0 );
			float2 uv_MaskingTexture = i.uv_texcoord * _MaskingTexture_ST.xy + _MaskingTexture_ST.zw;
			float cos560 = cos( _MaskRotation );
			float sin560 = sin( _MaskRotation );
			float2 rotator560 = mul( uv_MaskingTexture - float2( 0.5,0.5 ) , float2x2( cos560 , -sin560 , sin560 , cos560 )) + float2( 0.5,0.5 );
			float4 tex2DNode101 = tex2D( _MaskingTexture, rotator560 );
			float maskRedChannel331 = tex2DNode101.r;
			float3 ase_worldPos = i.worldPos;
			float temp_output_26_0_g49 = _Red150IntensityMin;
			float temp_output_27_0_g49 = _Red150IntensityMax;
			float temp_output_26_0_g41 = _Red100IntensityMin;
			float temp_output_27_0_g41 = _Red100IntensityMax;
			float temp_output_26_0_g35 = _Red50IntensityMin;
			float temp_output_27_0_g35 = _Red50IntensityMax;
			float temp_output_26_0_g25 = _Red0IntensityMin;
			float temp_output_27_0_g25 = _Red0IntensityMax;
			float temp_output_3_0_g34 = 0.0;
			float temp_output_3_0_g44 = ( temp_output_3_0_g34 + 50.0 );
			float temp_output_3_0_g46 = ( temp_output_3_0_g44 + 50.0 );
			float temp_output_3_0_g56 = ( temp_output_3_0_g46 + 50.0 );
			float maskGreenChannel341 = tex2DNode101.g;
			float temp_output_26_0_g55 = _Green150IntensityMin;
			float temp_output_27_0_g55 = _Green150IntensityMax;
			float temp_output_26_0_g45 = _Green100IntensityMin;
			float temp_output_27_0_g45 = _Green100IntensityMax;
			float temp_output_26_0_g43 = _Green50IntensityMin;
			float temp_output_27_0_g43 = _Green50IntensityMax;
			float temp_output_26_0_g33 = _Green0IntensityMin;
			float temp_output_27_0_g33 = _Green0IntensityMax;
			float temp_output_3_0_g38 = 0.0;
			float maskBlueChannel342 = tex2DNode101.b;
			float temp_output_26_0_g37 = _Blue0IntensityMin;
			float temp_output_27_0_g37 = _Blue0IntensityMax;
			float temp_output_3_0_g40 = ( temp_output_3_0_g38 + 50.0 );
			float temp_output_26_0_g39 = _Blue50IntensityMin;
			float temp_output_27_0_g39 = _Blue50IntensityMax;
			float temp_output_3_0_g48 = ( temp_output_3_0_g40 + 50.0 );
			float temp_output_26_0_g47 = _Blue100IntensityMin;
			float temp_output_27_0_g47 = _Blue100IntensityMax;
			float temp_output_3_0_g58 = ( temp_output_3_0_g48 + 50.0 );
			float temp_output_26_0_g57 = _Blue150IntensityMin;
			float temp_output_27_0_g57 = _Blue150IntensityMax;
			o.Emission = ( ( ( step( temp_output_3_0_g50 , ( maskRedChannel331 * 255.0 ) ) * ( ( _Red150Opacity * tex2D( _Red150Texture, ( float3( ( _Red150ScrollSpeed * _Time.y ) ,  0.0 ) + ( float3( _Red150Scale ,  0.0 ) * ase_worldPos ) ).xy ) ) * ( ( ( temp_output_26_0_g49 + temp_output_27_0_g49 ) + ( ( temp_output_26_0_g49 - temp_output_27_0_g49 ) * cos( _Time.y ) ) ) / 2.0 ) ) ) + ( step( temp_output_3_0_g42 , ( maskRedChannel331 * 255.0 ) ) * ( ( _Red100Opacity * tex2D( _Red100Texture, ( float3( ( _Red100ScrollSpeed * _Time.y ) ,  0.0 ) + ( float3( _Red100Scale ,  0.0 ) * ase_worldPos ) ).xy ) ) * ( ( ( temp_output_26_0_g41 + temp_output_27_0_g41 ) + ( ( temp_output_26_0_g41 - temp_output_27_0_g41 ) * cos( _Time.y ) ) ) / 2.0 ) ) ) + ( step( temp_output_3_0_g36 , ( maskRedChannel331 * 255.0 ) ) * ( ( _Red50Opacity * tex2D( _Red50Texture, ( float3( ( _Red50ScrollSpeed * _Time.y ) ,  0.0 ) + ( float3( _Red50Scale ,  0.0 ) * ase_worldPos ) ).xy ) ) * ( ( ( temp_output_26_0_g35 + temp_output_27_0_g35 ) + ( ( temp_output_26_0_g35 - temp_output_27_0_g35 ) * cos( _Time.y ) ) ) / 2.0 ) ) ) + ( step( temp_output_3_0_g26 , ( maskRedChannel331 * 255.0 ) ) * ( ( _Red0Opacity * tex2D( _Red0Texture, ( float3( ( _Red0ScrollSpeed * _Time.y ) ,  0.0 ) + ( float3( _Red0Scale ,  0.0 ) * ase_worldPos ) ).xy ) ) * ( ( ( temp_output_26_0_g25 + temp_output_27_0_g25 ) + ( ( temp_output_26_0_g25 - temp_output_27_0_g25 ) * cos( _Time.y ) ) ) / 2.0 ) ) ) ) + ( ( step( temp_output_3_0_g56 , ( maskGreenChannel341 * 255.0 ) ) * ( ( _Green150Opacity * tex2D( _Green150Texture, ( float3( ( _Green150ScrollSpeed * _Time.y ) ,  0.0 ) + ( float3( _Green150Scale ,  0.0 ) * ase_worldPos ) ).xy ) ) * ( ( ( temp_output_26_0_g55 + temp_output_27_0_g55 ) + ( ( temp_output_26_0_g55 - temp_output_27_0_g55 ) * cos( _Time.y ) ) ) / 2.0 ) ) ) + ( step( temp_output_3_0_g46 , ( maskGreenChannel341 * 255.0 ) ) * ( ( _Green100Opacity * tex2D( _Green100Texture, ( float3( ( _Green100ScrollSpeed * _Time.y ) ,  0.0 ) + ( float3( _Green100Scale ,  0.0 ) * ase_worldPos ) ).xy ) ) * ( ( ( temp_output_26_0_g45 + temp_output_27_0_g45 ) + ( ( temp_output_26_0_g45 - temp_output_27_0_g45 ) * cos( _Time.y ) ) ) / 2.0 ) ) ) + ( step( temp_output_3_0_g44 , ( maskGreenChannel341 * 255.0 ) ) * ( ( _Green50Opacity * tex2D( _Green50Texture, ( float3( ( _Green50ScrollSpeed * _Time.y ) ,  0.0 ) + ( float3( _Green50Scale ,  0.0 ) * ase_worldPos ) ).xy ) ) * ( ( ( temp_output_26_0_g43 + temp_output_27_0_g43 ) + ( ( temp_output_26_0_g43 - temp_output_27_0_g43 ) * cos( _Time.y ) ) ) / 2.0 ) ) ) + ( step( temp_output_3_0_g34 , ( maskGreenChannel341 * 255.0 ) ) * ( ( _Green0Opacity * tex2D( _Green0Texture, ( float3( ( _Green0ScrollSpeed * _Time.y ) ,  0.0 ) + ( float3( _Green0Scale ,  0.0 ) * ase_worldPos ) ).xy ) ) * ( ( ( temp_output_26_0_g33 + temp_output_27_0_g33 ) + ( ( temp_output_26_0_g33 - temp_output_27_0_g33 ) * cos( _Time.y ) ) ) / 2.0 ) ) ) ) + ( ( step( temp_output_3_0_g38 , ( maskBlueChannel342 * 255.0 ) ) * ( ( _Blue0Opacity * tex2D( _Blue0Texture, ( float3( ( _Blue0ScrollSpeed * _Time.y ) ,  0.0 ) + ( float3( _Blue0Scale ,  0.0 ) * ase_worldPos ) ).xy ) ) * ( ( ( temp_output_26_0_g37 + temp_output_27_0_g37 ) + ( ( temp_output_26_0_g37 - temp_output_27_0_g37 ) * cos( _Time.y ) ) ) / 2.0 ) ) ) + ( step( temp_output_3_0_g40 , ( maskBlueChannel342 * 255.0 ) ) * ( ( _Blue50Opacity * tex2D( _Blue50Texture, ( float3( ( _Blue50ScrollSpeed * _Time.y ) ,  0.0 ) + ( float3( _Blue50Scale ,  0.0 ) * ase_worldPos ) ).xy ) ) * ( ( ( temp_output_26_0_g39 + temp_output_27_0_g39 ) + ( ( temp_output_26_0_g39 - temp_output_27_0_g39 ) * cos( _Time.y ) ) ) / 2.0 ) ) ) + ( step( temp_output_3_0_g48 , ( maskBlueChannel342 * 255.0 ) ) * ( ( _Blue100Opacity * tex2D( _Blue100Texture, ( float3( ( _Blue100ScrollSpeed * _Time.y ) ,  0.0 ) + ( float3( _Blue100Scale ,  0.0 ) * ase_worldPos ) ).xy ) ) * ( ( ( temp_output_26_0_g47 + temp_output_27_0_g47 ) + ( ( temp_output_26_0_g47 - temp_output_27_0_g47 ) * cos( _Time.y ) ) ) / 2.0 ) ) ) + ( step( temp_output_3_0_g58 , ( maskBlueChannel342 * 255.0 ) ) * ( ( _Blue150Opacity * tex2D( _Blue150Texture, ( float3( ( _Blue150ScrollSpeed * _Time.y ) ,  0.0 ) + ( float3( _Blue150Scale ,  0.0 ) * ase_worldPos ) ).xy ) ) * ( ( ( temp_output_26_0_g57 + temp_output_27_0_g57 ) + ( ( temp_output_26_0_g57 - temp_output_27_0_g57 ) * cos( _Time.y ) ) ) / 2.0 ) ) ) ) ).rgb;
			o.Alpha = 1;
			float maskAlpha329 = tex2DNode101.a;
			clip( maskAlpha329 - _Cutoff );
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=16100
645;75;476;407;5510.848;1501.912;2.2;False;False
Node;AmplifyShaderEditor.Vector2Node;561;-7090.578,-1258.295;Float;False;Constant;_Vector0;Vector 0;74;0;Create;True;0;0;False;0;0.5,0.5;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;562;-7035.522,-1087.974;Float;False;Property;_MaskRotation;MaskRotation;74;0;Create;True;0;0;False;0;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;559;-7134.29,-1462.561;Float;False;0;101;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;343;-5620.339,-1138.779;Float;False;5040.14;1054.034;All textures that should appear in red parts of the image.;9;337;336;335;334;315;305;102;299;293;Red Channel;1,1,1,1;0;0
Node;AmplifyShaderEditor.RotatorNode;560;-6813.593,-1290.983;Float;False;3;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;2;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.CommentaryNode;504;-5618.962,1487.042;Float;False;5094.372;898.0527;All textures that should appear in blue parts of the image;9;501;502;503;500;471;460;457;454;451;Blue Channel;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;293;-5317.426,-879.3208;Float;False;1023.363;435.3832;Comment;9;86;87;79;84;157;92;301;302;332;Red >= 0;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;345;-5631.235,212.015;Float;False;5040.14;1054.034;All textures that should appear in green parts of the image.;9;363;352;351;348;346;357;447;449;450;Green Channel;1,1,1,1;0;0
Node;AmplifyShaderEditor.SamplerNode;101;-6555.172,-1274.764;Float;True;Property;_MaskingTexture;Masking Texture;0;1;[PerRendererData];Create;True;0;0;False;0;None;34a57996c8aa500468a6811a5ffc54a0;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;6;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;451;-5310.295,1852.128;Float;False;1023.363;435.3832;Comment;9;464;463;462;461;459;458;456;453;452;Blue >= 0;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;346;-5328.322,471.4736;Float;False;1023.363;435.3832;Comment;8;356;353;349;350;359;354;355;347;Green >= 0;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;92;-5297.714,-814.6942;Float;True;Property;_Red0Texture;Red0 Texture;2;2;[Gamma];[PerRendererData];Create;True;0;0;False;0;None;70d4e64e116f25c498e7e473c5d6e738;False;black;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;331;-5875.884,-1247.189;Float;False;maskRedChannel;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;299;-4212.915,-880.1395;Float;False;1081.194;418.1448;Comment;9;34;35;158;43;48;93;303;304;333;Red >= 50;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;347;-5275.267,528.6568;Float;True;Property;_Green0Texture;Green0 Texture;26;2;[Gamma];[PerRendererData];Create;True;0;0;False;0;None;70d4e64e116f25c498e7e473c5d6e738;False;black;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;452;-5257.241,1909.311;Float;True;Property;_Blue0Texture;Blue0 Texture;50;2;[Gamma];[PerRendererData];Create;True;0;0;False;0;None;70d4e64e116f25c498e7e473c5d6e738;False;black;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.WireNode;301;-5084.723,-700.799;Float;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.Vector2Node;158;-3958.66,-826.1733;Float;False;Property;_Red50Scale;Red50 Scale;9;1;[PerRendererData];Create;True;0;0;False;0;0,0;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.WireNode;349;-5062.276,642.5519;Float;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;84;-5296.397,-598.707;Float;False;Property;_Red0Opacity;Red0 Opacity;6;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;87;-4854.843,-808.2924;Float;False;Property;_Red0IntensityMax;Red0 Intensity Max;5;1;[PerRendererData];Create;True;0;0;False;0;0.9086623;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;86;-4562.086,-812.478;Float;False;Property;_Red0IntensityMin;Red0 Intensity Min;4;1;[PerRendererData];Create;True;0;0;False;0;0.2215246;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;157;-5051.813,-811.2047;Float;False;Property;_Red0Scale;Red0 Scale;3;1;[PerRendererData];Create;True;0;0;False;0;0,0;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.WireNode;453;-5044.25,2023.206;Float;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.WireNode;302;-5092.131,-627.4773;Float;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;341;-5878.505,-1168.016;Float;False;maskGreenChannel;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;348;-4227.928,488.4962;Float;False;1081.194;418.1448;Comment;9;365;370;369;358;366;368;367;371;360;Green >= 50;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;305;-3029.915,-878.6537;Float;False;1081.194;418.1448;Comment;9;313;312;311;310;309;308;307;306;339;Red >= 100;1,1,1,1;0;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;342;-5869.505,-1080.016;Float;False;maskBlueChannel;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;79;-4982.902,-555.9446;Float;False;Property;_Red0ScrollSpeed;Red0 Scroll Speed;7;1;[PerRendererData];Create;True;0;0;False;0;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;102;-5601.793,-712.3199;Float;True;Constant;_MaskingClipRed;Masking Clip Red;13;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;332;-4984.884,-655.1891;Float;False;331;maskRedChannel;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;454;-4209.902,1869.15;Float;False;1081.194;418.1448;Comment;9;476;475;474;473;472;470;469;465;455;Blue >= 50;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexturePropertyNode;48;-4203.905,-836.4649;Float;True;Property;_Red50Texture;Red50 Texture;8;2;[Gamma];[PerRendererData];Create;True;0;0;False;0;None;5c9357613a0af824e9413099165c2b21;False;black;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;35;-3777.528,-815.697;Float;False;Property;_Red50IntensityMax;Red50 Intensity Max;11;1;[PerRendererData];Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;307;-3020.905,-834.979;Float;True;Property;_Red100Texture;Red100 Texture;14;1;[PerRendererData];Create;True;0;0;False;0;None;5c9357613a0af824e9413099165c2b21;False;black;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;351;-5612.689,638.4744;Float;True;Constant;_Float1;Float 1;13;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;350;-5273.95,744.644;Float;False;Property;_Green0Opacity;Green0 Opacity;30;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;354;-5069.684,715.8737;Float;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-3429.161,-818.8626;Float;False;Property;_Red50IntensityMin;Red50 Intensity Min;10;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;306;-2775.66,-824.6874;Float;False;Property;_Red100Scale;Red100 Scale;15;1;[PerRendererData];Create;True;0;0;False;0;0,0;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;359;-4832.396,535.0585;Float;False;Property;_Green0IntensityMax;Green0 Intensity Max;29;1;[PerRendererData];Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;456;-4545.613,1919.527;Float;False;Property;_Blue0IntensityMin;Blue0 Intensity Min;52;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;458;-5255.924,2125.298;Float;False;Property;_Blue0Opacity;Blue0 Opacity;54;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;575;-4675.822,-692.9721;Float;False;MaskingShaderTexture;-1;;25;a40a5c1af537d1843b06044af40c3749;0;9;45;FLOAT2;1,1;False;33;FLOAT;0;False;32;FLOAT;0;False;28;SAMPLER2D;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;27;FLOAT;0;False;26;FLOAT;0;False;25;FLOAT;0;False;24;FLOAT;0;False;23;FLOAT2;0,0;False;2;FLOAT;31;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;462;-4944.411,2068.816;Float;False;342;maskBlueChannel;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;353;-5029.366,532.1463;Float;False;Property;_Green0Scale;Green0 Scale;27;1;[PerRendererData];Create;True;0;0;False;0;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;93;-4203.587,-616.1953;Float;False;Property;_Red50Opacity;Red50 Opacity;12;1;[PerRendererData];Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;465;-4167.549,1905.382;Float;True;Property;_Blue50Texture;Blue50 Texture;56;1;[PerRendererData];Create;True;0;0;False;0;None;5c9357613a0af824e9413099165c2b21;False;black;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.Vector2Node;455;-3922.307,1915.674;Float;False;Property;_Blue50Scale;Blue50 Scale;57;1;[PerRendererData];Create;True;0;0;False;0;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;360;-3940.331,535.0192;Float;False;Property;_Green50Scale;Green50 Scale;33;1;[PerRendererData];Create;True;0;0;False;0;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;460;-5594.663,2019.129;Float;True;Constant;_Float3;Float 3;13;0;Create;True;0;0;False;0;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;463;-4814.37,1915.713;Float;False;Property;_Blue0IntensityMax;Blue0 Intensity Max;53;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;457;-3022.785,1852.795;Float;False;1081.194;418.1448;Comment;9;484;483;481;480;479;478;477;467;466;Blue >= 100;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;352;-3040.81,472.1407;Float;False;1081.194;418.1448;Comment;9;377;373;376;375;379;378;374;361;362;Green >= 100;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;357;-4539.639,530.873;Float;False;Property;_Green0IntensityMin;Green0 Intensity Min;28;1;[PerRendererData];Create;True;0;0;False;0;0.2215246;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;43;-3892.035,-593.7758;Float;False;Property;_Red50ScrollSpeed;Red50 Scroll Speed;13;1;[PerRendererData];Create;True;0;0;False;0;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;464;-5011.34,1912.801;Float;False;Property;_Blue0Scale;Blue0 Scale;51;1;[PerRendererData];Create;True;0;0;False;0;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TexturePropertyNode;358;-4185.574,524.7275;Float;True;Property;_Green50Texture;Green50 Texture;32;1;[PerRendererData];Create;True;0;0;False;0;None;5c9357613a0af824e9413099165c2b21;False;black;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.GetLocalVarNode;333;-3901.152,-672.4508;Float;False;331;maskRedChannel;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;355;-4962.437,688.1619;Float;False;341;maskGreenChannel;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;303;-3791.321,-732.5577;Float;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;356;-4993.797,794.8499;Float;False;Property;_Green0ScrollSpeed;Green0 Scroll Speed;31;1;[PerRendererData];Create;True;0;0;False;0;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.WireNode;304;-3926.74,-651.6722;Float;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.CommentaryNode;315;-1844.255,-881.2542;Float;False;1081.194;418.1448;Comment;9;323;322;321;320;319;318;317;316;340;Red >= 150;1,1,1,1;0;0
Node;AmplifyShaderEditor.WireNode;459;-5051.658,2096.528;Float;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.Vector2Node;461;-4975.771,2175.504;Float;False;Property;_Blue0ScrollSpeed;Blue0 Scroll Speed;55;1;[PerRendererData];Create;True;0;0;False;0;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;339;-2717.806,-680.4351;Float;False;331;maskRedChannel;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;368;-4188.788,744.9971;Float;False;Property;_Green50Opacity;Green50 Opacity;36;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;469;-3889.022,2155.514;Float;False;Property;_Blue50ScrollSpeed;Blue50 Scroll Speed;61;1;[PerRendererData];Create;True;0;0;False;0;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.Vector2Node;312;-2709.035,-592.2897;Float;False;Property;_Red100ScrollSpeed;Red100 Scroll Speed;19;1;[PerRendererData];Create;True;0;0;False;0;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;311;-3032.588,-614.7094;Float;False;Property;_Red100Opacity;Red100 Opacity;18;1;[PerRendererData];Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;371;-3486.719,547.7255;Float;False;Property;_Green50IntensityMin;Green50 Intensity Min;34;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;466;-2983.685,1889.026;Float;True;Property;_Blue100Texture;Blue100 Texture;62;1;[PerRendererData];Create;True;0;0;False;0;None;5c9357613a0af824e9413099165c2b21;False;black;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;367;-3759.198,545.4955;Float;False;Property;_Green50IntensityMax;Green50 Intensity Max;35;1;[PerRendererData];Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;576;-3524.319,-721.7075;Float;True;MaskingShaderTexture;-1;;35;a40a5c1af537d1843b06044af40c3749;0;9;45;FLOAT2;1,1;False;33;FLOAT;0;False;32;FLOAT;0;False;28;SAMPLER2D;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;27;FLOAT;0;False;26;FLOAT;0;False;25;FLOAT;0;False;24;FLOAT;0;False;23;FLOAT2;0,0;False;2;FLOAT;31;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;577;-4686.717,657.8223;Float;False;MaskingShaderTexture;-1;;33;a40a5c1af537d1843b06044af40c3749;0;9;45;FLOAT2;1,1;False;33;FLOAT;0;False;32;FLOAT;0;False;28;SAMPLER2D;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;27;FLOAT;0;False;26;FLOAT;0;False;25;FLOAT;0;False;24;FLOAT;0;False;23;FLOAT2;0,0;False;2;FLOAT;31;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;578;-4668.691,2038.477;Float;False;MaskingShaderTexture;-1;;37;a40a5c1af537d1843b06044af40c3749;0;9;45;FLOAT2;1,1;False;33;FLOAT;0;False;32;FLOAT;0;False;28;SAMPLER2D;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;27;FLOAT;0;False;26;FLOAT;0;False;25;FLOAT;0;False;24;FLOAT;0;False;23;FLOAT2;0,0;False;2;FLOAT;31;COLOR;0
Node;AmplifyShaderEditor.Vector2Node;316;-1590,-827.288;Float;False;Property;_Red150Scale;Red150 Scale;21;1;[PerRendererData];Create;True;0;0;False;0;0,0;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;313;-2246.161,-817.3768;Float;False;Property;_Red100IntensityMin;Red100 Intensity Min;16;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;361;-2753.213,518.6636;Float;False;Property;_Green100Scale;Green100 Scale;39;1;[PerRendererData];Create;True;0;0;False;0;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.WireNode;309;-2608.321,-731.0719;Float;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;472;-3741.173,1926.15;Float;False;Property;_Blue50IntensityMax;Blue50 Intensity Max;59;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;471;-1837.125,1850.195;Float;False;1081.194;418.1448;Comment;9;494;493;492;491;490;489;488;486;485;Blue >= 150;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;363;-1855.149,469.5403;Float;False;1081.194;418.1448;Comment;9;389;388;386;385;384;383;382;381;380;Green >= 150;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;475;-4170.763,2125.651;Float;False;Property;_Blue50Opacity;Blue50 Opacity;60;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;473;-3754.966,2009.289;Float;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.Vector2Node;365;-3907.047,774.86;Float;False;Property;_Green50ScrollSpeed;Green50 ScrollSpeed;37;1;[PerRendererData];Create;True;0;0;False;0;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.WireNode;370;-3772.991,628.6346;Float;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;310;-2594.528,-814.2112;Float;False;Property;_Red100IntensityMax;Red100 Intensity Max;17;1;[PerRendererData];Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;470;-3864.798,2069.396;Float;False;342;maskBlueChannel;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;369;-3882.823,688.7416;Float;False;341;maskGreenChannel;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;317;-1835.245,-837.5795;Float;True;Property;_Red150Texture;Red150 Texture;20;2;[Gamma];[PerRendererData];Create;True;0;0;False;0;None;5c9357613a0af824e9413099165c2b21;False;black;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.WireNode;474;-3890.385,2090.174;Float;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.TexturePropertyNode;362;-3017.709,508.372;Float;True;Property;_Green100Texture;Green100 Texture;38;1;[PerRendererData];Create;True;0;0;False;0;None;5c9357613a0af824e9413099165c2b21;False;black;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.WireNode;308;-2799.74,-719.1864;Float;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.WireNode;366;-3908.41,709.5201;Float;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.Vector2Node;467;-2735.188,1899.318;Float;False;Property;_Blue100Scale;Blue100 Scale;63;1;[PerRendererData];Create;True;0;0;False;0;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;476;-3392.807,1922.984;Float;False;Property;_Blue50IntensityMin;Blue50 Intensity Min;58;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;380;-1846.139,513.2148;Float;True;Property;_Green150Texture;Green150 Texture;44;2;[Gamma];[PerRendererData];Create;True;0;0;False;0;None;5c9357613a0af824e9413099165c2b21;False;black;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.WireNode;483;-2759.269,2004.819;Float;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;321;-1060.501,-819.9771;Float;False;Property;_Red150IntensityMin;Red150 Intensity Min;22;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;374;-2695.359,662.9158;Float;False;341;maskGreenChannel;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;319;-1402.099,-848.4008;Float;True;Property;_Red150IntensityMax;Red150 Intensity Max;23;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;379;-2572.082,529.1398;Float;False;Property;_Green100IntensityMax;Green100 Intensity Max;41;1;[PerRendererData];Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;581;-3521.307,2027.583;Float;True;MaskingShaderTexture;-1;;39;a40a5c1af537d1843b06044af40c3749;0;9;45;FLOAT2;1,1;False;33;FLOAT;0;False;32;FLOAT;0;False;28;SAMPLER2D;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;27;FLOAT;0;False;26;FLOAT;0;False;25;FLOAT;0;False;24;FLOAT;0;False;23;FLOAT2;0,0;False;2;FLOAT;31;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;580;-2341.319,-720.2217;Float;True;MaskingShaderTexture;-1;;41;a40a5c1af537d1843b06044af40c3749;0;9;45;FLOAT2;1,1;False;33;FLOAT;0;False;32;FLOAT;0;False;28;SAMPLER2D;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;27;FLOAT;0;False;26;FLOAT;0;False;25;FLOAT;0;False;24;FLOAT;0;False;23;FLOAT2;0,0;False;2;FLOAT;31;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;579;-3539.331,646.9283;Float;True;MaskingShaderTexture;-1;;43;a40a5c1af537d1843b06044af40c3749;0;9;45;FLOAT2;1,1;False;33;FLOAT;0;False;32;FLOAT;0;False;28;SAMPLER2D;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;27;FLOAT;0;False;26;FLOAT;0;False;25;FLOAT;0;False;24;FLOAT;0;False;23;FLOAT2;0,0;False;2;FLOAT;31;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;378;-2998.142,728.6415;Float;False;Property;_Green100Opacity;Green100 Opacity;42;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.TexturePropertyNode;485;-1828.114,1893.869;Float;True;Property;_Blue150Texture;Blue150 Texture;68;2;[Gamma];[PerRendererData];Create;True;0;0;False;0;None;5c9357613a0af824e9413099165c2b21;False;black;Auto;Texture2D;0;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;478;-2205.689,1906.628;Float;False;Property;_Blue100IntensityMin;Blue100 Intensity Min;64;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;322;-1523.375,-594.8896;Float;False;Property;_Red150ScrollSpeed;Red150 Scroll Speed;25;1;[PerRendererData];Create;True;0;0;False;0;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;477;-2677.334,2043.57;Float;False;342;maskBlueChannel;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;376;-2263.283,525.9742;Float;False;Property;_Green100IntensityMin;Green100 Intensity Min;40;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;323;-1422.661,-733.672;Float;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;373;-2585.875,612.2791;Float;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;318;-1614.08,-721.787;Float;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.Vector2Node;377;-2719.93,758.5047;Float;False;Property;_Green100ScrollSpeed;Green100 Scroll Speed;43;1;[PerRendererData];Create;True;0;0;False;0;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.WireNode;375;-2777.293,624.1646;Float;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.Vector2Node;486;-1582.869,1904.161;Float;False;Property;_Blue150Scale;Blue150 Scale;69;1;[PerRendererData];Create;True;0;0;False;0;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;320;-1834.928,-617.3097;Float;False;Property;_Red150Opacity;Red150 Opacity;24;1;[PerRendererData];Create;True;0;0;False;0;0;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;381;-1600.894,523.5063;Float;False;Property;_Green150Scale;Green150 Scale;45;1;[PerRendererData];Create;True;0;0;False;0;1,1;1,1;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.GetLocalVarNode;340;-1584.239,-674.9662;Float;False;331;maskRedChannel;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;484;-2567.851,1992.933;Float;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;479;-2554.058,1909.794;Float;False;Property;_Blue100IntensityMax;Blue100 Intensity Max;65;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;481;-2701.905,2139.159;Float;False;Property;_Blue100ScrollSpeed;Blue100 Scroll Speed;66;1;[PerRendererData];Create;True;0;0;False;0;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;480;-2980.117,2109.296;Float;False;Property;_Blue100Opacity;Blue100 Opacity;67;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;384;-1629.443,668.966;Float;False;341;maskGreenChannel;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;388;-1433.555,617.1224;Float;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.FunctionNode;584;-2352.214,630.5728;Float;True;MaskingShaderTexture;-1;;45;a40a5c1af537d1843b06044af40c3749;0;9;45;FLOAT2;1,1;False;33;FLOAT;0;False;32;FLOAT;0;False;28;SAMPLER2D;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;27;FLOAT;0;False;26;FLOAT;0;False;25;FLOAT;0;False;24;FLOAT;0;False;23;FLOAT2;0,0;False;2;FLOAT;31;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;583;-2334.189,2011.227;Float;True;MaskingShaderTexture;-1;;47;a40a5c1af537d1843b06044af40c3749;0;9;45;FLOAT2;1,1;False;33;FLOAT;0;False;32;FLOAT;0;False;28;SAMPLER2D;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;27;FLOAT;0;False;26;FLOAT;0;False;25;FLOAT;0;False;24;FLOAT;0;False;23;FLOAT2;0,0;False;2;FLOAT;31;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;582;-1155.659,-722.8219;Float;True;MaskingShaderTexture;-1;;49;a40a5c1af537d1843b06044af40c3749;0;9;45;FLOAT2;1,1;False;33;FLOAT;0;False;32;FLOAT;0;False;28;SAMPLER2D;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;27;FLOAT;0;False;26;FLOAT;0;False;25;FLOAT;0;False;24;FLOAT;0;False;23;FLOAT2;0,0;False;2;FLOAT;31;COLOR;0
Node;AmplifyShaderEditor.WireNode;488;-1415.53,1997.777;Float;False;1;0;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.WireNode;500;-4284.917,2088.149;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;385;-1073.917,530.8172;Float;False;Property;_Green150IntensityMin;Green150 Intensity Min;46;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;494;-1396.34,1906.379;Float;False;Property;_Blue150IntensityMax;Blue150 Intensity Max;71;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;382;-1414.365,525.7245;Float;False;Property;_Green150IntensityMax;Green150 Intensity Max;47;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WireNode;489;-1606.949,2009.662;Float;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;383;-1845.822,733.4847;Float;False;Property;_Green150Opacity;Green150 Opacity;48;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;386;-1534.269,755.9048;Float;False;Property;_Green150ScrollSpeed;Green150 Scroll Speed;49;1;[PerRendererData];Create;True;0;0;False;0;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.RangedFloatNode;492;-1827.797,2114.139;Float;False;Property;_Blue150Opacity;Blue150 Opacity;72;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;490;-1611.418,2049.62;Float;False;342;maskBlueChannel;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.Vector2Node;491;-1516.244,2136.559;Float;False;Property;_Blue150ScrollSpeed;Blue150 Scroll Speed;73;1;[PerRendererData];Create;True;0;0;False;0;0,0;0,0;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.WireNode;389;-1624.974,629.0074;Float;False;1;0;SAMPLER2D;;False;1;SAMPLER2D;0
Node;AmplifyShaderEditor.RangedFloatNode;493;-1053.37,1911.471;Float;False;Property;_Blue150IntensityMin;Blue150 Intensity Min;70;1;[PerRendererData];Create;True;0;0;False;0;1;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;585;-1148.528,2008.627;Float;True;MaskingShaderTexture;-1;;57;a40a5c1af537d1843b06044af40c3749;0;9;45;FLOAT2;1,1;False;33;FLOAT;0;False;32;FLOAT;0;False;28;SAMPLER2D;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;27;FLOAT;0;False;26;FLOAT;0;False;25;FLOAT;0;False;24;FLOAT;0;False;23;FLOAT2;0,0;False;2;FLOAT;31;COLOR;0
Node;AmplifyShaderEditor.WireNode;450;-4263.358,1137.948;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;503;-1906.158,1784.824;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;334;-4242.127,-433.2446;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;337;-707.1184,-138.8349;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;447;-1874.826,920.7331;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;501;-4256.156,1733.703;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;335;-3035.506,-396.6802;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;586;-1166.553,627.9725;Float;True;MaskingShaderTexture;-1;;55;a40a5c1af537d1843b06044af40c3749;0;9;45;FLOAT2;1,1;False;33;FLOAT;0;False;32;FLOAT;0;False;28;SAMPLER2D;0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,1;False;27;FLOAT;0;False;26;FLOAT;0;False;25;FLOAT;0;False;24;FLOAT;0;False;23;FLOAT2;0,0;False;2;FLOAT;31;COLOR;0
Node;AmplifyShaderEditor.WireNode;449;-3115.734,1039.884;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;336;-1907.169,-300.4361;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.WireNode;502;-3090.784,1744.529;Float;False;1;0;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;338;-279.2603,-22.51135;Float;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleAddOpNode;499;-227.6256,1488.625;Float;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RegisterLocalVarNode;329;-5870.953,-1009.307;Float;False;maskAlpha;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;446;-223.2522,879.2764;Float;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.GetLocalVarNode;330;327.0976,1012.26;Float;False;329;maskAlpha;1;0;OBJECT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;448;164.561,920.5726;Float;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;140;655.2369,847.8613;Float;False;True;2;Float;ASEMaterialInspector;0;0;Unlit;MaskingShader;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Transparent;;Geometry;ForwardOnly;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0.3;0,0,0,0;VertexScale;True;False;Cylindrical;False;Relative;0;;1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;560;0;559;0
WireConnection;560;1;561;0
WireConnection;560;2;562;0
WireConnection;101;1;560;0
WireConnection;331;0;101;1
WireConnection;301;0;92;0
WireConnection;349;0;347;0
WireConnection;453;0;452;0
WireConnection;302;0;301;0
WireConnection;341;0;101;2
WireConnection;342;0;101;3
WireConnection;354;0;349;0
WireConnection;575;45;157;0
WireConnection;575;33;102;0
WireConnection;575;32;332;0
WireConnection;575;28;302;0
WireConnection;575;27;87;0
WireConnection;575;26;86;0
WireConnection;575;25;84;0
WireConnection;575;23;79;0
WireConnection;303;0;158;0
WireConnection;304;0;48;0
WireConnection;459;0;453;0
WireConnection;576;45;303;0
WireConnection;576;33;575;31
WireConnection;576;32;333;0
WireConnection;576;28;304;0
WireConnection;576;27;35;0
WireConnection;576;26;34;0
WireConnection;576;25;93;0
WireConnection;576;23;43;0
WireConnection;577;45;353;0
WireConnection;577;33;351;0
WireConnection;577;32;355;0
WireConnection;577;28;354;0
WireConnection;577;27;359;0
WireConnection;577;26;357;0
WireConnection;577;25;350;0
WireConnection;577;23;356;0
WireConnection;578;45;464;0
WireConnection;578;33;460;0
WireConnection;578;32;462;0
WireConnection;578;28;459;0
WireConnection;578;27;463;0
WireConnection;578;26;456;0
WireConnection;578;25;458;0
WireConnection;578;23;461;0
WireConnection;309;0;306;0
WireConnection;473;0;455;0
WireConnection;370;0;360;0
WireConnection;474;0;465;0
WireConnection;308;0;307;0
WireConnection;366;0;358;0
WireConnection;483;0;466;0
WireConnection;581;45;473;0
WireConnection;581;33;578;31
WireConnection;581;32;470;0
WireConnection;581;28;474;0
WireConnection;581;27;472;0
WireConnection;581;26;476;0
WireConnection;581;25;475;0
WireConnection;581;23;469;0
WireConnection;580;45;309;0
WireConnection;580;33;576;31
WireConnection;580;32;339;0
WireConnection;580;28;308;0
WireConnection;580;27;310;0
WireConnection;580;26;313;0
WireConnection;580;25;311;0
WireConnection;580;23;312;0
WireConnection;579;45;370;0
WireConnection;579;33;577;31
WireConnection;579;32;369;0
WireConnection;579;28;366;0
WireConnection;579;27;367;0
WireConnection;579;26;371;0
WireConnection;579;25;368;0
WireConnection;579;23;365;0
WireConnection;323;0;316;0
WireConnection;373;0;361;0
WireConnection;318;0;317;0
WireConnection;375;0;362;0
WireConnection;484;0;467;0
WireConnection;388;0;381;0
WireConnection;584;45;373;0
WireConnection;584;33;579;31
WireConnection;584;32;374;0
WireConnection;584;28;375;0
WireConnection;584;27;379;0
WireConnection;584;26;376;0
WireConnection;584;25;378;0
WireConnection;584;23;377;0
WireConnection;583;45;484;0
WireConnection;583;33;581;31
WireConnection;583;32;477;0
WireConnection;583;28;483;0
WireConnection;583;27;479;0
WireConnection;583;26;478;0
WireConnection;583;25;480;0
WireConnection;583;23;481;0
WireConnection;582;45;323;0
WireConnection;582;33;580;31
WireConnection;582;32;340;0
WireConnection;582;28;318;0
WireConnection;582;27;319;0
WireConnection;582;26;321;0
WireConnection;582;25;320;0
WireConnection;582;23;322;0
WireConnection;488;0;486;0
WireConnection;500;0;578;0
WireConnection;489;0;485;0
WireConnection;389;0;380;0
WireConnection;585;45;488;0
WireConnection;585;33;583;31
WireConnection;585;32;490;0
WireConnection;585;28;489;0
WireConnection;585;27;494;0
WireConnection;585;26;493;0
WireConnection;585;25;492;0
WireConnection;585;23;491;0
WireConnection;450;0;577;0
WireConnection;503;0;583;0
WireConnection;334;0;575;0
WireConnection;337;0;582;0
WireConnection;447;0;584;0
WireConnection;501;0;500;0
WireConnection;335;0;576;0
WireConnection;586;45;388;0
WireConnection;586;33;584;31
WireConnection;586;32;384;0
WireConnection;586;28;389;0
WireConnection;586;27;382;0
WireConnection;586;26;385;0
WireConnection;586;25;383;0
WireConnection;586;23;386;0
WireConnection;449;0;579;0
WireConnection;336;0;580;0
WireConnection;502;0;581;0
WireConnection;338;0;337;0
WireConnection;338;1;336;0
WireConnection;338;2;335;0
WireConnection;338;3;334;0
WireConnection;499;0;501;0
WireConnection;499;1;502;0
WireConnection;499;2;503;0
WireConnection;499;3;585;0
WireConnection;329;0;101;4
WireConnection;446;0;586;0
WireConnection;446;1;447;0
WireConnection;446;2;449;0
WireConnection;446;3;450;0
WireConnection;448;0;338;0
WireConnection;448;1;446;0
WireConnection;448;2;499;0
WireConnection;140;2;448;0
WireConnection;140;10;330;0
ASEEND*/
//CHKSM=965AB691F57C18D620C06C5BE8E564D6E59C0EB0