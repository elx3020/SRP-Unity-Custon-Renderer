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

    const int maxShadowedDirectionalLightCount = 4;

    struct ShadowedDirectionalLight
    {
        public int visibleLightIndex;
    }

    ShadowedDirectionalLight[] shadowedDirectionalLights = new ShadowedDirectionalLight[maxShadowedDirectionalLightCount];

    static int dirShadowAtlasId = Shader.PropertyToID("_DirectionalShadowAtlas"),
    dirShadowMatricesId = Shader.PropertyToID("_DirectionalShadowMatrices");

    static Matrix4x4[] dirShadowMatrices = new Matrix4x4[maxShadowedDirectionalLightCount];



    int ShadowedDirectionalLightCount;




    public void Setup(ScriptableRenderContext context, CullingResults cullingResults, ShadowSettings settings)
    {
        this.context = context;
        this.cullingResults = cullingResults;
        this.settings = settings;
        ShadowedDirectionalLightCount = 0;


    }

    public Vector2 ReserveDirectionShadows(Light light, int visibleLightIndex)
    {
        //  reserve the light if the light count is less than the max amount and if the light cast shadows and the shadow shadowStrengh is larger than 0
        if (ShadowedDirectionalLightCount < maxShadowedDirectionalLightCount && light.shadows != LightShadows.None && light.shadowStrength > 0f && cullingResults.GetShadowCasterBounds(visibleLightIndex, out Bounds b))
        {
            shadowedDirectionalLights[ShadowedDirectionalLightCount] = new ShadowedDirectionalLight { visibleLightIndex = visibleLightIndex };
            return new Vector2(light.shadowStrength, ShadowedDirectionalLightCount++);
        }
        return Vector2.zero;
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

        // split shadow map for each light 
        int split = ShadowedDirectionalLightCount <= 1 ? 1 : 2;
        int tileSize = atlasSize / split;

        for (int i = 0; i < ShadowedDirectionalLightCount; i++)
        {
            RenderDirectionalShadows(i, split, tileSize);
        }

        shadowBuffer.SetGlobalMatrixArray(dirShadowMatricesId, dirShadowMatrices);
        shadowBuffer.EndSample(bufferName);
        ExecuteBuffer();

    }

    void RenderDirectionalShadows(int i, int split, int tileSize)
    {
        ShadowedDirectionalLight light = shadowedDirectionalLights[i];
        var shadowSettings = new ShadowDrawingSettings(cullingResults, light.visibleLightIndex);
        // ! important and needs to check how it works
        // The idea of a shadow map is that we render the scene from the light's point of view, only storing the depth information. The result tells us how far the light travels before it hits something.

        cullingResults.ComputeDirectionalShadowMatricesAndCullingPrimitives(light.visibleLightIndex, 0, 1, Vector3.zero, tileSize, 0f, out Matrix4x4 viewMatrix, out Matrix4x4 projectionMatrix, out ShadowSplitData splitData);

        shadowSettings.splitData = splitData;
        SetTileViewport(i, split, tileSize);
        // conversion matrices world space to light space
        dirShadowMatrices[i] = ConvertToAtlasMatrix(projectionMatrix * viewMatrix, SetTileViewport(i, split, tileSize), split);
        shadowBuffer.SetViewProjectionMatrices(viewMatrix, projectionMatrix);
        ExecuteBuffer();
        context.DrawShadows(ref shadowSettings);
    }

    Vector2 SetTileViewport(int index, int split, float tileSize)
    {
        Vector2 offset = new Vector2(index % split, index / split);
        shadowBuffer.SetViewport(new Rect(
            offset.x * tileSize, offset.y * tileSize, tileSize, tileSize
        ));

        return offset;
    }

    Matrix4x4 ConvertToAtlasMatrix(Matrix4x4 m, Vector2 offset, int split)
    {
        if (SystemInfo.usesReversedZBuffer)
        {
            m.m20 = -m.m20;
            m.m21 = -m.m21;
            m.m22 = -m.m22;
            m.m23 = -m.m23;

        }
        // Second, clip space is defined inside a cube with with coordinates going from −1 to 1, with zero at its center. But textures coordinates and depth go from zero to one. We can bake this conversion into the matrix by scaling and offsetting the XYZ dimensions by half.
        // Finally, we have to apply the tile offset and scale. Once again we can do this directly to avoid a lot of unnecessary calculations.
        float scale = 1f / split;
        m.m00 = (0.5f * (m.m00 + m.m30) + offset.x * m.m30) * scale;
        m.m01 = (0.5f * (m.m01 + m.m31) + offset.x * m.m31) * scale;
        m.m02 = (0.5f * (m.m02 + m.m32) + offset.x * m.m32) * scale;
        m.m03 = (0.5f * (m.m03 + m.m33) + offset.x * m.m33) * scale;
        m.m10 = (0.5f * (m.m10 + m.m30) + offset.y * m.m30) * scale;
        m.m11 = (0.5f * (m.m11 + m.m31) + offset.y * m.m31) * scale;
        m.m12 = (0.5f * (m.m12 + m.m32) + offset.y * m.m32) * scale;
        m.m13 = (0.5f * (m.m13 + m.m33) + offset.y * m.m33) * scale;
        m.m20 = 0.5f * (m.m20 + m.m30);
        m.m21 = 0.5f * (m.m21 + m.m31);
        m.m22 = 0.5f * (m.m22 + m.m32);
        m.m23 = 0.5f * (m.m23 + m.m33);

        return m;
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
