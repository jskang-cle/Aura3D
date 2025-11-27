# 开始上手
## 安装
在 Avalonia 项目中添加 Aura3D.Avalonia 包：

```shell
dotnet add package Aura3D.Avalonia
```

然后在你的项目中使用 Aura3DView 控件, 并绑定 SceneInitialized 事件：

```xaml
<Window
    ...
    xmlns:aura3d="clr-namespace:Aura3D.Avalonia;assembly=Aura3D.Avalonia"
    ...>
	<aura3d:Aura3DView x:Name="aura3Dview" SceneInitialized="OnSceneInitialized"/>
</Window>
```

在 SceneInitialized 事件中，初始化你的场景：

```CSharp
 public void OnSceneInitialized(object sender, RoutedEventArgs args)
 {
 }
```

## 摄像机

摄像机是观察场景的入口，所以场景中必须有一个摄像机才能显示出画面

```CSharp
public void OnSceneInitialized(object sender, RoutedEventArgs args)
{
    var view = (Aura3DView)sender;

    // 0.0.1版本需要手动new相机
    var camera = new Camera();
    // 其他或者更新版本内置了主相机
    var camera = view.MainCamera;
    
    camera.ClearColor = Color.Gray;

	view.AddNode(camera);  
}
```

## 模型
Aura3D 默认支持 [GLTF](https://www.khronos.org/gltf/) 格式模型，后续将通过扩展包的方式支持更多的模型格式。

### 静态模型

```CSharp
...

var model = ModelLoader.LoadGlbModel("your model file path(*.glb)");

model.Position = camera.Forward * 3;

view.AddNode(model);
```

### 骨骼模型和蒙皮动画

```CSharp
...

var (model, animations) = ModelLoader.LoadGlbModelAndAnimations(s);

model.AnimationSampler = new AnimationSampler(animations.First());

model.Position = camera.Forward * 3;

view.AddNode(model);

```

## 光源
Aura3D 默认的光照模型是写实风格的 [Blinn-Phong](https://handwiki.org/wiki/Blinn%E2%80%93Phong_reflection_model) 经验模型，所以场景中必须有光源才能看到模型。

> [!WARNING]
> 默认管线是前向管线，由于性能限制，每种光源最多支持4盏。

### 方向光
用于模拟特别远的光源，当光源距离足够远时，光源的每条光线近似平行的，例如太阳光
```CSharp
var dl = new DirectionalLight();

dl.LightColor = Color.Red;

dl.RotationDegrees = new Vector3(-24, 0, 0);

view.AddNode(dl);
```
### 点光
顾名思义，从一个点发出的光，例如白炽灯，火把等。
```CSharp
var pl = new PointLight();

pl.LightColor = Color.Green;

pl.AttenuationRadius = 5;

view.AddNode(pl);
```

### 聚光灯
手电筒，舞台的聚光灯，灯筒等光源
```CSharp
var sp = new SpotLight();

sp.LightColor = Color.Blue;

sp.AttenuationRadius = 5;

sp.InnerConeAngleDegree = 15;

sp.OuterAngleDegree = 17;

view.AddNode(sp);
```