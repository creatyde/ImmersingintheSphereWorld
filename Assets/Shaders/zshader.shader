Shader "Custom/zshader" {
	Properties{
		// _MainTex("Main Texture",2D) = "white"{}
		_Color("Color", Color) = (0,0,0,0) // blind depth marker
		_Color2("Color 2", Color) = (0.0, 0.0, 0.0, 1.0) // outline color
		_Outline("Outline Length", float) = 0.07 // for smooth sphere
	}
		SubShader{
		Tags{ "RenderType" = "AlphaTest" "Queue" = "AlphaTest" }
		LOD 200
		
		
		Pass{

		Blend SrcAlpha OneMinusSrcAlpha

		Cull Front
		ZWrite On

		CGPROGRAM

#pragma vertex vert
#pragma fragment frag

	float4 _Color;
	float _Outline;

	struct appdata {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f {
		float4 pos : SV_POSITION;
	};

	// first, make zdepth blind draw
	v2f vert(appdata v) {
		v2f o;

		float4 vert = v.vertex;
		//vert.xyz -= v.normal * _Outline; // extend inqES

		o.pos = UnityObjectToClipPos(vert);

		return o;
	}

	half4 frag(v2f i) : COLOR{
		return _Color;
	}

		ENDCG
	}
	

		// draw outline using zdepth info
		Pass{

		Cull Front
		Ztest LEqual

		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM

#pragma vertex vert
#pragma fragment frag

	float _Outline;
	float4 _Color2;

	struct appdata {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f {
		float4 pos : SV_POSITION;
	};

	v2f vert(appdata v) {
		v2f o;

		float4 vert = v.vertex;
		vert.xyz += v.normal * _Outline; // extend outward

		o.pos = UnityObjectToClipPos(vert);

		return o;
	}

	half4 frag(v2f i) : COLOR{
		return _Color2;
	}

		ENDCG
	}

}
		FallBack "Diffuse"
}