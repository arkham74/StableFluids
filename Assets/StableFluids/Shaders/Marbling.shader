Shader "Hidden/StableFluids/Marbling"
{
	HLSLINCLUDE

	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
	#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
	#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl"

	struct MarblingCommand
	{
		float4 color;
		float2 position;
		float2 force;
		float falloff;
	};

	// float4 _Color;
	// float2 _Origin;
	// float2 _Force;
	// float _Falloff;

	StructuredBuffer<MarblingCommand> _CommandsBuffer;
	int _CommandsCount;
	float _Aspect;

	void VertexProcedural(uint vertexID : SV_VertexID,	out float4 positionCS : SV_POSITION,	out float2 uv : TEXCOORD0)
	{
		positionCS = GetFullScreenTriangleVertexPosition(vertexID);
		uv = GetFullScreenTriangleTexCoord(vertexID);
	}

	// Position-based amplitude with peak at _Origin and falloff with _Falloff
	float GetAmplitude(float2 uv, float2 origin, float falloff)
	{
		float2 pos = uv - 0.5;
		pos.y /= _Aspect;
		return exp(-falloff * distance(origin, pos));
	}

	float4 FragmentInjection(float4 positionCS : SV_POSITION, float2 uv : TEXCOORD0) : SV_Target
	{
		float4 sum = 0;
		for(int i = 0; i < _CommandsCount; i++)
		{
			MarblingCommand cmd = _CommandsBuffer[i];
			sum += cmd.color * saturate(100 * GetAmplitude(uv, cmd.position, cmd.falloff));
		}
		return sum;
	}

	float2 FragmentForce(float4 positionCS : SV_POSITION, float2 uv : TEXCOORD0) : SV_Target
	{
		float2 sum = 0;
		for(int i = 0; i < _CommandsCount; i++)
		{
			MarblingCommand cmd = _CommandsBuffer[i];
			sum += cmd.force * GetAmplitude(uv, cmd.position, cmd.falloff);
		}
		return sum;
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