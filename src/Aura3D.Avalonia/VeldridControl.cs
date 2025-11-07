using Aura3D.Avalonia.OpenGL;
using Avalonia;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Avalonia.Rendering.Composition;
using Microsoft.VisualBasic;
using Silk.NET.OpenGLES;
using System.Drawing;
using Veldrid;

namespace Aura3D.Avalonia;

public class VeldridControl : Control
{
    Compositor? _compositor;

    GraphicsDevice? _graphicsDevice;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        initialize();
    }


    protected void OnUpdate()
    {
        if (!_initialized)
            return;
        if (_compositor == null)
            return;

        _compositor.RequestCompositionUpdate(OnUpdate);
    }

    bool _initialized = false;

    async void initialize()
    {
        if (_initialized)
            return;
        try
        {

            var selfVisual = ElementComposition.GetElementVisual(this);

            if (selfVisual == null)
                throw new InvalidOperationException("Failed to get ElementVisual from control.");

            _compositor = selfVisual.Compositor;

            var interop = await _compositor.TryGetCompositionGpuInterop();

            if (interop == null)
                throw new InvalidOperationException("Failed to get GpuInterop from Compositor.");

            _initialized = true;

            _compositor.RequestCompositionUpdate(OnUpdate);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    GraphicsDevice CreateGraphicsDeviceFromOpenGL(ICompositionGpuInterop interop)
    {


        return null;
    }

}

