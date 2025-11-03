using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using System.Numerics;
using System.Text;
using Veldrid;
using Veldrid.OpenGL;
using Vulkan;

namespace Aura3D.Avalonia;

public class VeldridControl : OpenGlControlBase, IExternalOpenGLThreadCallback
{
    GraphicsDevice? device;

    public VeldridControl()
    {

        Console.WriteLine("main Thread: " + Thread.CurrentThread.ManagedThreadId);
    }

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);

        size = Bounds;

        device = GraphicsDevice.CreateOpenGL(false, gl.GetProcAddress, new System.Drawing.Size((int)size.Width, (int)size.Height), this);

        Task.Run(() =>
        {
            try
            {

                CreateResources();
                while (true)
                {

                    _commandList.Begin();

                    _commandList.SetFramebuffer(device.SwapchainFramebuffer);
                    _commandList.ClearColorTarget(0, RgbaFloat.Red);
                    _commandList.ClearDepthStencil(1.0f);
                    _commandList.SetVertexBuffer(0, _vertexBuffer);
                    _commandList.SetIndexBuffer(_indexBuffer, IndexFormat.UInt16);
                    _commandList.SetPipeline(_pipeline);
                    _commandList.DrawIndexed(
                        indexCount: 4,
                        instanceCount: 1,
                        indexStart: 0,
                        vertexOffset: 0,
                        instanceStart: 0);
                    _commandList.End();
                    device.SubmitCommands(_commandList);
                    device.SwapBuffers();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in VeldridControl rendering task: " + ex.Message);
            }

        });
    }

    bool b = false;

    int fbo = 0;

    Rect size = default;
    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        if (device == null)
            return;
        if (fbo != fb)
        {
            fbo = fb;
            OnSetOpenGLFramebuffer.Invoke((uint)fb);
        }
        if (size != Bounds)
        {
            size = Bounds;
            OnFrameBufferSizeChanged.Invoke(new System.Drawing.Size((int)Bounds.Width, (int)Bounds.Height));
        }
        if (!b)
        {
            Console.WriteLine("OpenGL Thread: " + Thread.CurrentThread.ManagedThreadId);
            b = !b;
        }
        try
        {
            OnOpenGLThreadExcecute.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in VeldridControl OnOpenGlRender: " + ex.Message);
        }
        RequestNextFrameRendering();
    }


    private static CommandList _commandList;
    private static DeviceBuffer _vertexBuffer;
    private static DeviceBuffer _indexBuffer;
    private static Shader[] _shaders;
    private static Pipeline _pipeline;
    private const string VertexCode = @"#version 300 es

precision mediump float;
layout(location = 0) in vec2 Position;
layout(location = 1) in vec4 Color;

out vec4 fsin_Color;

void main()
{
    gl_Position = vec4(Position, 0, 1);
    fsin_Color = Color;
}";

    private const string FragmentCode = @"#version 300 es

precision mediump float;
in vec4 fsin_Color;
layout(location = 0) out vec4 fsout_Color;

void main()
{
    fsout_Color = fsin_Color;
}";

    public event Action OnOpenGLThreadExcecute = () => { };
    public event Action<uint> OnSetOpenGLFramebuffer = id => { };
    public event Action<System.Drawing.Size> OnFrameBufferSizeChanged = size => { };

    private void CreateResources()
    {
        ResourceFactory factory = device.ResourceFactory;

        VertexPositionColor[] quadVertices =
        {
                new VertexPositionColor(new Vector2(-0.75f, 0.75f), RgbaFloat.Red),
                new VertexPositionColor(new Vector2(0.75f, 0.75f), RgbaFloat.Green),
                new VertexPositionColor(new Vector2(-0.75f, -0.75f), RgbaFloat.Blue),
                new VertexPositionColor(new Vector2(0.75f, -0.75f), RgbaFloat.Yellow)
            };

        ushort[] quadIndices = { 0, 1, 2, 3 };

        _vertexBuffer = factory.CreateBuffer(new BufferDescription(4 * VertexPositionColor.SizeInBytes, BufferUsage.VertexBuffer));
        _indexBuffer = factory.CreateBuffer(new BufferDescription(4 * sizeof(ushort), BufferUsage.IndexBuffer));

        device.UpdateBuffer(_vertexBuffer, 0, quadVertices);
        device.UpdateBuffer(_indexBuffer, 0, quadIndices);

        VertexLayoutDescription vertexLayout = new VertexLayoutDescription(
            new VertexElementDescription("Position", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float2),
            new VertexElementDescription("Color", VertexElementSemantic.TextureCoordinate, VertexElementFormat.Float4));

        ShaderDescription vertexShaderDesc = new ShaderDescription(
            ShaderStages.Vertex,
            Encoding.UTF8.GetBytes(VertexCode),
            "main");
        ShaderDescription fragmentShaderDesc = new ShaderDescription(
            ShaderStages.Fragment,
            Encoding.UTF8.GetBytes(FragmentCode),
            "main");

        _shaders = [factory.CreateShader(vertexShaderDesc), factory.CreateShader(fragmentShaderDesc)];

        GraphicsPipelineDescription pipelineDescription = new GraphicsPipelineDescription();
        pipelineDescription.BlendState = BlendStateDescription.SingleOverrideBlend;

        pipelineDescription.DepthStencilState = new DepthStencilStateDescription(
            depthTestEnabled: true,
            depthWriteEnabled: true,
            comparisonKind: ComparisonKind.LessEqual);

        pipelineDescription.RasterizerState = new RasterizerStateDescription(
            cullMode: FaceCullMode.Back,
            fillMode: PolygonFillMode.Solid,
            frontFace: FrontFace.Clockwise,
            depthClipEnabled: true,
            scissorTestEnabled: false);

        pipelineDescription.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
        pipelineDescription.ResourceLayouts = System.Array.Empty<ResourceLayout>();

        pipelineDescription.ShaderSet = new ShaderSetDescription(
            vertexLayouts: new VertexLayoutDescription[] { vertexLayout },
            shaders: _shaders);

        pipelineDescription.Outputs = device.SwapchainFramebuffer.OutputDescription;
        _pipeline = factory.CreateGraphicsPipeline(pipelineDescription);

        _commandList = factory.CreateCommandList();
    }

    struct VertexPositionColor
    {
        public Vector2 Position; // This is the position, in normalized device coordinates.
        public RgbaFloat Color; // This is the color of the vertex.
        public VertexPositionColor(Vector2 position, RgbaFloat color)
        {
            Position = position;
            Color = color;
        }
        public const uint SizeInBytes = 24;
    }
}
