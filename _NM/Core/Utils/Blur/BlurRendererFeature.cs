using UnityEngine.Rendering.Universal;


namespace _NM.Core.Utils.Blur
{
    public class BlurRendererFeature : ScriptableRendererFeature
    {
        BlurRenderPass blurRenderPass;

        public override void Create()
        {
            blurRenderPass = new BlurRenderPass();
            name = "Blur";
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            blurRenderPass.Setup(renderer);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (blurRenderPass != null)
            {
                renderer.EnqueuePass(blurRenderPass);
            }
        }
    }
}

