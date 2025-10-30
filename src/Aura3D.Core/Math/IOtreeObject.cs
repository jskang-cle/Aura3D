using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura3D.Core.Math;

public interface IOtreeObject
{
    BoundingBox? BoundingBox { get; }

    public event Action<IOtreeObject> OnChanged;
}
