using Aura3D.Avalonia;
using Aura3D.Core.Nodes;
using Aura3D.Core.Resources;
using Aura3D.Model;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Example.ViewModels;
using System;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace Example.Pages;

public partial class BlendSpacePage : UserControl
{
    AnimationBlendSpace? animationBlendSpace = null;
    public BlendSpacePage()
    {
        InitializeComponent();
    }

    private void aura3Dview_SceneInitialized(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var aura3DView = sender as Aura3DView;
        if (aura3DView == null)
            return;
        var dl = new DirectionalLight();

        dl.RotationDegrees = new Vector3(-30, 0, 0);

        dl.LightColor = Color.White;

        aura3DView.AddNode(dl);
        
        var model = AssimpLoader.Load("Models/SK_Mannequin.FBX");

        var forward = AssimpLoader.LoadAnimations("Models/Jog_Fwd_Rifle.FBX", model.Skeleton);
        var back = AssimpLoader.LoadAnimations("Models/Jog_Bwd_Rifle.FBX", model.Skeleton);
        var left = AssimpLoader.LoadAnimations("Models/Jog_Lt_Rifle.FBX", model.Skeleton);
        var right = AssimpLoader.LoadAnimations("Models/Jog_Rt_Rifle.FBX", model.Skeleton);
        var idle = AssimpLoader.LoadAnimations("Models/Idle_Rifle_Hip.FBX", model.Skeleton);

        animationBlendSpace = new AnimationBlendSpace(model.Skeleton);

        animationBlendSpace.AddAnimationSampler(new(0, 0), new AnimationSampler(idle.First()));
        animationBlendSpace.AddAnimationSampler(new(0, 1), new AnimationSampler(forward.First()));

        animationBlendSpace.AddAnimationSampler(new(0, -1), new AnimationSampler(back.First()));

        animationBlendSpace.AddAnimationSampler(new(-1, 0), new AnimationSampler(left.First()));

        animationBlendSpace.AddAnimationSampler(new(1, 0), new AnimationSampler(right.First()));

        model.AnimationSampler = animationBlendSpace;
        aura3Dview.AddNode(model);

        aura3Dview.MainCamera.FitToBoundingBox(model.BoundingBox);

        aura3Dview.MainCamera.ClearColor = Color.FromArgb(255, 100, 100, 100);


    }

    private void aura3Dview_SceneUpdated(object? sender, Aura3D.Avalonia.UpdateRoutedEventArgs e)
    {
        var vm = DataContext as BlendSpaceViewModel;
        if (vm == null)
            return;
        if (animationBlendSpace == null)
            return;
        animationBlendSpace.SetAxis((float)vm.X, (float)vm.Y);
    }
}