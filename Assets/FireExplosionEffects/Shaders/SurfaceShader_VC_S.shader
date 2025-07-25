Shader "Custom/SurfaceShader_VC-S" {
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Albedo (RGB)", 2D) = "white" {}
		_Normal("Normap Map", 2D) = "bump" {}
		[Toggle(FASTSOFTPARTICLE_ON)] FASTSOFTPARTICLE_ON("Fast Softparticle Enable", Float) = 0
	}
		SubShader{
		Tags{ "Queue" = "Transparent" "RenderType" = "Transparent" }
		LOD 200
		Blend One OneMinusSrcAlpha

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard fullforwardshadows vertex:vert alpha:fade
#pragma multi_compile __ FASTSOFTPARTICLE_ON

		// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

	sampler2D _MainTex;
	sampler2D _Normal;

	struct Input {
		float2 uv_MainTex;
		float4 vertex : SV_POSITION;
		float4 color : COLOR;
#ifdef FASTSOFTPARTICLE_ON
		float depth;
#endif
	};

	void vert(inout appdata_full v, out Input o)
	{
		UNITY_INITIALIZE_OUTPUT(Input, o);
		o.color = v.color;
#ifdef FASTSOFTPARTICLE_ON
		float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
		o.depth = dot(v.texcoord1, float4(worldPos, 1));
#endif
	}

	fixed4 _Color;

	void surf(Input IN, inout SurfaceOutputStandard o) {
		// Albedo comes from a texture tinted by color
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
		o.Albedo = c.rgb*IN.color;
		o.Normal = UnpackNormal(tex2D(_Normal, IN.uv_MainTex));
		o.Alpha = c.a*IN.color.a;
#ifdef FASTSOFTPARTICLE_ON
		o.Alpha *= saturate(IN.depth);
#endif
	}
	ENDCG
	}
		FallBack "Diffuse"
}