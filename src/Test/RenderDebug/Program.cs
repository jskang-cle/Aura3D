// See https://aka.ms/new-console-template for more information
using Aura3D.Core;
using Aura3D.Core.Nodes;
using Aura3D.Core.Renderers;
using Aura3D.Core.Resources;
using Aura3D.Core.Scenes;
using Aura3D.Model;
using Silk.NET.Windowing;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Numerics;

var window = Window.Create(WindowOptions.Default);
ControlRenderTarget controlRenderTarget = new ControlRenderTarget();
Camera.ControlRenderTarget = controlRenderTarget;
Scene scene = new Scene(scene => new NoLightPipeline(scene));
window.Load += () =>
{
    controlRenderTarget.Width = (uint)(window.Size.X);
    controlRenderTarget.Height = (uint)(window.Size.Y);
    controlRenderTarget.FrameBufferId = 0;

    scene.RenderPipeline.Initialize(str =>
    {
        window.GLContext.TryGetProcAddress(str, out var p);
        return p;
    });

    var camera = scene.MainCamera;

    camera.ClearColor = Color.Gray;
    camera.NearPlane = 1;

    var list = new List<Stream>();
    List<string> name =
    [
        "px.png",
        "nx.png",
        "py.png",
        "ny.png",
        "pz.png",
        "nz.png",
    ];
    foreach(var filename in name)
    {
        var stream = new StreamReader($"../../../../../../example/Example/Assets/Textures/skybox/{filename}").BaseStream;
        list.Add(stream);
    }

    var cubeTexture = TextureLoader.LoadCubeTexture(list);

    foreach (var stream in list)
    {
        stream.Dispose();
    }

    camera.ClearType = ClearType.Skybox;

    camera.SkyboxTexture = cubeTexture;


    var (model, animations) = AssimpLoader.LoadModelAndAnimations($"../../../../../../example/Example/Assets/Models/Soldier.glb");

    if (animations.Count > 0 && model.Skeleton != null)
    {

        var animationBlend = new AnimationBlendSpace(model.Skeleton);
        animationBlend.AddAnimationSampler(new(0, 1), new AnimationSampler(animations[0]));
        animationBlend.AddAnimationSampler(new(0, -1), new AnimationSampler(animations[1]));
        animationBlend.AddAnimationSampler(new(1, 0), new AnimationSampler(animations[2]));
        animationBlend.AddAnimationSampler(new(-1, 0), new AnimationSampler(animations[3]));

        model.AnimationSampler = new AnimationSampler(animations[0]);
    }

    model.RotationDegrees = new Vector3(0, 180, 0);

    List<Color> colors = [];
    if (model.Skeleton != null)
    {
        int i = 0;
        colors.Clear();
        foreach (var bone in model.Skeleton.Bones)
        {
            Console.WriteLine($"{i}:\t {bone.Name}");

            if (i >= 0 && i <= 3)
                colors.Add(Color.Red);

            else if (i >= 4 && i <= 5)
                colors.Add(Color.Green);

            else if (i >= 6 && i <= 22)
                colors.Add(Color.Blue);

            else if (i >= 23 && i <= 40)
                colors.Add(Color.Blue);

            else if (i >= 41 && i <= 44)
                colors.Add(Color.Green);

            else if (i >= 45 && i <= 48)
                colors.Add(Color.Green);
            else
            {

            }
            i++;

        }
    }
    
    foreach (var mesh in model.Meshes)
    {
        if (mesh.Material == null)
            continue;
        mesh.Material.SetShaderPassParametersCallback("NoLightPass", pass =>
        {
            int i = 0;
            foreach (var color in colors)
            {
                pass.UniformColor($"boneColor[{i}]", color);
                i++;
            }
        });
        mesh.Material?.SetShaderSource("NoLightPass", ShaderType.Fragment, @"#version 300 es
precision mediump float;
out vec4 outColor;

#define BONE_NUMBER 150

in vec2 vTexCoord;
in vec4 myColor;

uniform sampler2D BaseColorTexture;
uniform vec4 BaseColor;
uniform int HasBaseColorTexture;
uniform float alphaCutoff;


void main()
{
	vec4 baseColor = BaseColor;

	if (HasBaseColorTexture == 1)
	{
		baseColor = texture(BaseColorTexture, vTexCoord);
	}

	#if defined(BLENDMODE_MASKED) || defined(BLENDMODE_TRANSLUCENT)
		if (baseColor.a <= alphaCutoff)
			discard;
	#endif
	outColor = baseColor * myColor;
}");

        mesh.Material?.SetShaderSource("NoLightPass", ShaderType.Vertex, @"#version 300 es
precision mediump float;

#define BONE_NUMBER 150

//{{defines}}

layout(location = 0) in vec3 position;
layout(location = 1) in vec2 texCoord;
layout(location = 2) in vec3 normal;
layout(location = 3) in vec3 tangent;
layout(location = 4) in vec3 bitangent;

layout(location = 5) in vec4 boneIndices;
layout(location = 6) in vec4 boneWeights;

#ifdef SKINNED_MESH

uniform mat4 BoneMatrices[BONE_NUMBER];

#endif

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

uniform mat4 normalMatrix;
uniform vec4 boneColor[BONE_NUMBER];

out vec2 vTexCoord;
out vec4 myColor;
void main()
{
	vTexCoord = texCoord;
	
	
	vec4 boneIds = boneIndices;
	vec4 boneWeightsOut = boneWeights;


	vec4 color1 = boneColor[int(boneIds.x)];
	vec4 color2 = boneColor[int(boneIds.y)];

	vec4 color3 = boneColor[int(boneIds.z)];
	vec4 color4 = boneColor[int(boneIds.w)];

	vec4 color = color1 * boneWeightsOut.x + color2 * boneWeightsOut.y + color3 * boneWeightsOut.z + color4 * boneWeightsOut.w;
	
	myColor = color;
#ifdef SKINNED_MESH
		
	mat4 skinMatrix = boneWeights.x * BoneMatrices[int(boneIndices.x)];
	skinMatrix += boneWeights.y * BoneMatrices[int(boneIndices.y)];
	skinMatrix += boneWeights.z * BoneMatrices[int(boneIndices.z)];
	skinMatrix += boneWeights.w * BoneMatrices[int(boneIndices.w)];

	vec4 worldPosition = modelMatrix * skinMatrix * vec4(position, 1.0);

#else
	vec4 worldPosition = modelMatrix * vec4(position, 1.0);
#endif

	gl_Position = projectionMatrix * viewMatrix * worldPosition;
}");
    }


    AddNode(model);

    camera.FitToBoundingBox(model.BoundingBox);


    DirectionalLight dl = new DirectionalLight();

    dl.CastShadow = true;

    dl.RotationDegrees = new Vector3(-45, 45, 0);

    AddNode(dl);
};


window.Render += (delta) =>
{
    if (window.WindowState == WindowState.Minimized)
        return;

    controlRenderTarget.Width = (uint)(window.Size.X);
    controlRenderTarget.Height = (uint)(window.Size.Y);
    scene.RenderPipeline.DefaultFramebuffer = (uint)0;

    scene.RenderPipeline.Render();

    scene.Update(delta);




};

window.Run();


void AddNode<T>(T node) where T : Node
{
    scene.AddNode(node);
}
