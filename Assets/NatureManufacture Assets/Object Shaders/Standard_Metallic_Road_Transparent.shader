Shader "NatureManufacture Shaders/Road/Standard_Metallic_Road_Transparent"
{
	Properties
	{
		_AlphaNoiseTilling("Alpha Noise Tilling", Vector) = (0,0,0,0)
		_Color("Color", Color) = (1,1,1,1)
		_MainRoadBrightness("Main Road Brightness", Float) = 1
		_MainTex("Main Road Albedo_T", 2D) = "white" {}
		_AlphaNoisePower("Alpha Noise Power", Range( 0 , 5)) = 0
		_BumpMap("Main Road Normal", 2D) = "bump" {}
		_BumpScale("Main Road BumpScale", Range( 0 , 5)) = 0
		_MetalicRAmbientOcclusionGHeightBEmissionA("Main Road Metallic (R) Ambient Occlusion (G) Height (B) Smoothness (A)", 2D) = "white" {}
		_MainRoadMetalicPower("Main Road Metalic Power", Range( 0 , 2)) = 0
		_MainRoadAmbientOcclusionPower("Main Road Ambient Occlusion Power", Range( 0 , 1)) = 1
		_MainRoadSmoothnessPower("Main Road Smoothness Power", Range( 0 , 2)) = 1
		_MainRoadParallaxPower("Main Road Parallax Power", Range( -0.1 , 0.1)) = 0
		_DetailMask("DetailMask (A)", 2D) = "white" {}
		_DetailAlbedoMap("DetailAlbedoMap", 2D) = "black" {}
		_DetailAlbedoPower("Main Road Detail Albedo Power", Range( 0 , 2)) = 0
		_DetailNormalMap("DetailNormalMap", 2D) = "bump" {}
		_DetailNormalMapScale("Main Road DetailNormalMapScale", Range( 0 , 5)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
		[Header(Forward Rendering Options)]
		[ToggleOff] _SpecularHighlights("Specular Highlights", Float) = 1.0
		[ToggleOff] _GlossyReflections("Reflections", Float) = 1.0
	}

	SubShader
	{
		Tags{ "RenderType" = "Custom"  "Queue" = "AlphaTest+0" "Offset"="-2, -2" "ForceNoShadowCasting"="True" }
		Cull Off
		ZTest LEqual
		Offset  -3 , 0
		CGPROGRAM
		#include "UnityStandardUtils.cginc"
		#pragma target 3.0
		#pragma shader_feature _SPECULARHIGHLIGHTS_OFF
		#pragma shader_feature _GLOSSYREFLECTIONS_OFF
		#pragma multi_compile_fog
		#include "UnityPBSLighting.cginc"
		#pragma exclude_renderers gles
		#pragma surface surf Standard keepalpha  decal:blend
		struct Input
		{
			half2 uv_texcoord;
			float3 viewDir;
			INTERNAL_DATA
			float4 vertexColor : COLOR;
		};

		uniform half _BumpScale;
		uniform sampler2D _BumpMap;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform float4 _MetalicRAmbientOcclusionGHeightBEmissionA_ST;
		uniform half _MainRoadParallaxPower;
		uniform half _DetailNormalMapScale;
		uniform sampler2D _DetailNormalMap;
		uniform sampler2D _DetailAlbedoMap;
		uniform float4 _DetailAlbedoMap_ST;
		uniform sampler2D _DetailMask;
		uniform float4 _DetailMask_ST;
		uniform half _MainRoadBrightness;
		uniform half4 _Color;
		uniform half _DetailAlbedoPower;
		uniform sampler2D _MetalicRAmbientOcclusionGHeightBEmissionA;
		uniform half _MainRoadMetalicPower;
		uniform half _MainRoadSmoothnessPower;
		uniform half _MainRoadAmbientOcclusionPower;
		uniform half2 _AlphaNoiseTilling;
		uniform half _AlphaNoisePower;


		float3 mod2D289( float3 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float2 mod2D289( float2 x ) { return x - floor( x * ( 1.0 / 289.0 ) ) * 289.0; }

		float3 permute( float3 x ) { return mod2D289( ( ( x * 34.0 ) + 1.0 ) * x ); }

		float snoise( float2 v )
		{
			const float4 C = float4( 0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439 );
			float2 i = floor( v + dot( v, C.yy ) );
			float2 x0 = v - i + dot( i, C.xx );
			float2 i1;
			i1 = ( x0.x > x0.y ) ? float2( 1.0, 0.0 ) : float2( 0.0, 1.0 );
			float4 x12 = x0.xyxy + C.xxzz;
			x12.xy -= i1;
			i = mod2D289( i );
			float3 p = permute( permute( i.y + float3( 0.0, i1.y, 1.0 ) ) + i.x + float3( 0.0, i1.x, 1.0 ) );
			float3 m = max( 0.5 - float3( dot( x0, x0 ), dot( x12.xy, x12.xy ), dot( x12.zw, x12.zw ) ), 0.0 );
			m = m * m;
			m = m * m;
			float3 x = 2.0 * frac( p * C.www ) - 1.0;
			float3 h = abs( x ) - 0.5;
			float3 ox = floor( x + 0.5 );
			float3 a0 = x - ox;
			m *= 1.79284291400159 - 0.85373472095314 * ( a0 * a0 + h * h );
			float3 g;
			g.x = a0.x * x0.x + h.x * x0.y;
			g.yz = a0.yz * x12.xz + h.yz * x12.yw;
			return 130.0 * dot( m, g );
		}


		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			float2 uv_MetalicRAmbientOcclusionGHeightBEmissionA = i.uv_texcoord * _MetalicRAmbientOcclusionGHeightBEmissionA_ST.xy + _MetalicRAmbientOcclusionGHeightBEmissionA_ST.zw;
			float2 Offset710 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, uv_MetalicRAmbientOcclusionGHeightBEmissionA ).b - 1 ) * i.viewDir.xy * _MainRoadParallaxPower ) + uv_MainTex;
			float2 Offset728 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset710 ).b - 1 ) * i.viewDir.xy * _MainRoadParallaxPower ) + Offset710;
			float2 Offset754 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset728 ).b - 1 ) * i.viewDir.xy * _MainRoadParallaxPower ) + Offset728;
			float2 Offset778 = ( ( tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset754 ).b - 1 ) * i.viewDir.xy * _MainRoadParallaxPower ) + Offset754;
			half3 tex2DNode4 = UnpackScaleNormal( tex2D( _BumpMap, Offset778 ), _BumpScale );
			float2 uv_DetailAlbedoMap = i.uv_texcoord * _DetailAlbedoMap_ST.xy + _DetailAlbedoMap_ST.zw;
			float2 uv_DetailMask = i.uv_texcoord * _DetailMask_ST.xy + _DetailMask_ST.zw;
			half4 tex2DNode481 = tex2D( _DetailMask, uv_DetailMask );
			float3 lerpResult479 = lerp( tex2DNode4 , BlendNormals( tex2DNode4 , UnpackScaleNormal( tex2D( _DetailNormalMap, uv_DetailAlbedoMap ), _DetailNormalMapScale ) ) , tex2DNode481.a);
			o.Normal = lerpResult479;
			half4 tex2DNode1 = tex2D( _MainTex, Offset778 );
			float4 temp_output_77_0 = ( ( _MainRoadBrightness * tex2DNode1 ) * _Color );
			half4 blendOpSrc474 = temp_output_77_0;
			half4 blendOpDest474 = ( _DetailAlbedoPower * tex2D( _DetailAlbedoMap, uv_DetailAlbedoMap ) );
			float4 lerpResult480 = lerp( temp_output_77_0 , (( blendOpDest474 > 0.5 ) ? ( 1.0 - ( 1.0 - 2.0 * ( blendOpDest474 - 0.5 ) ) * ( 1.0 - blendOpSrc474 ) ) : ( 2.0 * blendOpDest474 * blendOpSrc474 ) ) , ( _DetailAlbedoPower * tex2DNode481.a ));
			o.Albedo = lerpResult480.rgb;
			half4 tex2DNode2 = tex2D( _MetalicRAmbientOcclusionGHeightBEmissionA, Offset778 );
			o.Metallic = ( tex2DNode2.r * _MainRoadMetalicPower );
			o.Smoothness = ( tex2DNode2.a * _MainRoadSmoothnessPower );
			float clampResult96 = clamp( tex2DNode2.g , ( 1.0 - _MainRoadAmbientOcclusionPower ) , 1.0 );
			o.Occlusion = clampResult96;
			float2 uv_TexCoord793 = i.uv_texcoord * _AlphaNoiseTilling;
			float simplePerlin2D779 = snoise( uv_TexCoord793 );
			float temp_output_629_0 = ( tex2DNode1.a * _Color.a );
			float clampResult788 = clamp( ( ( ( simplePerlin2D779 * _AlphaNoisePower ) * temp_output_629_0 ) + temp_output_629_0 ) , 0.0 , 1.0 );
			o.Alpha = ( clampResult788 * i.vertexColor.a );
		}

		ENDCG
	}
	Fallback "Diffuse"
}