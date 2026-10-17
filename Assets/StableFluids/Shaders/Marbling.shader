Shader "Hidden/StableFluids/Marbling"
{
	HLSLINCLUDE

	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"

	// struct Command
	// {
		// 	float4 color;
		// 	float2 position;
		// 	float2 force;
		// 	float falloff;
		// }

		float4 _Color;
		float2 _Origin;
		float2 _Force;
		float _Falloff;

		float _Aspect;

		// Position-based amplitude with peak at _Origin and falloff with _Falloff
		float GetAmplitude(float2 uv)
		{
			float2 pos = uv - 0.5;
			pos.y /= _Aspect;
			return exp(-_Falloff * distance(_Origin, pos));
		}

		void VertexProcedural(uint vertexID : SV_VertexID,	out float4 positionCS : SV_POSITION,	out float2 uv : TEXCOORD0)
		{
			positionCS = GetFullScreenTriangleVertexPosition(vertexID);
			uv = GetFullScreenTriangleTexCoord(vertexID);
		}

		half4 FragmentInjection(float4 positionCS : SV_POSITION, float2 uv : TEXCOORD0) : SV_Target
		{
			float alpha = _Color.a * saturate(100 * GetAmplitude(uv));
			return float4(_Color.rgb, alpha);
		}

		half4 FragmentForce(float4 positionCS : SV_POSITION, float2 uv : TEXCOORD0) : SV_Target
		{
			return half4(_Force * GetAmplitude(uv), 0, 1);
		}

		ENDHLSL

		SubShader
		{
			Cull Off ZWrite Off ZTest Always

			// Pass 0: Color Injection
			Pass
			{
				Name "Color Injection"
				HLSLPROGRAM
				#pragma vertex VertexProcedural
				#pragma fragment FragmentInjection
				ENDHLSL
			}

			// Pass 1: Force Application
			Pass
			{
				Name "Force Injection"
				HLSLPROGRAM
				#pragma vertex VertexProcedural
				#pragma fragment FragmentForce
				ENDHLSL
			}
		}
	}