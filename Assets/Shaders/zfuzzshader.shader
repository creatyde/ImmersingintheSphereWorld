Shader "Custom/zshader2" {
	Properties{
		_Color("Color", Color) = (0,0,0,0) // blind depth marker
		_Color2("Color 2", Color) = (1.0, 1.0, 1.0, 1.0) // outline color
		_Outline("Outline Length", float) = 0.05 // for smooth ball
		_RelevantDistance("Relevant Distance", float) = 5.0 // amount of depth biasing
	}
		SubShader{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }
		LOD 200

		Pass{

		Blend SrcAlpha OneMinusSrcAlpha

		Cull Front
		ZWrite On

		CGPROGRAM

#pragma vertex vert
#pragma fragment frag

		float4 _Color;

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
		// Ztest LEqual

		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM

#include "UnityCG.cginc" // for ComputeScreenPos
#pragma vertex vert
#pragma fragment frag

	float _Outline;
	float4 _Color2;
	uniform float _RelevantDistance;
	uniform sampler2D _CameraDepthTexture; //Depth Texture

	struct appdata {
		float4 vertex : POSITION;
		float3 normal : NORMAL;
	};

	struct v2f {
		float4 pos : SV_POSITION;
		float4 projPos : TEXCOORD1; // data
	};

	v2f vert(appdata v) {
		v2f o;

		float4 vert = v.vertex;
		vert.xyz += v.normal * _Outline; // extend outward
		o.pos = UnityObjectToClipPos(vert);
		o.projPos = ComputeScreenPos(o.pos); // problem

		return o;
	}

	half4 frag(v2f i) : COLOR{
		// https://answers.unity.com/questions/462500/get-depth-in-material-shader.html
		float depth = Linear01Depth(tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)).r);
		// map inverseDist to [0,1]
		float alpha = atan(depth / _RelevantDistance) / 1.570796325;
		// amount of black
		return half4(0, 0, 0, alpha);
	}

		ENDCG
	}

	}
		FallBack "Diffuse"
}