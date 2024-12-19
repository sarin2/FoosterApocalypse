using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


public class RadialBlurPass : ScriptableRenderPass
{
    public Material blurMaterial = null;
    public RenderTargetIdentifier source { get; set; }

    private ScriptableRenderer renderer;
    public RadialBlurPass(Material blurMaterial)
    {
        this.blurMaterial = blurMaterial;
    }
        
    public void Setup(ScriptableRenderer renderer)
    {
        this.renderer = renderer;
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        CommandBuffer cmd = CommandBufferPool.Get("_RadialBlur");
        RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
		RenderTargetIdentifier cameraColorTarget = renderer.cameraColorTarget; // here!
		opaqueDesc.useMipMap = false;

		cmd.GetTemporaryRT(Shader.PropertyToID("_Temp"), opaqueDesc);

		cmd.SetGlobalTexture("_MainTex", cameraColorTarget);
		cmd.SetGlobalFloat("_BlurAmount", blurMaterial.GetFloat("_BlurAmount"));
		cmd.SetGlobalVector("_BlurCenter", blurMaterial.GetVector("_BlurCenter"));
		cmd.SetGlobalFloat("_BlurSampleCount", blurMaterial.GetFloat("_BlurSampleCount"));
		cmd.SetGlobalFloat("_BlurSampleIntensity", blurMaterial.GetFloat("_BlurSampleIntensity"));

		Blit(cmd, cameraColorTarget, "_Temp", blurMaterial);
        Blit(cmd, "_Temp", cameraColorTarget);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }
        
}

public class RadialBlurFeature : ScriptableRendererFeature
{

    public Material blurMaterial = null;
    public RadialBlurPass radialBlurPass;
    RenderTargetHandle tempTexture;

    public override void Create()
    {
        if (!blurMaterial)
        {
            blurMaterial = new Material(Shader.Find("Hidden/RadialBlur"));
        }
      

        radialBlurPass = new RadialBlurPass(blurMaterial);
        radialBlurPass.renderPassEvent = RenderPassEvent.AfterRenderingSkybox;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // This line will pass renderer to _RadialBlurPass
        radialBlurPass.Setup(renderer);
        renderer.EnqueuePass(radialBlurPass);
    }
}
