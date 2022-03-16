using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Rendering/Custom Render Pipeline")]
public class CustomRenderPipelineAsset : RenderPipelineAsset
{
    [SerializeField]
    bool useDynamicBatching = true, useGPUInstancing = true, useSPRBatcher = true;
    [SerializeField]
    // passing shadow settings to the asset
    ShadowSettings shadows = default;
    protected override RenderPipeline CreatePipeline()
    {
        // constructor instanciated on the pipeline asset
        return new CustomRenderPipeline(useDynamicBatching, useGPUInstancing, useSPRBatcher, shadows);
    }
}
