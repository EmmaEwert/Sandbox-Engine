#ifndef PIPELINE_UNLIT_INCLUDED
#define PIPELINE_UNLIT_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerFrame)
	float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
	float4x4 unity_ObjectToWorld;
CBUFFER_END

#define UNITY_MATRIX_M unity_ObjectToWorld

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

UNITY_INSTANCING_BUFFER_START(PerInstance)
	UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(PerInstance)

sampler2D _MainTex;

struct VertexInput {
	float4 pos : POSITION;
	float3 normal : NORMAL;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
	float4 clipPos : SV_POSITION;
	float3 normal : TEXCOORD0;
	float3 objPos : TEXCOORD2;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

VertexOutput UnlitPassVertex(VertexInput input) {
	VertexOutput output;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);
	float4 worldPos = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0));
	output.clipPos = mul(unity_MatrixVP, worldPos);
	output.normal = mul((float3x3)UNITY_MATRIX_M, input.normal);
	output.objPos = input.pos.xyz;
	return output;
}

float4 UnlitPassFragment(VertexOutput input) : SV_TARGET {
	UNITY_SETUP_INSTANCE_ID(input);
	float3 albedo = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color).rgb;
	float2 uv = input.normal.x * input.objPos.zy + input.normal.y * input.objPos.xz + input.normal.z * input.objPos.xy;
	float3 color = albedo * tex2D(_MainTex, uv).rgb;
	return float4(color, 1);
}

#endif // PIPELINE_UNLIT_INCLUDED