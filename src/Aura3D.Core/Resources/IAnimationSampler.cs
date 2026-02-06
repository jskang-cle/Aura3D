using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Aura3D.Core.Resources;

public interface IAnimationSampler
{
    public bool NeedUpdate { get; set; }
    public IReadOnlyList<Matrix4x4> BonesTransform { get; }
    public void Update(double deltaTime);

    public void Reset();
}