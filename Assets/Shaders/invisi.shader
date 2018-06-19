Shader "Custom/invisi" {
	Properties{
		// _MainTex("Main Texture",2D) = "white"{}
		_Color("Color", Color) = (0,0,0,0) // blind depth marker
	}
		SubShader{
		Tags{ "RenderType" = "AlphaTest" "Queue" = "Transparent" }
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
	}

		FallBack "Diffuse"
}