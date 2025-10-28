using Avalonia.OpenGL;
using Avalonia.OpenGL.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Aura3D.Avalonia;

public class VeldridControl : OpenGlControlBase
{
    GraphicsDevice? device;

    Action? render;

    Action<uint>? setFbo;

    protected override void OnOpenGlInit(GlInterface gl)
    {
        base.OnOpenGlInit(gl);

        device = GraphicsDevice.CreateOpenGL(false, gl.GetProcAddress, new Size((int)Bounds.Width, (int)Bounds.Height), out render, out setFbo);

    }
    protected override void OnOpenGlRender(GlInterface gl, int fb)
    {
        var factory = device.ResourceFactory;

        var cmdlist = factory.CreateCommandList();

        cmdlist.Begin();
        cmdlist.SetFramebuffer(device.MainSwapchain.Framebuffer);

        cmdlist.ClearColorTarget(0, RgbaFloat.CornflowerBlue);

        cmdlist.End();
        device.SubmitCommands(cmdlist);

        setFbo?.Invoke((uint)fb);
        render?.Invoke();
    }
}
