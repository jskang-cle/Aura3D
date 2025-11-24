using Aura3D.Avalonia;
using Aura3D.Core.Geometries;
using Aura3D.Core.Nodes;
using Aura3D.Core.Resources;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace Example.Pages;

public partial class FrustumCullingPage : UserControl
{
    bool _isPressed = false;

    Avalonia.Point point = new(-1, -1);

    public FrustumCullingPage()
    {
        InitializeComponent();
        aura3Dview.Focusable = true;
        checkbox.IsChecked = true;
        this.aura3Dview.PointerPressed += (s, e) =>
        {
            _isPressed = true;
            point = new(-1, -1);

        };

        this.aura3Dview.PointerReleased += (s, e) =>
        {
            _isPressed = false;
            point = new(-1, -1);
        };

        this.aura3Dview.PointerMoved += (s, e) =>
        {
            if (_isPressed == false)
                return;
            if (e.Pointer.IsPrimary == false)
                return;

            var newPosition = e.GetCurrentPoint(this).Position;
            if (point.X != -1 && point.Y != -1)
            {
                var delta = newPosition - point;

                aura3Dview.MainCamera!.RotationDegrees = new Vector3(
                    (float)(aura3Dview.MainCamera.RotationDegrees.X + (float)delta.Y * (float)deltaTime * 20),
                    (float)(aura3Dview.MainCamera.RotationDegrees.Y + (float)delta.X * (float)deltaTime * 20f), 0);

            }
            point = newPosition;

        };

        this.aura3Dview.KeyDown += (s, e) =>
        {
            if (e.Key == Avalonia.Input.Key.W)
            {
                aura3Dview.MainCamera!.Position += aura3Dview.MainCamera.Forward * (float)deltaTime;
            }
            else if (e.Key == Avalonia.Input.Key.S)
            {
                aura3Dview.MainCamera!.Position -= aura3Dview.MainCamera.Forward * (float)deltaTime;
            }
            else if (e.Key == Avalonia.Input.Key.A)
            {
                aura3Dview.MainCamera!.Position -= aura3Dview.MainCamera.Right * (float)deltaTime;
            }
            else if (e.Key == Avalonia.Input.Key.D)
            {
                aura3Dview.MainCamera!.Position += aura3Dview.MainCamera.Right * (float)deltaTime;
            }
        };

    }
    private void Aura3DView_SceneInitialized(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var view = sender as Aura3DView;

        if (view == null)
        {
            return;
        }

        int num = 10000; // 100个mesh对象
        float spacing = 1.5f; // Cube间距（>1避免重叠，可按需调整）

        // 计算10×10网格的居中偏移（让整体以原点为中心）
        int gridSize = (int)Math.Sqrt(num);
        float offset = (gridSize - 1) * spacing / 2f;

        int currentIndex = 0; // 已创建的mesh计数
        var box = new BoxGeometry();
        var material = new Material
        {
            BlendMode = BlendMode.Opaque,
            Channels = new List<Channel>
                    {
                        new Channel()
                        {
                            Name = "BaseColor",
                            Color = Color.White,
                        }
                    }
        };
        for (int xIndex = 0; xIndex < gridSize; xIndex++)
        {
            for (int zIndex = 0; zIndex < gridSize; zIndex++)
            {
                // 达到100个则停止创建
                if (currentIndex >= num) break;

                var mesh = new Mesh();

                mesh.Geometry = box;

                mesh.Material = material;

                // 核心：计算均匀分布的位置（Y固定为0）
                // X坐标：网格列×间距 - 居中偏移
                float xPos = xIndex * spacing - offset;
                // Y坐标固定为0
                float yPos = -1f;
                // Z坐标：网格行×间距 - 居中偏移
                float zPos = zIndex * spacing - offset;

                // 设置mesh的Position（需匹配你项目中Position的赋值方式）
                // 若Position是Vector3结构体，直接赋值；若为单独的X/Y/Z属性，分别赋值
                mesh.Position = new Vector3(xPos, yPos, zPos);
                // 若你的Position是分开的属性，替换为：
                // mesh.PositionX = xPos;
                // mesh.PositionY = yPos;
                // mesh.PositionZ = zPos;

                view.AddNode(mesh);
                currentIndex++;
            }
            if (currentIndex >= num) break;
        }

        var dl = new DirectionalLight();

        dl.RotationDegrees = new Vector3(-30, -15, 0);

        dl.LightColor = Color.Red;

        view.AddNode(dl);

    }

    double deltaTime = 0;
    private void aura3Dview_SceneUpdated(object? sender, UpdateRoutedEventArgs args)
    {
        var view = (Aura3DView)sender;
        deltaTime = args.DeltaTime;
        Console.WriteLine(deltaTime);
    }

    private void CheckBox_IsCheckedChanged(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (aura3Dview.Scene == null)
            return;
        if (checkbox.IsChecked == null || checkbox.IsChecked == false)
        {
            
            aura3Dview.Scene.RenderPipeline.EnableFrustumCulling = false;
        }
        else
        {
            aura3Dview.Scene.RenderPipeline.EnableFrustumCulling = true;
        }
    }
}