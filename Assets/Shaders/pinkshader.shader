Shader "Custom/pinkshader" {
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		//_Emission("Emission", Color) = (1,1,1,1)
		_MainTex ("Color (RGB) Alpha (A)", 2D) = "white" {}
		_Glossiness("Smoothness", Range(0,1)) = 0.5
		_Metallic("Metallic", Range(0,1)) = 0.0
		_LineColor("LineColor", Color) = (1.0, 1.0, 1.0, 1.0) // outline color
		_Outline("Outline Length", float) = 0.05 // for smooth sphere
	}
		SubShader{
			Tags { "RenderType" = "Transparent" "Queue" = "Transparent" }
			LOD 200
			
			CGPROGRAM
			// Physically based Standard lighting model, and enable shadows on all light types
#pragma surface surf Standard fullforwardshadows alpha
			// Use shader model 3.0 target, to get nicer looking lighting
#pragma target 3.0

			sampler2D _MainTex;

		struct Input {
			float2 uv_MainTex;
		};

		half _Glossiness;
		half _Metallic;
		fixed4 _Color;
		//fixed4 _Emission;

		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
			// put more per-instance properties here
			UNITY_INSTANCING_BUFFER_END(Props)

			void surf(Input IN, inout SurfaceOutputStandard o) {
			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
			o.Albedo = c.rgb;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
			// o.Emission = _Emission;
		}
		ENDCG

			Pass{
				Blend SrcAlpha OneMinusSrcAlpha
				Cull Front
				ZWrite On

				CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag alpha

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
					o.pos = UnityObjectToClipPos(vert);
					return o;
				}

				half4 frag(v2f i) : COLOR{
					return _Color + float4(.07, .07, .07,.07); // som
				}

				ENDCG
			}


		// draw outline using zdepth info
		Pass{
			Cull Front
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			float _Outline;
			float4 _LineColor;

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
				return _LineColor;
			}
				
			ENDCG
		}
	}
}
