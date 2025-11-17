using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;
using Veldrid;

namespace Aura3D.Avalonia.Controls;

public class VeldridControl : Control
{
    private Compositor? _compositor;

    private CompositionSurfaceVisual? _visual;

    private GraphicsDevice? _graphicsDevice;

    private IGraphicsImpl? _graphicsImpl;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        initialize();
    }


    async void initialize()
    {
        try
        {

            var selfVisual = ElementComposition.GetElementVisual(this);

            if (selfVisual == null)
                throw new InvalidOperationException("Failed to get ElementVisual from control.");

            _compositor = selfVisual.Compositor;

            var interop = await _compositor.TryGetCompositionGpuInterop();

            if (interop == null)
                throw new InvalidOperationException("Failed to get GpuInterop from Compositor.");

            var surface = _compositor.CreateDrawingSurface();

            _visual = _compositor.CreateSurfaceVisual();

            _visual.Size = new Vector(Bounds.Width, Bounds.Height);

            _visual.Surface = surface;

            ElementComposition.SetElementChildVisual(this, _visual);

            _graphicsImpl = new OpenGLImpl(_compositor, _visual, surface, this);

            _graphicsDevice = await _graphicsImpl.Initialize();

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (_visual != null && change.Property == BoundsProperty)
        {
            _visual.Size = new Vector(Bounds.Width, Bounds.Height);
        }
        base.OnPropertyChanged(change);
    }

}

