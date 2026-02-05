using Aura3D.Core.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Aura3D.Core.Resources;

public class AnimationBlend : IAnimationSampler
{
    public AnimationBlend(Skeleton skeleton)
    {
        Skeleton = skeleton;

        BonesTransform = new Matrix4x4[skeleton.Bones.Count];
    }

    public Skeleton Skeleton { get; private set; }

    public bool NeedUpdate { get; set; }

    public IReadOnlyList<Matrix4x4> BonesTransform { get; private set; }

    Dictionary<Vector2, IAnimationSampler> AnimationSamplerMaps = [];

    List<(float, IAnimationSampler)> XAxisSampmplers = [];
    List<(float, IAnimationSampler)> YAxisSampmplers = [];

    public void AddAnimationSampler(Vector2 point, IAnimationSampler animationSampler)
    {
        if (point.X > 1 || point.X < -1)
            throw new Exception();

        if (point.Y > 1 || point.Y < -1)
            throw new Exception();

        AnimationSamplerMaps.Add(point, animationSampler);
        XAxisSampmplers.Add((point.X, animationSampler));
        YAxisSampmplers.Add((point.Y, animationSampler));

        XAxisSampmplers.Sort((left, right) =>
        {
            return right.Item1 > left.Item1 ? -1 : 1;
        });

        YAxisSampmplers.Sort((left, right) =>
        {
            return right.Item1 > left.Item1 ? -1 : 1;
        });

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

    }
}
