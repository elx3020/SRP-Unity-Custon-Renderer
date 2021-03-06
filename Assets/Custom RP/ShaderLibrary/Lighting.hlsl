// include guard defined:
#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

float3 IncomingLight(Surface surface, Light light){

	float3 lightnormal = saturate(dot(surface.normal,light.direction) * light.attenuation) * light.color;

	// lightnormal =  floor(lightnormal * 5);
	return lightnormal;
}

float3 GetLighting(Surface surface,Light light){
	return IncomingLight(surface,light) * surface.color;
}

float3 GetLighting (Surface surfaceWS){
	float3 color = 0.0;
	for(int i = 0; i < GetDirectionalLightCount();i++){
		color += GetLighting(surfaceWS,GetDirectionalLight(i,surfaceWS));
	}
	return color;
}

#endif

