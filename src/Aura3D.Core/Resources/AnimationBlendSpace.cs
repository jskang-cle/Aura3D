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

        bonesTransform = new Matrix4x4[skeleton.Bones.Count];
    }

    public Skeleton Skeleton { get; private set; }

    public bool NeedUpdate { get; set; } = true;

    public IReadOnlyList<Matrix4x4> BonesTransform => bonesTransform;

    public Matrix4x4[] bonesTransform;

    List <(Vector2, IAnimationSampler)> animationSamplers = [];

    List<float> weights = new List<float>();

    public void AddAnimationSampler(Vector2 point, IAnimationSampler animationSampler)
    {
        if (point.X > 1 || point.X < -1)
            throw new Exception();

        if (point.Y > 1 || point.Y < -1)
            throw new Exception();

        animationSamplers.Add((point, animationSampler));
        weights.Add(0);

    }
    Vector2 AxisValue = default;

    public void SetAxis(float x, float y)
    {
        if (x < -1 || y < -1 || x > 1 || y > 1)
            throw new Exception();
        AxisValue.X = x;
        AxisValue.Y = y;
    }
    public float IdwPower { get; set; } = 2f;

    public void Update(double deltaTime)
    {
        float totalRawWeight = 0f;

        int index = 0;
        foreach (var (point, anim) in animationSamplers)
        {
            float distance = CalculateDistance(AxisValue.X, AxisValue.Y, point.X, point.Y);
            
            if (distance < 0.000001)
            {
                anim.Update(deltaTime);

                for(int i = 0; i < BonesTransform.Count; i++)
                {
                    bonesTransform[i] = anim.BonesTransform[i];
                }
                return;
            }
            weights[index] = 1f / (float)MathF.Pow(distance, IdwPower);
            totalRawWeight += weights[index];

            index++;
        }

        index = 0;
        foreach (var result in weights)
        {
            float weight = result / totalRawWeight;
            if (weight > 0.001)
            {
                animationSamplers[index].Item2.Update(deltaTime);
                for (int i = 0; i < BonesTransform.Count; i++)
                {
                    if (index == 0)
                        bonesTransform[i] =  animationSamplers[index].Item2.BonesTransform[i] * weight;

                    bonesTransform[i] = bonesTransform[i] + animationSamplers[index].Item2.BonesTransform[i] * weight;
                }
            }
            index++;
        }

    }

    private float CalculateDistance(float x1, float y1, float x2, float y2)
    {
        float dx = x1 - x2;
        float dy = y1 - y2;
        return (float)MathF.Sqrt(dx * dx + dy * dy);
    }
}
