using UnityEngine;
using UnityEngine.Rendering;
public class CustomRenderPipeline : RenderPipeline
{

    CameraRenderer renderer = new CameraRenderer();

    // to control batching or instancing
    bool useDynamicBatching, useGPUInstancing;

    // shadows data;

    ShadowSettings shadowSettings;


    // Custom Render Pipeline constructor definition

    public CustomRenderPipeline(bool useDynamicBatching, bool useGPUInstancing, bool useSPRBatcher, ShadowSettings shadowSettings)
    {
        this.useDynamicBatching = useDynamicBatching;
        this.useGPUInstancing = useGPUInstancing;
        this.shadowSettings = shadowSettings;
        GraphicsSettings.useScriptableRenderPipelineBatching = useSPRBatcher;
        GraphicsSettings.lightsUseLinearIntensity = true;
    }




    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        foreach (Camera camera in cameras)
        {
            renderer.Render(context, camera, useDynamicBatching, useGPUInstancing, shadowSettings);
        }
    }
}
