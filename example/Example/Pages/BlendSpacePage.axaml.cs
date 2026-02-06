using Aura3D.Avalonia;
using Aura3D.Core;
using Aura3D.Core.Nodes;
using Aura3D.Core.Resources;
using Aura3D.Model;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Example.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
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

    List<Animation> animations = [];

    Model? model = null;

    private void aura3Dview_SceneInitialized(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var aura3DView = sender as Aura3DView;
        if (aura3DView == null)
            return;
        var dl = new DirectionalLight();

        dl.RotationDegrees = new Vector3(-30, 0, 0);

        dl.LightColor = Color.White;

        aura3DView.AddNode(dl);

        if (model == null)
        {
            using (var stream = AssetLoader.Open(new Uri($"avares://Example/Assets/Models/SK_Mannequin.FBX")))
            {
                model = AssimpLoader.Load(stream, "fbx");
            }
        }

        if (animations.Count == 0)
        {
            using (var stream = AssetLoader.Open(new Uri($"avares://Example/Assets/Models/Idle_Rifle_Hip.FBX")))
            {
                animations.AddRange(AssimpLoader.LoadAnimations(stream, model.Skeleton, "fbx"));
            }
            using (var stream = AssetLoader.Open(new Uri($"avares://Example/Assets/Models/Jog_Fwd_Rifle.FBX")))
            {
                animations.AddRange(AssimpLoader.LoadAnimations(stream, model.Skeleton, "fbx"));
            }


            using (var stream = AssetLoader.Open(new Uri($"avares://Example/Assets/Models/Jog_Bwd_Rifle.FBX")))
            {
                animations.AddRange(AssimpLoader.LoadAnimations(stream, model.Skeleton, "fbx"));
            }

            using (var stream = AssetLoader.Open(new Uri($"avares://Example/Assets/Models/Jog_Lt_Rifle.FBX")))
            {
                animations.AddRange(AssimpLoader.LoadAnimations(stream, model.Skeleton, "fbx"));
            }

            using (var stream = AssetLoader.Open(new Uri($"avares://Example/Assets/Models/Jog_Rt_Rifle.FBX")))
            {
                animations.AddRange(AssimpLoader.LoadAnimations(stream, model.Skeleton, "fbx"));
            }
        }


        animationBlendSpace = new AnimationBlendSpace(model.Skeleton);

        animationBlendSpace.AddAnimationSampler(new(0, 0), new AnimationSampler(animations.First()));

        animationBlendSpace.AddAnimationSampler(new(0, 1), new AnimationSampler(animations.Skip(1).First()));

        animationBlendSpace.AddAnimationSampler(new(0, -1), new AnimationSampler(animations.Skip(2).First()));

        animationBlendSpace.AddAnimationSampler(new(-1, 0), new AnimationSampler(animations.Skip(3).First()));

        animationBlendSpace.AddAnimationSampler(new(1, 0), new AnimationSampler(animations.Skip(4).First()));

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