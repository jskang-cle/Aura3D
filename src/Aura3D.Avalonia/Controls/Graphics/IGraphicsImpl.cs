using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;

namespace Aura3D.Avalonia.Controls;

internal interface IGraphicsImpl
{
    public Task<GraphicsDevice> Initialize();

    public void Resize(int width, int height);
}
