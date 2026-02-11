using Aura3D.Core.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura3D.Core.Renderers.PBRDeferred;

public class PBRDeferredPipeline : RenderPipeline, IRenderPipelineCreateInstance
{
    public PBRDeferredPipeline(Scene scene) : base(scene)
    {
        this.RegisterRenderPass(new BasePass(this).SetOutPutRenderTarget("GBuffer"), RenderPassGroup.EveryCamera);

        this.RegisterRenderPass(new LightingPass(this, "GBuffer").SetOutPutRenderTarget("BaseRenderTarget"), RenderPassGroup.EveryCamera);

        this.RegisterRenderTarget("GBuffer")
            .AddTexture("BaseColorMetallic", TextureFormat.Rgba8)
            .AddTexture("NormalRoughness", TextureFormat.Rgba8)
            .SetDepthTexture(TextureFormat.DepthComponent16);
    }

    public static RenderPipeline CreateInstance(Scene scene) => new PBRDeferredPipeline(scene);
}
