# 自定义渲染管线
Aura3D 支持自定义渲染管线，开发者开发自定义风格的渲染管线时，可以不用处理 VAO，VBO 等繁琐细节，但是仍然需要具备基础的渲染知识。

## 内置渲染管线
Aura3D 内置了两个简单的渲染管线：
1. BlinnPhong: 写实风格的渲染管线
2. NoLight: 无光照的渲染管线

### 内置渲染管线
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
    view.CreateRenderPipeline = scene => new NoLightPipeline(scene)；
}

```

### 规划中
1. 卡通渲染管线
2. PBR延迟管线

## 自定义渲染管线

自定义渲染管线主要需要实现两种类：RenderPipeline和RenderPass，建议参考源码中的 NoLightPipeline 进行学习。

### RenderPipeline
RenderPipeline 主要用来注册 RenderPass 和 RenderTarget

#### RenderPassGroup
RenderPass 的渲染时机分为 RenderPassGroup.EveryCamera 和 RenderPassGroup.Once
EveryCamera 是针对每个相机都会执行的 Pass, 大部分RenderPass的RenderPassGroup都应该注册成EveryCamera

Once则是针对不区分摄像机的渲染行为，例如光源的Shadowmap的渲染

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
RenderPass是一个渲染流程，一般一个Shader(包含不同变体)是一个RenderPass
#### 着色器
实现RenderPass时，需要指定Shader的顶点着色器和片段着色器的代码，代码中允许有宏定义开关，渲染前使用`UseShader(宏定义开启列表)`来指定要开启哪些开关
#### 渲染
在渲染时调用`RenderMeshes`函数来筛选需要渲染的模型

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
#### 自定义传参
当开发者需要针对某个Mesh单独进行传参时，可以覆盖`RenderMesh`函数
```CSharp
 public override void RenderMesh(Mesh mesh, Matrix4x4 view, Matrix4x4 projection)
    {
        if (条件)
        {
            UniformFloat("some parameter", "value");
        }
        UniformMatrix4("viewMatrix", camera.View);
        UniformMatrix4("projectionMatrix", camera.Projection);
        base.RenderMesh(mesh, view, projection);
    }
```