
<div id="header" align="center">
	<img width="200px" src="./logo.svg" ></img> 
	<h4><i>一个轻量级、可扩展、高性能的3D渲染控件</i></h4>
	<div id="link" >
		<a href="./README.md">English</a> | 
		<span>中文</span> |
		<a href="./doc/cn/home.md">文档</a> 
	</div>
</div>
<br/>

![demo](./doc/images/demo.png)


> [!IMPORTANT]
> 项目正在积极开发中，欢迎大家踊跃提出建议。


## 特性
### 1. 基础功能
- 模型渲染
- 光源
- 投影
- 蒙皮动画
- 八叉树场景管理
- 视锥体剔除
- 默认布林冯渲染管线
### 2. 进阶功能
- 自定义管线 
### 3. 支持平台
- Avalonia

## 开始上手

在 Avalonia 项目中添加 Aura3D.Avalonia 包：

```shell
dotnet add package Aura3D.Avalonia
```

然后在你的项目中使用 Aura3DView 控件, 并绑定 SceneInitialized 事件：

```xaml
<Window
    ...
    xmlns:a="https://sunce.tech/aura3d"
    ...>
	<a:Aura3DView x:Name="aura3Dview" SceneInitialized="OnSceneInitialized"/>
</Window>
```

在 SceneInitialized 事件中，初始化你的场景：

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

	var model = ModelLoader.LoadGlbModel("your model file path(*.glb)");

	model.Position = camera.Forward * 3;

	view.AddNode(model);

 }
```

