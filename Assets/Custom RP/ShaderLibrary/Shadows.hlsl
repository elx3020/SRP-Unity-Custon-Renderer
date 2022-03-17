// include guard defined:
#ifndef CUSTOM_SHADOWS_INCLUDED
#define CUSTOM_SHADOWS_INCLUDED



#define MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT 4

// contains shadow map texture
TEXTURE2D_SHADOW(_DirectionalShadowAtlas);
// shadow map sampler
#define SHADOW_SAMPLER sampler_linear_clamp_compare
SAMPLER_CMP(SHADOW_SAMPLER);


// shadow matrices
CBUFFER_START(_CustomShadows)
	float4x4 _DirectionalShadowMatrices[MAX_SHADOWED_DIRECTIONAL_LIGHT_COUNT];
CBUFFER_END

// shadow data from direction light

struct DirectionalShadowData {
	float strength;
	int tileIndex;
};


//  function that return the 
float SampleDirectionalShadowAtlas (float3 positionSTS) {
	return SAMPLE_TEXTURE2D_SHADOW(
		_DirectionalShadowAtlas, SHADOW_SAMPLER, positionSTS
	);
}

float GetDirectionalShadowAttenuation (DirectionalShadowData data, Surface surfaceWS) {

	if (data.strength <= 0.0) {
		return 1.0;
	}
	float3 positionSTS = mul(
		_DirectionalShadowMatrices[data.tileIndex],
		float4(surfaceWS.position, 1.0)
	).xyz;
	float shadow = SampleDirectionalShadowAtlas(positionSTS);
	return lerp(1.0, shadow, data.strength);
}



#endif

