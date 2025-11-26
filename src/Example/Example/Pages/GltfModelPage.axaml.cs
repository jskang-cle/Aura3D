using Aura3D.Avalonia;
using Aura3D.Core;
using Aura3D.Core.Nodes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using System;
using System.Drawing;
using System.Numerics;
using System.Threading.Tasks;
using Ursa.Common;
using Ursa.Controls;

namespace Example.Pages;

public partial class GltfModelPage : UserControl
{
    Model? lion;

    Model? solider;

    Model? woodenStool;

    Model? _currentModel;

    Vector3 modelPosition;
    Model? currentModel
    {
        get => _currentModel;
        set 
        {
            if (_currentModel == value)
                return;
            if (aura3d.Scene == null)
                return;
            if (_currentModel != null)
                aura3d.Scene.RemoveNode(_currentModel);
            if (value != null)
            {
                _currentModel = value;
                aura3d.Scene.AddNode(_currentModel);

            }
        }
    }
    public GltfModelPage()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

    }

    private void Aura3DView_SceneInitialized(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var view = sender as Aura3DView;
        if (view == null)
        {
            return;
        }

        modelPosition = view.MainCamera.Position + view.MainCamera.Forward * 2;

        var dl = new DirectionalLight();

        dl.RotationDegrees = new Vector3(-30, 0, 0);

        dl.LightColor = Color.White;

        view.AddNode(dl);
    }

    private void Aura3DView_SceneUpdated(object? sender, Aura3D.Avalonia.UpdateRoutedEventArgs e)
    {
    }

    private async void Button_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {

        var button = sender as Button;
        if (button == null)
            return;
        var s = button.Content?.ToString();
        switch (s)
        {
            case "lion head":
                if (lion == null)
                {
                    lionButton.IsEnabled = false;
                    lion = await Task.Run(() =>
                    {
                        using (var stream = AssetLoader.Open(new Uri($"avares://Example/Assets/Models/lion_head_1k.glb")))
                        {
                            var model = ModelLoader.LoadGlbModel(stream);
                            model.Position = modelPosition;
                            model.Position = modelPosition - model.Up * 1;
                            model.Scale = Vector3.One * 4;
                            model.RotationDegrees = Vector3.Zero;
                            return model;
                        }
                    });
                    lionButton.IsEnabled = true;
                }
                currentModel = lion;
                break;
            case "soldier":
                if (solider == null)
                {
                    soldierButton.IsEnabled = false;
                    solider = await Task.Run(() =>
                    {
                        using (var stream = AssetLoader.Open(new Uri($"avares://Example/Assets/Models/Soldier.glb")))
                        {
                            var model = ModelLoader.LoadGlbModel(stream);
                            model.Position = modelPosition;
                            model.Position = modelPosition - model.Up * 1;
                            model.RotationDegrees = new Vector3(0, 180, 0);
                            return model;
                        }
                    });
                    soldierButton.IsEnabled = true;
                }
                currentModel = solider;
                break;
            case "wooden stool":
                if (woodenStool == null)
                {
                    woodenButton.IsEnabled = false;
                    woodenStool = await Task.Run(() =>
                    {
                        using (var stream = AssetLoader.Open(new Uri($"avares://Example/Assets/Models/wooden_stool_02_1k.glb")))
                        {
                            var model = ModelLoader.LoadGlbModel(stream);
                            model.Position = modelPosition;
                            model.Position = modelPosition - model.Up * 1;
                            model.Scale = Vector3.One * 5;
                            model.RotationDegrees = Vector3.Zero;
                            return model;
                        }
                    });
                    woodenButton.IsEnabled = true;
                }
                currentModel = woodenStool;
                break;
            default:
                break;
        }
    }
}