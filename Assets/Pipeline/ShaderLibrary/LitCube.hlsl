#ifndef PIPELINE_LIT_CUBE_INCLUDED
#define PIPELINE_LIT_CUBE_INCLUDED

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"

CBUFFER_START(UnityPerFrame)
	float4x4 unity_MatrixVP;
CBUFFER_END

CBUFFER_START(UnityPerDraw)
	float4x4 unity_ObjectToWorld;
	float4 unity_LightIndicesOffsetAndCount;
	float4 unity_4LightIndices0;
	float4 unity_4LightIndices1;
CBUFFER_END

#define MAX_VISIBLE_LIGHTS 64

CBUFFER_START(_LightBuffer)
	float4 _VisibleLightColors[MAX_VISIBLE_LIGHTS];
	float4 _VisibleLightDirectionsOrPositions[MAX_VISIBLE_LIGHTS];
	float4 _VisibleLightAttenuations[MAX_VISIBLE_LIGHTS];
	float4 _VisibleLightSpotDirections[MAX_VISIBLE_LIGHTS];
CBUFFER_END

float3 DiffuseLight(int index, float3 normal, float3 worldPos) {
	float3 lightColor = _VisibleLightColors[index].rgb;
	float4 lightPositionOrDirection = _VisibleLightDirectionsOrPositions[index];
	float4 lightAttenuation = _VisibleLightAttenuations[index];
	float3 spotDirection = _VisibleLightSpotDirections[index].xyz;

	float3 lightVector = lightPositionOrDirection.xyz - worldPos * lightPositionOrDirection.w;
	float3 lightDirection = normalize(lightVector);
	float diffuse = saturate(dot(normal, lightDirection));

	float rangeFade = dot(lightVector, lightVector) * lightAttenuation.x;
	rangeFade = saturate(1.0 - rangeFade * rangeFade);
	rangeFade *= rangeFade;

	float spotFade = dot(spotDirection, lightDirection);
	spotFade = saturate(spotFade * lightAttenuation.z + lightAttenuation.w);
	spotFade *= spotFade;

	float distanceSqr = max(dot(lightVector, lightVector), 0.00001);
	diffuse *= spotFade * rangeFade / distanceSqr;
	return diffuse * lightColor;
}

#define UNITY_MATRIX_M unity_ObjectToWorld

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

UNITY_INSTANCING_BUFFER_START(PerInstance)
	UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
UNITY_INSTANCING_BUFFER_END(PerInstance)

sampler2D _MainTex;
sampler2D _BackTex;
sampler2D _LeftTex;
sampler2D _RightTex;
sampler2D _TopTex;
sampler2D _BottomTex;

struct VertexInput {
	float4 pos : POSITION;
	float3 normal : NORMAL;
	float2 uv : TEXCOORD;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct VertexOutput {
	float4 clipPos : SV_POSITION;
	float3 normal : TEXCOORD0;
	float3 worldPos : TEXCOORD1;
	float3 objPos : TEXCOORD2;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

VertexOutput LitPassVertex(VertexInput input) {
	VertexOutput output;
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);
	float4 worldPos = mul(UNITY_MATRIX_M, float4(input.pos.xyz, 1.0));
	output.clipPos = mul(unity_MatrixVP, worldPos);
	output.normal = mul((float3x3)UNITY_MATRIX_M, input.normal);
	output.worldPos = worldPos.xyz;
	output.objPos = input.pos.xyz;
	return output;
}

float4 LitPassFragment(VertexOutput input) : SV_TARGET {
	UNITY_SETUP_INSTANCE_ID(input);
	input.normal = normalize(input.normal);
	float3 albedo = UNITY_ACCESS_INSTANCED_PROP(PerInstance, _Color).rgb;

	float3 diffuseLight = 0;
	int i;
	for (i = 0; i < min(unity_LightIndicesOffsetAndCount.y, 4); ++i) {
		int lightIndex = unity_4LightIndices0[i];
		diffuseLight += DiffuseLight(lightIndex, input.normal, input.worldPos);
	}
	for (i = 4; i < min(unity_LightIndicesOffsetAndCount.y, 8); ++i) {
		int lightIndex = unity_4LightIndices1[i - 4];
		diffuseLight += DiffuseLight(lightIndex, input.normal, input.worldPos);
	}
	input.normal = round(input.normal);
	float3 tex =
		saturate(input.normal.x) * tex2D(_RightTex, input.objPos.zy).rgb
		+ saturate(-input.normal.x) * tex2D(_LeftTex, input.objPos.zy).rgb
		+ saturate(input.normal.y) * tex2D(_TopTex, input.objPos.xz).rgb
		+ saturate(-input.normal.y) * tex2D(_BottomTex, input.objPos.xz).rgb
		+ saturate(input.normal.z) * tex2D(_MainTex, input.objPos.xy).rgb
		+ saturate(-input.normal.z) * tex2D(_BackTex, input.objPos.xy).rgb;
	//float2 uv = input.normal.x * input.objPos.yz + input.normal.y * input.objPos.xz + input.normal.z * input.objPos.xy;
	//float3 color = diffuseLight * albedo * tex2D(_MainTex, uv).rgb;
	float3 color = diffuseLight * albedo * tex;
	return float4(color, 1);
}

#endif // PIPELINE_LIT_CUBE_INCLUDED