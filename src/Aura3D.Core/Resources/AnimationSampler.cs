using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Aura3D.Core.Resources;

public class AnimationSampler : IAnimationSampler
{
    public bool NeedUpdate { get; set; } = true;
    public AnimationSampler(Animation animation)
    {
        bonesTransform = new Matrix4x4[animation.Skeleton!.Bones.Count];
        this.animation = animation;
        Skeleton = animation.Skeleton!;
    }

    public Skeleton Skeleton { get; }
    public float TimeScale { get; set; } = 1.0f;

    protected Animation animation { get; set; }

    public IReadOnlyList<Matrix4x4> BonesTransform => bonesTransform;

    private Matrix4x4[] bonesTransform;

    private DateTime startTime { get; set; } = default;

    public LoopMode LoopMode { get; set; } = LoopMode.Loop;

    private bool pingPongForward { get; set; } = true;

    public void Update(double deltaTime)
    {
        if (startTime == default)
        {
            startTime = DateTime.Now;
        }

        if (DateTime.Now - startTime > TimeSpan.FromSeconds(animation.Duration / TimeScale))
        {
            if (LoopMode == LoopMode.Loop || LoopMode == LoopMode.PingPong)
            {
                startTime = DateTime.Now;
                pingPongForward = !pingPongForward;
            }
            else if (LoopMode == LoopMode.Once)
            {
                return;
            }
        }

        var time = (float)(DateTime.Now - startTime).TotalSeconds * TimeScale;

        if (pingPongForward == false)
        {
            time = animation.Duration - time;
        }

        processBoneTransform(Skeleton.Root, time);

    }

    private void processBoneTransform(Bone bone, float time)
    {
        var channelMatrix = animation.Sample(bone.Name, (float)((DateTime.Now - startTime).TotalSeconds * TimeScale));
        if (bone.Parent != null)
        {
            bonesTransform[bone.Index] = channelMatrix * BonesTransform[bone.Parent.Index];
        }
        else
        {
            bonesTransform[bone.Index] = channelMatrix;
        }
        foreach (var child in bone.Children)
        {
            processBoneTransform(child, time);
        }
    }

    public void Reset()
    {
        startTime = default;
    }

}

public enum LoopMode
{
    Once,
    Loop,
    PingPong
}

public enum CopyType
{
    SharedResource,
    SharedResourceData,
    FullCopy
}