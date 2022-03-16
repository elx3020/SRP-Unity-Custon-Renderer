using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

// We can access the scene's main light via RenderSettings.sun.

public class Lighting
{
    const string bufferName = "Lighting";
    CommandBuffer lightBuffer = new CommandBuffer { name = bufferName };

    const int maxDirLightCount = 4;

    static int dirLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
    dirLightColorsId = Shader.PropertyToID("_DirectionalLightColors"),
    dirLightDirectionsId = Shader.PropertyToID("_DirectionalLightDirections");

    // the way of storing the vectors can change in the future and use a structured buffer

    static Vector4[] dirLightColors = new Vector4[maxDirLightCount],
    dirLightDirections = new Vector4[maxDirLightCount];




    // to get just one light, properties are later not arrays but float3

    // static int dirLightColorId = Shader.PropertyToID("_DirectionalLightColor"),
    // dirLightDirectionId = Shader.PropertyToID("_DirectionalLightDirection");

    // has information of what the camera sees
    CullingResults cullingResults;

    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings shadowSettings)
    {
        this.cullingResults = cullingResults;
        lightBuffer.BeginSample(bufferName);
        // get directional lights of the scene view
        SetupLights();
        lightBuffer.EndSample(bufferName);
        context.ExecuteCommandBuffer(lightBuffer);
        lightBuffer.Clear();

    }


    void SetupDirectionalLight(int index, ref VisibleLight visibleLight)
    {
        dirLightColors[index] = visibleLight.finalColor;
        // lights direction
        dirLightDirections[index] = -visibleLight.localToWorldMatrix.GetColumn(2);



        // another way to get directional light. Only get one.
        // sun light
        // Light light = RenderSettings.sun;
        // lightBuffer.SetGlobalVector(dirLightColorId, light.color.linear * light.intensity);
        // lightBuffer.SetGlobalVector(dirLightDirectionId, -light.transform.forward);

    }

    void SetupLights()
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;
        int dirLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            if (visibleLight.lightType == LightType.Directional)
            {
                SetupDirectionalLight(dirLightCount++, ref visibleLight);
                if (dirLightCount >= maxDirLightCount)
                {
                    break;
                }

            }

        }

        lightBuffer.SetGlobalInt(dirLightCountId, visibleLights.Length);
        lightBuffer.SetGlobalVectorArray(dirLightColorsId, dirLightColors);
        lightBuffer.SetGlobalVectorArray(dirLightDirectionsId, dirLightDirections);


    }




}
