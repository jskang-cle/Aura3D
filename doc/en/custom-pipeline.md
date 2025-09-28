# Custom Rendering Pipeline
Aura3D supports custom rendering pipelines. When developers create custom-style rendering pipelines, they don't need to handle tedious details like VAO and VBO, but they still need to have basic rendering knowledge.

## Built-in Rendering Pipelines
Aura3D has two built-in simple rendering pipelines:
1. BlinnPhong: A photorealistic rendering pipeline
2. NoLight: A rendering pipeline without lighting

### Built-in Rendering Pipelines
```xaml
<Window
    ...
    xmlns:aura3d="clr-namespace:Aura3D.Avalonia;assembly=Aura3D.Avalonia"
    ...>
	<aura3d:Aura3DView x:Name="aura3Dview" SetupPipeline="OnSetupPipeline"/>
</Window>
```

```CSharp 
public void OnSetupPipeline(object sender, RoutedEventArgs args)
{
    var view = (Aura3DView)sender;
    view.CreateRenderPipeline = scene => new NoLightPipeline(scene);
}
```

### Planned
1. Cartoon rendering pipeline
2. PBR deferred pipeline

## Custom Rendering Pipeline

To create a custom rendering pipeline, you mainly need to implement two types of classes: RenderPipeline and RenderPass. It is recommended to refer to the NoLightPipeline in the source code for learning.

### RenderPipeline
RenderPipeline is mainly used to register RenderPass and RenderTarget

#### RenderPassGroup
The rendering timing of RenderPass is divided into RenderPassGroup.EveryCamera and RenderPassGroup.Once
EveryCamera is a Pass that executes for each camera. Most RenderPasses should be registered with RenderPassGroup.EveryCamera

Once is for rendering behaviors that don't distinguish between cameras, such as rendering shadowmaps for light sources

```CSharp
public class NoLightPipeline : RenderPipeline
{
    public NoLightPipeline(Scene scene)
    {
        var noLightPass = new NoLightPass(this);

        RegisterRenderPass(new BackgroundPass(this).SetOutPutRenderTarget("BaseRenderTarget"), RenderPassGroup.EveryCamera);

        RegisterRenderPass(noLightPass.SetOutPutRenderTarget("BaseRenderTarget"), RenderPassGroup.EveryCamera);

        RegisterRenderPass(new GammaCorrectionPass(this, "BaseRenderTarget", "Color").SetOutPutRenderTarget("GammaOutput"), RenderPassGroup.EveryCamera);
        RegisterRenderPass(new FxaaPass(this, "GammaOutput", "Color"), RenderPassGroup.EveryCamera);

        RegisterRenderTarget("BaseRenderTarget")
            .AddTexture("Color", TextureFormat.Rgba16f)
            .SetDepthTexture(TextureFormat.DepthComponent16);


        RegisterRenderTarget("GammaOutput")
            .AddTexture("Color", TextureFormat.Rgba8)
            .SetDepthTexture(TextureFormat.DepthComponent16);
    }
}
```

### RenderPass
RenderPass is a rendering process. Generally, one Shader (including different variants) is one RenderPass
#### Shader
When implementing RenderPass, you need to specify the code for the vertex shader and fragment shader of the Shader. The code allows macro definition switches. Before rendering, use `UseShader(list of enabled macros)` to specify which switches to enable.
#### Rendering
Call the `RenderMeshes` function during rendering to filter the models that need to be rendered

```CSharp
public class NoLightPass : RenderPass
{
    public NoLightPass(RenderPipeline renderPipeline) : base(renderPipeline)
    {
        this.FragmentShader = ShaderResource.NoLightFrag;
        this.VertexShader = ShaderResource.NoLightVert;
    }

    public override void Render(Camera camera)
    {
        ...

        UseShader();
        RenderMeshes(mesh => FilterSkeletonMesh(mesh) == false && (mesh.Material == null || mesh.Material.BlendMode == BlendMode.Opaque), camera.View, camera.Projection);

        UseShader("SKINNED_MESH");
        RenderMeshes(mesh => FilterSkeletonMesh(mesh) && (mesh.Material == null || mesh.Material.BlendMode == BlendMode.Opaque), camera.View, camera.Projection);

        ...
    }
}
```

#### Custom Parameter Passing
When developers need to pass parameters单独 for a specific Mesh, they can override the `RenderMesh` function
```CSharp
 public override void RenderMesh(Mesh mesh, Matrix4x4 view, Matrix4x4 projection)
    {
        if (condition)
        {
            UniformFloat("some parameter", "value");
        }
        UniformMatrix4("viewMatrix", camera.View);
        UniformMatrix4("projectionMatrix", camera.Projection);
        base.RenderMesh(mesh, view, projection);
    }
```