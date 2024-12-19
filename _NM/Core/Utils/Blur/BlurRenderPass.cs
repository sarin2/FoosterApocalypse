using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _NM.Core.Utils.Blur
{
    public class BlurRenderPass : ScriptableRenderPass
    {
        private Material material;
        private BlurSettings blurSettings;
        private RTHandle source;
        private int blurTexID;

        public void Setup(ScriptableRenderer renderer)
        {
            source = renderer.cameraColorTargetHandle;
            blurSettings = VolumeManager.instance.stack.GetComponent<BlurSettings>();
            renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

            if (blurSettings != null && blurSettings.IsActive())
            {
                material = new Material(Shader.Find("PostProcessing/Blur"));
            }
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            if (blurSettings == null || !blurSettings.IsActive())
            {
                return;
            }

            blurTexID = Shader.PropertyToID("_BlurTex");
            cmd.GetTemporaryRT(blurTexID, cameraTextureDescriptor);

            base.Configure(cmd, cameraTextureDescriptor);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (blurSettings == null || !blurSettings.IsActive())
            {
                return;
            }

            CommandBuffer cmd = CommandBufferPool.Get("Blur Post Process");

            int gridSize = Mathf.CeilToInt(blurSettings.strength.value * 3.0f);

            if (gridSize % 2 == 0)
            {
                gridSize++;
            }

            material.SetInteger("_GridSize", gridSize);
            material.SetFloat("_Spread", blurSettings.strength.value);

            cmd.Blit(source, blurTexID, material, 0);
            cmd.Blit(blurTexID, source, material, 1);

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(blurTexID);
            base.FrameCleanup(cmd);
        }
    }
}