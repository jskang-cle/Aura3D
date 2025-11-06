using Aura3D.Avalonia.OpenGL;
using Avalonia;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using Silk.NET.OpenGLES;
using System.Drawing;

namespace Aura3D.Avalonia;

public class VeldridControl : OpenGL.OpenGlControlBase
{
    public IGlContext? glContext;

    GL? gl;
    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);

        this.gl = GL.GetApi(gl.GetProcAddress);

    }

    int fbo = 0;
    int color = 0;
    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        this.fbo = fb;
        this.gl.BindFramebuffer(FramebufferTarget.Framebuffer, (uint)this.fbo);
        this.gl.ClearColor(Color.FromArgb(255, color / 10, 0, 0));
        this.gl.Clear(ClearBufferMask.ColorBufferBit);
        if (color++ >= 2550)
            color = 0;
        RequestNextFrameRendering();
    }

    



}