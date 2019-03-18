// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "uTerrains/Triplanar/Diffuse" {
	Properties {
		_TopTexture ("Top Texture", 2D) = "white" {}
		_SidesTexture ("Sides Texture", 2D) = "white" {}
		_RedTexture ("Red Channel Texture", 2D) = "white" {}
		_GreenTexture ("Green Channel Texture", 2D) = "white" {}
		_BlueTexture ("Blue Channel Texture", 2D) = "white" {}
		_AlphaTexture ("Alpha Channel Texture", 2D) = "white" {}

		_TexPower("Global Texture Power", Float) = 10
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Lambert vertex:vert
		
		sampler2D _TopTexture;
		float4 _TopTexture_ST;
		
		sampler2D _SidesTexture;
		float4 _SidesTexture_ST;
		
		sampler2D _RedTexture;
		float4 _RedTexture_ST;
		
		sampler2D _GreenTexture;
		float4 _GreenTexture_ST;
		
		sampler2D _BlueTexture;
		float4 _BlueTexture_ST;
		
		sampler2D _AlphaTexture;
		float4 _AlphaTexture_ST;
		
		half _TexPower;

		struct Input {			
			fixed4 blendPower;
			float3 worldPos;
			float4 color;
		};
		
		void vert (inout appdata_full v, out Input o) {
		
			UNITY_INITIALIZE_OUTPUT(Input,o);
						
			fixed3 worldNormal = normalize(mul(unity_ObjectToWorld, fixed4(v.normal, 0)).xyz);
			
			o.blendPower.x = worldNormal.x;
			o.blendPower.y = max(worldNormal.y, 0);
			o.blendPower.z = worldNormal.z;
			o.blendPower.w = min(worldNormal.y, 0);
			o.blendPower = pow(abs(o.blendPower), _TexPower);
			o.blendPower = o.blendPower / (o.blendPower.x + o.blendPower.y + o.blendPower.z + o.blendPower.w);
			
			o.color = v.color;
		}
		
		void surf (Input IN, inout SurfaceOutput o) {
			
			float2 topUV_x = (IN.worldPos.zy * _TopTexture_ST.xy + _TopTexture_ST.zw); // X-projection
			float2 topUV_y = (IN.worldPos.xz * _TopTexture_ST.xy + _TopTexture_ST.zw); // Y-projeciton
			float2 topUV_z = (IN.worldPos.xy * _TopTexture_ST.xy + _TopTexture_ST.zw); // Z-projection
			
			float2 sidesUV_x = (IN.worldPos.zy * _SidesTexture_ST.xy + _SidesTexture_ST.zw); // X-projection
			float2 sidesUV_y = (IN.worldPos.xz * _SidesTexture_ST.xy + _SidesTexture_ST.zw); // Y-projeciton
			float2 sidesUV_z = (IN.worldPos.xy * _SidesTexture_ST.xy + _SidesTexture_ST.zw); // Z-projection
			
			float2 redUV_x = (IN.worldPos.zy * _RedTexture_ST.xy + _RedTexture_ST.zw); // X-projection
			float2 redUV_y = (IN.worldPos.xz * _RedTexture_ST.xy + _RedTexture_ST.zw); // Y-projeciton
			float2 redUV_z = (IN.worldPos.xy * _RedTexture_ST.xy + _RedTexture_ST.zw); // Z-projection
			
			float2 greenUV_x = (IN.worldPos.zy * _GreenTexture_ST.xy + _GreenTexture_ST.zw); // X-projection
			float2 greenUV_y = (IN.worldPos.xz * _GreenTexture_ST.xy + _GreenTexture_ST.zw); // Y-projeciton
			float2 greenUV_z = (IN.worldPos.xy * _GreenTexture_ST.xy + _GreenTexture_ST.zw); // Z-projection
			
			float2 blueUV_x = (IN.worldPos.zy * _BlueTexture_ST.xy + _BlueTexture_ST.zw); // X-projection
			float2 blueUV_y = (IN.worldPos.xz * _BlueTexture_ST.xy + _BlueTexture_ST.zw); // Y-projeciton
			float2 blueUV_z = (IN.worldPos.xy * _BlueTexture_ST.xy + _BlueTexture_ST.zw); // Z-projection
			
			float2 alphaUV_x = (IN.worldPos.zy * _AlphaTexture_ST.xy + _AlphaTexture_ST.zw); // X-projection
			float2 alphaUV_y = (IN.worldPos.xz * _AlphaTexture_ST.xy + _AlphaTexture_ST.zw); // Y-projeciton
			float2 alphaUV_z = (IN.worldPos.xy * _AlphaTexture_ST.xy + _AlphaTexture_ST.zw); // Z-projection
			
			// Get colors
			half4 t1,t2,t3,t4,t1_,t2_,t3_,t4_;
			half blend_;
			t1 = tex2D(_SidesTexture, sidesUV_x).xyzw;
			t2 = tex2D(_TopTexture,   topUV_y  ).xyzw;
			t3 = tex2D(_SidesTexture, sidesUV_z).xyzw;
			t4 = tex2D(_SidesTexture, sidesUV_y).xyzw;
			
			// Red channel
			t1_ = tex2D(_RedTexture, redUV_x).xyzw;
			t2_ = tex2D(_RedTexture, redUV_y).xyzw;
			t3_ = tex2D(_RedTexture, redUV_z).xyzw;
			t4_ = tex2D(_RedTexture, redUV_y).xyzw;
			blend_ = pow((1.5 * IN.color.r), 1.5);
			blend_ = clamp(blend_, 0.0, 1.0);
			t1 = lerp(t1, t1_, blend_);
			t2 = lerp(t2, t2_, blend_);
			t3 = lerp(t3, t3_, blend_);
			t4 = lerp(t4, t4_, blend_);
			
			// Green channel
			t1_ = tex2D(_GreenTexture, greenUV_x).xyzw;
			t2_ = tex2D(_GreenTexture, greenUV_y).xyzw;
			t3_ = tex2D(_GreenTexture, greenUV_z).xyzw;
			t4_ = tex2D(_GreenTexture, greenUV_y).xyzw;
			blend_ = pow((1.5 * IN.color.g), 1.5);
			blend_ = clamp(blend_, 0.0, 1.0);
			t1 = lerp(t1, t1_, blend_);
			t2 = lerp(t2, t2_, blend_);
			t3 = lerp(t3, t3_, blend_);
			t4 = lerp(t4, t4_, blend_);
			
			// Blue channel
			t1_ = tex2D(_BlueTexture, blueUV_x).xyzw;
			t2_ = tex2D(_BlueTexture, blueUV_y).xyzw;
			t3_ = tex2D(_BlueTexture, blueUV_z).xyzw;
			t4_ = tex2D(_BlueTexture, blueUV_y).xyzw;
			blend_ = pow((1.5 * IN.color.b), 1.5);
			blend_ = clamp(blend_, 0.0, 1.0);
			t1 = lerp(t1, t1_, blend_);
			t2 = lerp(t2, t2_, blend_);
			t3 = lerp(t3, t3_, blend_);
			t4 = lerp(t4, t4_, blend_);
			
			// Alpha channel
			t1_ = tex2D(_AlphaTexture, alphaUV_x).xyzw;
			t2_ = tex2D(_AlphaTexture, alphaUV_y).xyzw;
			t3_ = tex2D(_AlphaTexture, alphaUV_z).xyzw;
			t4_ = tex2D(_AlphaTexture, alphaUV_y).xyzw;
			blend_ = pow((1.5 * IN.color.a), 1.5);
			blend_ = clamp(blend_, 0.0, 1.0);
			t1 = lerp(t1, t1_, blend_);
			t2 = lerp(t2, t2_, blend_);
			t3 = lerp(t3, t3_, blend_);
			t4 = lerp(t4, t4_, blend_);
			
			// Combine all
			fixed4 tex = t1 * IN.blendPower.x + t2 * IN.blendPower.y + t3 * IN.blendPower.z + t4 * IN.blendPower.w;

			o.Albedo = tex.rgb;
			o.Alpha = tex.a;
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
