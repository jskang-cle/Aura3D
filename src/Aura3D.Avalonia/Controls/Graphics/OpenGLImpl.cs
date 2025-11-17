using Avalonia;
using Avalonia.Controls;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using Silk.NET.OpenGLES;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Aura3D.Avalonia.Controls;

internal class OpenGLImpl : IGraphicsImpl
{
    Compositor compositor;

    CompositionSurfaceVisual visual;

    Control control;

    IGlContext? glContext;

    Thread? renderThread;

    GL? gl;

    object sizeLock = new object();

    [MemberNotNullWhen(true, nameof(gl))]
    [MemberNotNullWhen(true, nameof(renderThread))]
    [MemberNotNullWhen(true, nameof(glContext))]
    bool initialized { get; set; } = false;

    public OpenGLImpl(Compositor compositor, CompositionSurfaceVisual visual, CompositionDrawingSurface surface, Control control)
    {
        this.compositor = compositor;
        this.visual = visual;
        this.control = control;
    }

    public async Task<GraphicsDevice> Initialize()
    {
        var contextSharingFeature = (IOpenGlTextureSharingRenderInterfaceContextFeature?)await compositor.TryGetRenderInterfaceFeature(typeof(IOpenGlTextureSharingRenderInterfaceContextFeature));

        if (contextSharingFeature == null)
            throw new InvalidOperationException("Failed to get OpenGL Texture Sharing Feature from Compositor.");

        if (contextSharingFeature != null && contextSharingFeature.CanCreateSharedContext)
        {
            glContext = contextSharingFeature.CreateSharedContext();
        }
        else
        {
            var contextFactory = AvaloniaLocator.Current.GetRequiredService<IPlatformGraphicsOpenGlContextFactory>();
            glContext = contextFactory.CreateContext(null);
        }

        if (glContext == null)
            throw new InvalidOperationException("Failed to create OpenGL context.");

        gl = GL.GetApi(glContext.GlInterface.GetProcAddress);

        initialized = true;

        renderThread = new Thread(OnRenderThreadRun);

        renderThread.Start();

        return null;
    }

    private void OnRenderThreadRun()
    {
        if (initialized == false)
            return;
        var fbo = gl.GenFramebuffer();
        glContext.MakeCurrent();
        
    }

    
    public void Resize(int width, int height)
    {
    }
}
