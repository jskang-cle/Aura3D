using Aura3D.Core.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Aura3D.Core.Resources;

public class AnimationBlendSpace : IAnimationSampler
{
    public AnimationBlendSpace(Skeleton skeleton)
    {
        Skeleton = skeleton;

        BonesTransform = new Matrix4x4[skeleton.Bones.Count];
    }

    public Skeleton Skeleton { get; private set; }

    public bool NeedUpdate { get; set; }

    public IReadOnlyList<Matrix4x4> BonesTransform { get; private set; }

    Dictionary<Vector2, IAnimationSampler> AnimationSamplerMaps = [];

    public void AddAnimationSampler(Vector2 point, IAnimationSampler animationSampler)
    {
        if (point.X > 1 || point.X < -1)
            throw new Exception();

        if (point.Y > 1 || point.Y < -1)
            throw new Exception();

        AnimationSamplerMaps.Add(point, animationSampler);

    }
    Vector2 AxisValue = default;

    public void SetAxis(float x, float y)
    {
        if (x < -1 || y < -1 || x > 1 || y > 1)
            throw new Exception();
        AxisValue.X = x;
        AxisValue.Y = y;
    }

    public void Update(double deltaTime)
    {
        foreach(var (point, anim) in AnimationSamplerMaps)
        {
            float distance = CalculateDistance(AxisValue.X, AxisValue.Y, point.X, point.Y);
            
            if (distance < 0.000001)
            {
                
            }

        }
    }

    private float CalculateDistance(float x1, float y1, float x2, float y2)
    {
        float dx = x1 - x2;
        float dy = y1 - y2;
        return (float)MathF.Sqrt(dx * dx + dy * dy);
    }
}
