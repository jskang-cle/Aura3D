// See https://aka.ms/new-console-template for more information
using Aura3D.Core;
using Aura3D.Core.Nodes;
using Aura3D.Core.Renderers;
using Aura3D.Core.Scenes;
using Aura3D.Model;
using Silk.NET.Windowing;
using System.Drawing;
using System.Numerics;

var window = Window.Create(WindowOptions.Default);
ControlRenderTarget controlRenderTarget = new ControlRenderTarget();
Camera.ControlRenderTarget = controlRenderTarget;
Scene scene = new Scene(scene => new BlinnPhongPipeline(scene));
window.Load += () =>
{
    controlRenderTarget.Width = (uint)(window.Size.X);
    controlRenderTarget.Height = (uint)(window.Size.Y);
    controlRenderTarget.FrameBufferId = 0;

    scene.RenderPipeline.Initialize(str =>
    {
        window.GLContext.TryGetProcAddress(str, out var p);
        return p;
    });

    var camera = scene.MainCamera;

    camera.ClearColor = Color.Gray;
    camera.NearPlane = 1;

    var list = new List<Stream>();
    List<string> name =
    [
        "px.png",
        "nx.png",
        "py.png",
        "ny.png",
        "pz.png",
        "nz.png",
    ];
    foreach(var filename in name)
    {
        var stream = new StreamReader($"../../../../../../example/Example/Assets/Textures/skybox/{filename}").BaseStream;
        list.Add(stream);
    }

    var cubeTexture = TextureLoader.LoadCubeTexture(list);

    foreach (var stream in list)
    {
        stream.Dispose();
    }

    camera.ClearType = ClearType.Skybox;

    camera.SkyboxTexture = cubeTexture;


    var model = ModelLoader.LoadGlbModel("C:\\Users\\cesun\\Desktop\\untitled.glb");

    model.Scale = Vector3.One * 10000;

    AddNode(model);

    // camera.FitToBoundingBox(model.BoundingBox);


    DirectionalLight dl = new DirectionalLight();

    dl.CastShadow = true;

    dl.RotationDegrees = new Vector3(-45, 45, 0);

    AddNode(dl);
};


window.Render += (delta) =>
{

    controlRenderTarget.Width = (uint)(window.Size.X);
    controlRenderTarget.Height = (uint)(window.Size.Y);
    scene.RenderPipeline.DefaultFramebuffer = (uint)0;

    scene.RenderPipeline.Render();

    scene.Update(delta);




};

window.Run();


void AddNode<T>(T node) where T : Node
{
    scene.AddNode(node);
}
