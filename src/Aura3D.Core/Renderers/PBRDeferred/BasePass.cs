using Aura3D.Core.Nodes;

namespace Aura3D.Core.Renderers.PBRDeferred;

internal class BasePass : RenderPass
{

    public BasePass(RenderPipeline renderPipeline) : base(renderPipeline)
    {
        VertexShader = ShaderResource.MeshVert;
        
    }

    public override void BeforeRender(Camera camera)
    {
        base.BeforeRender(camera);
    }

    public override void Render(Camera camera)
    {
        base.Render(camera);
    }

    public override void AfterRender(Camera camera)
    {
        base.AfterRender(camera);
    }
}
