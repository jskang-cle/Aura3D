# Get Started
## Installation
Add the Aura3D.Avalonia package to the Avalonia project:

```shell
dotnet add package Aura3D.Avalonia
```

Then use the Aura3DView control in your project and bind the SceneInitialized event:

```xaml
<Window
    ...
    xmlns:a="https://sunce.tech/aura3d"
    ...>
	<a:Aura3DView x:Name="aura3Dview" SceneInitialized="OnSceneInitialized"/>
</Window>
```

Initialize your scene in the SceneInitialized event:

```CSharp
 public void OnSceneInitialized(object sender, RoutedEventArgs args)
 {
 }
```

## Camera

A camera is the entry point for viewing the scene, so there must be a camera in the scene to display the image.

```CSharp
public void OnSceneInitialized(object sender, RoutedEventArgs args)
{
    var view = (Aura3DView)sender;

    // Version 0.0.1 requires manual instantiation of the camera
    var camera = new Camera();
    // For other versions or newer releases, the main camera is built-in
    var camera = view.MainCamera;

    camera.ClearColor = Color.Gray;

	view.AddNode(camera);  
}
```

## Models

Aura3D supports [GLTF](https://www.khronos.org/gltf/) format models by default, and will support more model formats through extension packages in the future.

### Static Models

```CSharp
...

var model = ModelLoader.LoadGlbModel("your model file path(*.glb)");

model.Position = camera.Forward * 3;

view.AddNode(model);
```

### Skeletal Models and Skinning Animations

```CSharp
...

var (model, animations) = ModelLoader.LoadGlbModelAndAnimations(s);

model.AnimationSampler = new AnimationSampler(animations.First());

model.Position = camera.Forward * 3;

view.AddNode(model);

```

## Lights

Aura3D's default lighting model is the photorealistic [Blinn-Phong](https://handwiki.org/wiki/Blinn%E2%80%93Phong_reflection_model) empirical model, so there must be light sources in the scene to see the models.

> [!WARNING]
> The default pipeline is the forward pipeline, and due to performance limitations, each type of light supports a maximum of 4 lights.

### Directional Light

Used to simulate very distant light sources. When the light source is far enough away, each ray of the light source is approximately parallel, such as sunlight.

```CSharp

var dl = new DirectionalLight();

dl.LightColor = Color.Red;

dl.RotationDegrees = new Vector3(-24, 0, 0);

view.AddNode(dl);

```
### Point Light

As the name suggests, light emitted from a single point, such as incandescent lamps, torches, etc.

```CSharp
var pl = new PointLight();

pl.LightColor = Color.Green;

pl.AttenuationRadius = 5;

view.AddNode(pl);
```

### Spot Light

Flashlights, stage spotlights, light barrels, and other light sources.

```CSharp
var sp = new SpotLight();

sp.LightColor = Color.Blue;

sp.AttenuationRadius = 5;

sp.InnerConeAngleDegree = 15;

sp.OuterAngleDegree = 17;

view.AddNode(sp);
```