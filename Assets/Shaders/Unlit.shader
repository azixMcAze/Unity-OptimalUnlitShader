Shader "Unlit/Unlit"
{
	Properties
	{
		[Enum(Opaque, 0, Cutout, 1, Transparent, 2)] _RenderingMode("Rendering mode", Int) = 0

		_MainTex ("Texture", 2D) = "white" {}
		_Mask ("Mask", 2D) = "white" {}
		_Color ("Color", color) = (1, 1, 1, 1)
		_Cutoff("Alpha Cutoff", Range(0.0, 1.0)) = 0.5

		[HideInInspector] _SrcBlend ("__src", Float) = 1.0
		[HideInInspector] _DstBlend ("__dst", Float) = 0.0
		[HideInInspector] _ZWrite ("__zw", Float) = 1.0
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Blend [_SrcBlend] [_DstBlend]
		ZWrite [_ZWrite]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			#pragma shader_feature _ _TEXTURE_OFF
			#pragma shader_feature _ _MASK_OFF
			#pragma shader_feature _ _COLOR_OFF
			#pragma shader_feature _ _ALPHATEST_ON
			#include "UnityCG.cginc"

			#define CONCAT(A, B) A ## B

			#if !defined(_TEXTURE_OFF) || !defined(_MASK_OFF)
				#define UV1_TEXCOORD TEXCOORD0
				#define FOG_TEXCOORD 1
				#if !defined(_TEXTURE_OFF)
					#define UV1_SCALE_OFFSET _MainTex
				#else
					#define UV1_SCALE_OFFSET _Mask
				#endif
			#endif

			struct appdata
			{
				float4 vertex : POSITION;
			#if defined(UV1_TEXCOORD)
				float2 uv : TEXCOORD0;
			#endif
			};

			struct v2f
			{
			#if defined(UV1_TEXCOORD)
				float2 uv1 : UV1_TEXCOORD;
			#endif
				UNITY_FOG_COORDS(FOG_TEXCOORD)
				float4 vertex : SV_POSITION;
			};

		#if !defined(_TEXTURE_OFF)
			sampler2D _MainTex;
		#endif
		#if defined(UV1_SCALE_OFFSET)
			float4 CONCAT(UV1_SCALE_OFFSET, _ST);
		#endif
		#if !defined(_MASK_OFF)
			sampler2D _Mask;
		#endif
		#if !defined(_COLOR_OFF)
			fixed4 _Color;
		#endif
		#if defined(_ALPHATEST_ON)
			fixed _Cutoff;
		#endif
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
			#if defined(UV1_TEXCOORD)
				o.uv1 = TRANSFORM_TEX(v.uv, UV1_SCALE_OFFSET);
			#endif
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = 1;
			#if !defined(_TEXTURE_OFF)
				col *= tex2D(_MainTex, i.uv1);
			#endif
			#if !defined(_MASK_OFF)
				col.a *= tex2D(_Mask, i.uv1).a;
			#endif
			#if !defined(_COLOR_OFF)
				col *= _Color;
			#endif
			#if defined(_ALPHATEST_ON)
				clip(col.a - _Cutoff);
			#endif
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
	CustomEditor "UnlitShaderGUI"
}
