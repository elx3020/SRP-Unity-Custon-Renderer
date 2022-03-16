Shader "Custom RP/Lit"{

    
    Properties {
        _BaseTexture("Base Texture",2d) = "white" {}
        _BaseColor("Base Color", Color) = (0.5,0.5,0.5,1.0)
        [Enum (UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend",float) = 1
        [Enum (UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend",float) = 0
        [Enum(Off,0,On,1)] _ZWrite("Z Write",float) = 1
        _Cutoff ("Alpha Cutoff", Range(0.0, 1.0)) = 0.5
        [Toggle(_CLIPPING)] _Clipping ("Alpha Clipping", Float) = 0
    }

    SubShader{

        Pass{
            Tags {
                "Lightmode" = "CustomLit"
            }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            HLSLPROGRAM
            #pragma target 3.5
            // to optimize normal calculation if object are scaled uniformaly
            // #pragma instancing_options assumeuniformscaling
            // use to toggle alpha clipping
            #pragma shader_feature _CLIPPING
            // necessart for gpu instancing
            #pragma multi_compile_instancing
            // name of the vertex and fragment function
            // define vertex and fragment function
            #pragma vertex LitPassVertex
            #pragma fragment LitPassFragment
            // include lit pass code
            #include "LitPass.hlsl"
            ENDHLSL
        }

        Pass{
            Tags {
				"LightMode" = "ShadowCaster"
			}
            // we dont need to write colors, only depht
			ColorMask 0

			HLSLPROGRAM
			#pragma target 3.5
			#pragma shader_feature _CLIPPING
			#pragma multi_compile_instancing
			#pragma vertex ShadowCasterPassVertex
			#pragma fragment ShadowCasterPassFragment
			#include "ShadowCasterPass.hlsl"
			ENDHLSL

        }
    }

}
