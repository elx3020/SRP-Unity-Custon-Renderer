using UnityEngine;
using UnityEngine.Rendering;

// We can access the scene's main light via RenderSettings.sun.

public class Shadows
{
    const string bufferName = "Shadows";
    CommandBuffer shadowBuffer = new CommandBuffer { name = bufferName };

    ScriptableRenderContext context;

    CullingResults cullingResults;
    ShadowSettings settings;

    const int maxShadowedDirectionalLightCount = 1;

    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }

    ShadowedDirectionalLight[] shadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

    static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas");



    int ShadowedDirectionalLightCount;




    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings)
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.settings = settings;
        ShadowedDirectionalLightCount = 0;


    }

    public void ReserveDirectionShadows(Light light, int visibleLightIndex)
    {
        //  reserve the light if the light count is less than the max amount and if the light cast shadows and the shadow shadowStrengh is larger than 0
        if (ShadowedDirectionalLightCount < maxShadowedDirectionalLightCount && light.shadows != LightShadows.None && light.shadowStrength > 0f && cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
        {
            shadowedDirectionalLights[ShadowedDirectionalLightCount++] = new ShadowedDirectionalLight { visibleLightIndex = visibleLightIndex };
        }
    }

    public void Render()
    {
        if (ShadowedDirectionalLightCount > 0)
        {
            RenderDirectionalShadows();
        }
        else
        {
            shadowBuffer.GetTemporaryRT(dirShadowAtlasId, 1, 1, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        }
    }

    void RenderDirectionalShadows()
    {
        int atlasSize = (int)settings.directional.atlasSize;
        shadowBuffer.GetTemporaryRT(dirShadowAtlasId, atlasSize, atlasSize, 32, FilterMode.Bilinear, RenderTextureFormat.Shadowmap);
        shadowBuffer.SetRenderTarget(dirShadowAtlasId, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        shadowBuffer.ClearRenderTarget(true, false, Color.clear);
        shadowBuffer.BeginSample(bufferName);
        ExecuteBuffer();

        for (int i = 0; i < ShadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, atlasSize);
        }

        shadowBuffer.EndSample(bufferName);
        ExecuteBuffer();

    }

    void RenderDirectionalShadows(int i, int tileSize)
    {
        ShadowedDirectionalLight light = shadowedDirectionalLights[i];
        var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
        // ! important and needs to check how it works
        // The idea of a shadow map is that we render the scene from the light's point of view, only storing the depth information. The result tells us how far the light travels before it hits something.

        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f, out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);

        shadowSettings.splitData = splitData;
        shadowBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
        ExecuteBuffer();
        context.DrawShadows(ref shadowSettings);
    }



    void ExecuteBuffer()
    {
        context.ExecuteCommandBuffer(shadowBuffer);
        shadowBuffer.Clear();
    }

    public void Cleanup()
    {
        shadowBuffer.ReleaseTemporaryRT(dirShadowAtlasId);
        ExecuteBuffer();
    }










}
