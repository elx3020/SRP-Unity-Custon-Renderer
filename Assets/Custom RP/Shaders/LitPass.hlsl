// include guard defined:
#ifndef CUSTOM_LIT_PASS_INCLUDED
#define CUSTOM_LIT_PASS_INCLUDED

#include "../ShaderLibrary/Common.hlsl"
#include "../ShaderLibrary/Surface.hlsl"
#include "../ShaderLibrary/Light.hlsl"
#include "../ShaderLibrary/Lighting.hlsl"

// CBUFFER_START(UnityPerMaterial)
// 	float4 _BaseColor;
// CBUFFER_END

// shader resources cannot be provided per instances
TEXTURE2D(_BaseTexture);
SAMPLER(sampler_BaseTexture);

// material properties/ can be change with a  property block by instance
UNITY_INSTANCING_BUFFER_START(UnityPerMaterial)
	//	float4 _BaseColor;
	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseColor)
	// tiling and offset
	UNITY_DEFINE_INSTANCED_PROP(float4, _BaseTexture_ST)
	UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
	

UNITY_INSTANCING_BUFFER_END(UnityPerMaterial)

struct Attributes {
	float3 positionOS : POSITION;
	float3 normalOS : NORMAL;
	float2 uv0 : TEXCOORD0;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};


struct Varyings {
	// SV_position is a sematic word
    float4 positionCS : SV_POSITION;
	float3 normalWS : VAR_NORMAL;
	// the sematic doesn have any further meaning
	float2 uv0 : VAR_UV0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};


Varyings LitPassVertex (Attributes input) { //: SV_POSITION {
	// out varyings struct --> fragment data
	Varyings output;
	// vertex data
	UNITY_SETUP_INSTANCE_ID(input);
	// set vertex data to support instancing
	UNITY_TRANSFER_INSTANCE_ID(input, output);
	// convert object space position of vertices to world space
	float3 positionWS = TransformObjectToWorld(input.positionOS);
	// convert world space position of vertices to clip space position (fragment)
	output.positionCS = TransformWorldToHClip(positionWS);
	// normals
	output.normalWS = TransformObjectToWorldNormal(input.normalOS);

	// set uv coordinates to be scale and offset by an scale and offset vectors.

	float4 texture_ST = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseTexture_ST);
	output.uv0 = input.uv0 * texture_ST.xy + texture_ST.zw; 
	return output;
} 




float4 LitPassFragment (Varyings input) : SV_TARGET {
	UNITY_SETUP_INSTANCE_ID(input);
	float4 baseTexture = SAMPLE_TEXTURE2D(_BaseTexture, sampler_BaseTexture, input.uv0);
	float4 baseColor = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _BaseColor);
	
	float4 base = baseTexture * baseColor;
	// build in function
	#if defined(_CLIPPING)
		float alphaclipthrhold = UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff);
		clip(base.a - UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Cutoff));
	#endif



	Surface surface;
	surface.normal = normalize(input.normalWS);
	surface.color = base.rgb;
	surface.alpha = base.a;

	float3 color = GetLighting(surface);

	return float4(color,surface.alpha);
}


#endif

