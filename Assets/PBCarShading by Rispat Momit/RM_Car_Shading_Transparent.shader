Shader "RM Shaders/ Car Shading Transparent" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_SpecTex ("Specular Map (A)", 2D) = "white" {}
		_BumpMap ("Bumpmap", 2D) = "bump" {}
        _AOTexture ("AO Texture (A)", 2D) = "white" {}
        
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.1
		
		_RimColor ("Rim Color", Color) = (0.26,0.19,0.16,0.0)
        _RimPower ("Rim Power", Range(-1,1)) = 0.5
		_RimLight ("Rim Light", Range(0,10)) = 1.0

		_Cube ("Cubemap", CUBE) = "" {}

	}
	SubShader {
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}

		LOD 200

		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows alpha

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _AOTexture;
        sampler2D _SpecTex;
        
		float4 _RimColor;
      	float _RimPower;
      	float _RimLight;
      	samplerCUBE _Cube;

		struct Input {
			float2 uv_MainTex;
			float2 uv_SpecTex;
			float2 uv_AOTexture;
			float2 uv_BumpMap;
            float3 worldRefl;
			float3 viewDir;
			INTERNAL_DATA
            
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		void surf (Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			
			fixed4 ao = tex2D (_AOTexture, IN.uv_AOTexture);
						
			fixed4 sp = tex2D (_SpecTex, IN.uv_SpecTex);


			o.Albedo = c.rgb*ao.a;
			
			o.Normal = UnpackNormal (tex2D (_BumpMap,  IN.uv_BumpMap));
			
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic*sp.a;
			o.Smoothness = _Glossiness*sp.a;
			
			
			
			
			float3 worldRefl = WorldReflectionVector (IN,o.Normal);
	        fixed4 reflcol = texCUBE (_Cube, worldRefl);
			
            half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
            
            o.Albedo +=  reflcol.rgb*_RimColor.rgb * pow (rim*ao.a, _RimPower)*c.a*ao.a*_RimLight;
         
         o.Alpha = c.a*rim*_RimPower;
		
		}
		ENDCG
	} 
	FallBack "Diffuse"
}
