using Assimp;
using Aura3D.Core;
using Aura3D.Core.Math;
using Aura3D.Core.Nodes;
using Aura3D.Core.Resources;
using StbImageSharp;
using System.Drawing;
using System.Numerics;

namespace Aura3D.Model;

public static class AssimpLoader
{
    public unsafe static Core.Nodes.Model Load(string path, Func<string, Core.Resources.Texture>? loadTextureFunc = null)
    {
        var defaultFlags = PostProcessSteps.Triangulate
                  | PostProcessSteps.GenerateNormals
                  | PostProcessSteps.OptimizeMeshes
                  | PostProcessSteps.CalculateTangentSpace
                | PostProcessSteps.GenerateUVCoords;

        var importer = new AssimpContext();

        var directory = Path.GetDirectoryName(path);

        var scene = importer.ImportFile(path, defaultFlags);

        var model = processScene(scene, directory,  loadTextureFunc); 

        var skeleton = processSkeleton(scene);

        model.Skeleton = skeleton;

        foreach (var mesh in model.Meshes)
        {
            mesh.Model = model;
        }

        return model;

    }


    public unsafe static List<Core.Resources.Animation> LoadAnimations(string path)
    {
        var defaultFlags = PostProcessSteps.Triangulate
                  | PostProcessSteps.GenerateNormals
                  | PostProcessSteps.OptimizeMeshes
                  | PostProcessSteps.CalculateTangentSpace
                | PostProcessSteps.GenerateUVCoords;

        var importer = new AssimpContext();

        var directory = Path.GetDirectoryName(path);

        var scene = importer.ImportFile(path, defaultFlags);

        var animations = processAnimations(scene);

        return animations;
    }

    public unsafe static List<Core.Resources.Animation> LoadAnimations(Stream stream)
    {
        var defaultFlags = PostProcessSteps.Triangulate
                  | PostProcessSteps.GenerateNormals
                  | PostProcessSteps.OptimizeMeshes
                  | PostProcessSteps.CalculateTangentSpace
                | PostProcessSteps.GenerateUVCoords;

        var importer = new AssimpContext();

        var scene = importer.ImportFileFromStream(stream, defaultFlags);

        var animations = processAnimations(scene);

        return animations;
    }

    private unsafe static List<Core.Resources.Animation> processAnimations(Scene scene)
    {
        List<Core.Resources.Animation> animations = [];

        foreach (var assAnimation in scene.Animations)
        {
            if (assAnimation.HasNodeAnimations == false)
                continue;
            var animation = new Core.Resources.Animation();

            float maxTime = 0;
            foreach(var assChannel in assAnimation.NodeAnimationChannels)
            {
                var channel = new AnimationChannel();

                foreach(var posKey in assChannel.PositionKeys)
                {
                    channel.PositionKeyframes.Add(new Keyframe<Vector3>
                    {
                        Time = (float)(posKey.Time / assAnimation.TicksPerSecond),
                        Value = new Vector3(posKey.Value.X, posKey.Value.Y, posKey.Value.Z),
                    });
                    if (channel.PositionKeyframes.Last().Time > maxTime)
                        maxTime = channel.PositionKeyframes.Last().Time;
                }
                foreach (var rotKey in assChannel.RotationKeys)
                {
                    channel.RotationKeyframes.Add(new Keyframe<Quaternion>
                    {
                        Time = (float)(rotKey.Time / assAnimation.TicksPerSecond),
                        Value = new Quaternion(rotKey.Value.X, rotKey.Value.Y, rotKey.Value.Z, rotKey.Value.W),
                    });
                    if (channel.RotationKeyframes.Last().Time > maxTime)
                        maxTime = channel.RotationKeyframes.Last().Time;
                }

                foreach (var scaleKey in assChannel.ScalingKeys)
                {
                    channel.ScaleKeyframes.Add(new Keyframe<Vector3>
                    {
                        Time = (float)(scaleKey.Time / assAnimation.TicksPerSecond),
                        Value = new Vector3(scaleKey.Value.X, scaleKey.Value.Y, scaleKey.Value.Z),
                    });
                    if (channel.ScaleKeyframes.Last().Time > maxTime)
                        maxTime = channel.ScaleKeyframes.Last().Time;
                }

                animation.Channels.Add(assChannel.NodeName, channel);
            }   
            animation.Name = assAnimation.Name;
            animation.Duration = maxTime;
            animations.Add(animation);
        }
        return animations;
    }

    public unsafe static Core.Nodes.Model Load(Stream stream, Func<string, Core.Resources.Texture>? loadTextureFunc = null)
    {

        var defaultFlags = PostProcessSteps.Triangulate
                  | PostProcessSteps.GenerateNormals
                  | PostProcessSteps.OptimizeMeshes
                  | PostProcessSteps.CalculateTangentSpace
                | PostProcessSteps.GenerateUVCoords;

        var importer = new AssimpContext();

        var scene = importer.ImportFileFromStream(stream, defaultFlags);

        var model =  processScene(scene, null, loadTextureFunc);

        var skeleton = processSkeleton(scene);

        model.Skeleton = skeleton;

        foreach(var mesh in model.Meshes)
        {
            mesh.Model = model;
        }

        return model;
    }


    private unsafe static Core.Nodes.Model processScene(Scene scene, string? directory, Func<string, Core.Resources.Texture>? loadTextureFunc)
    {

        Dictionary<int, Core.Resources.Material> materialsMap = new();

        var model = new Core.Nodes.Model();

        processMaterial(scene, materialsMap, directory, loadTextureFunc);

        processNode(scene, scene.RootNode, model, materialsMap);

        return model;
    }


    public static unsafe void processMaterial(Scene scene, Dictionary<int, Core.Resources.Material> materialsMap, string? path, Func<string, Core.Resources.Texture>? loadTextureFunc)
    {
        for (int i = 0; i < scene.MaterialCount; i++)
        {
            var material = new Core.Resources.Material();

            var assimpMaterial = scene.Materials[i];

            if (assimpMaterial.HasTwoSided)
            {
                material.DoubleSided = assimpMaterial.IsTwoSided;
            }

            if (assimpMaterial.BlendMode == Assimp.BlendMode.Default)
            {
                material.BlendMode = Core.Resources.BlendMode.Opaque;
            }

            Core.Resources.Texture? texture = null;
            
            if (assimpMaterial.HasTextureDiffuse && assimpMaterial.TextureDiffuse.FilePath != null)
            {
                var slot = assimpMaterial.TextureDiffuse;
                texture = processTexture(scene, slot, path, loadTextureFunc);

               
            }
            else if (assimpMaterial.PBR.HasTextureBaseColor && assimpMaterial.PBR.TextureBaseColor.FilePath != null)
            {
                var slot = assimpMaterial.PBR.TextureBaseColor;
                texture = processTexture(scene, slot, path, loadTextureFunc);

            }
            if (texture != null)
            {
                texture.IsGammaSpace = true;
            }
            material.Channels.Add(new Channel
            {
                Name = "BaseColor",
                Texture = texture,
                Color = Color.White,
            });

            if (assimpMaterial.HasTextureNormal && assimpMaterial.TextureNormal.FilePath != null)
            {
                var slot = assimpMaterial.TextureNormal;
                texture = processTexture(scene, slot, path, loadTextureFunc);

                material.Channels.Add(new Channel
                {
                    Name = "Normal",
                    Texture = texture,
                    Color = Color.White,
                });

            }


            materialsMap.Add(i, material);
        }
    }


    private static unsafe Core.Resources.Texture? processTexture(Scene scene, TextureSlot textureSlot, string? path, Func<string, Core.Resources.Texture>? loadTextureFunc)
    {
        EmbeddedTexture? assimpTexture = null;

        if (textureSlot.FilePath[0] == '*')
        {
            assimpTexture = scene.Textures[int.Parse(textureSlot.FilePath.Skip(1).Take(textureSlot.FilePath.Length - 1).ToArray())];
        }
        else if (scene.TextureCount > 0)
        {
            assimpTexture = scene.Textures.FirstOrDefault(texture => texture.Filename == textureSlot.FilePath);
        }
        if (assimpTexture != null)
        {
            if (assimpTexture.IsCompressed == true)
            {
                StbImage.stbi_set_flip_vertically_on_load_thread(1);
                try
                {
                    return TextureLoader.LoadTexture(assimpTexture.CompressedData);

                }
                finally
                {
                    StbImage.stbi_set_flip_vertically_on_load_thread(0);
                }
            }
            else if (assimpTexture.HasNonCompressedData)
            {

                var texture = new Core.Resources.Texture();


                texture.Width = (uint)assimpTexture.Width;
                texture.Height = (uint)assimpTexture.Height;

                texture.IsHdr = false;

                texture.ColorFormat = ColorFormat.RGBA;

                List<byte> data = [];
                for (int i = 0; i < assimpTexture.NonCompressedDataSize; i++)
                {
                    data.Add(assimpTexture.NonCompressedData[i].R);
                    data.Add(assimpTexture.NonCompressedData[i].G);
                    data.Add(assimpTexture.NonCompressedData[i].B);
                    data.Add(assimpTexture.NonCompressedData[i].A);
                }
                texture.LdrData = data;
                return texture;
            }
            else
            {
                throw new Exception("error texture");
            }
        }
        else
        {
            if (loadTextureFunc!=null)
            {
                return loadTextureFunc(textureSlot.FilePath);
            }
            else if (path != null)
            {

                var filePath = Path.Combine(path, textureSlot.FilePath);

                StbImage.stbi_set_flip_vertically_on_load_thread(1);
                try
                {
                    using (var sr = new StreamReader(filePath))
                    {
                        return TextureLoader.LoadTexture(sr.BaseStream);
                    }
                }
                catch(FileNotFoundException)
                {
                    return null;
                }
                finally
                {

                    StbImage.stbi_set_flip_vertically_on_load_thread(0);
                }
            }
            else
            {
                return null;
            }
        }

    }

    private static unsafe void processNode(Scene scene, Assimp.Node node, Core.Nodes.Node parent, Dictionary<int, Core.Resources.Material> materialMap)
    {
        var currentNode = new Core.Nodes.Node();

        parent.AddChild(currentNode);

        currentNode.LocalTransform = node.Transform;

        currentNode.Name = node.Name;

        for(int i = 0; i < node.MeshCount; i ++)
        {
            var mesh = new Core.Nodes.Mesh();

            mesh.LocalTransform = Matrix4x4.Identity;

            currentNode.AddChild(mesh);

            mesh.LocalTransform = Matrix4x4.Identity;


            var geometry = new Geometry();

            var assimpMesh = scene.Meshes[node.MeshIndices[i]];

            List<float> positions = new List<float>();
            List<float> normals = new List<float>();
            List<float> uvs = new List<float>();
            List<float> bones = new List<float>();
            List<float> boneWeight = new List<float>();

            for(int j = 0; j < assimpMesh.VertexCount; j ++)
            {
                var vertex = assimpMesh.Vertices[j];
                positions.Add(vertex.X);
                positions.Add(vertex.Y);
                positions.Add(vertex.Z);

                if (assimpMesh.HasNormals)
                {
                    normals.Add(assimpMesh.Normals[j].X);
                    normals.Add(assimpMesh.Normals[j].Y);
                    normals.Add(assimpMesh.Normals[j].Z);
                }

                if (assimpMesh.HasTextureCoords(0))
                {

                    uvs.Add(assimpMesh.TextureCoordinateChannels[0][j].X);
                    uvs.Add(assimpMesh.TextureCoordinateChannels[0][j].Y);
                }


            }

            geometry.SetVertexAttribute(BuildInVertexAttribute.Position, 3, positions);

            if (assimpMesh.HasTextureCoords(0))
            {
                geometry.SetVertexAttribute(BuildInVertexAttribute.TexCoord_0, 2, uvs);
            }


            List<uint> indices = [];

           ;
            foreach(var index in assimpMesh.GetIndices())
            {
                indices.Add((uint)index);
            }
            geometry.SetIndices(indices);

            geometry.SetVertexAttribute(BuildInVertexAttribute.Normal, 3, normals);

            if (assimpMesh.HasNormals && assimpMesh.HasTextureCoords(0))
            {
                ModelHelper.CalcVerticsTbn(geometry.Indices, normals, uvs, out var tangents, out var bitangents);
                geometry.SetVertexAttribute(BuildInVertexAttribute.Tangent, 3, tangents);
                geometry.SetVertexAttribute(BuildInVertexAttribute.Bitangent, 3, bitangents);
            }


            mesh.Geometry = geometry;

            mesh.Material = materialMap[assimpMesh.MaterialIndex];
        }

        for(int i = 0; i < node.ChildCount; i ++)
        {
            processNode(scene, node.Children[i],currentNode, materialMap);
        }
    }

    public static Skeleton processSkeleton(Scene scene)
    {
        Dictionary<string, Core.Resources.Bone> boneMap = [];
        var skeleton = new Skeleton();
        processNode(scene, scene.RootNode, boneMap);

        foreach (var (name, bone) in boneMap)
        {
            var node = scene.RootNode.FindNode(name);

            if (node.Parent != null && boneMap.TryGetValue(node.Parent.Name, out var parentBone))
            {

                bone.Parent = parentBone;
                parentBone.Children.Add(bone);

            }
        }
        foreach (var (name, bone) in boneMap)
        {
            if (bone.Parent == null)
            {
                skeleton.Root = bone;
                bone.LocalMatrix = bone.WorldMatrix;
            }
            else
            {
                bone.LocalMatrix = bone.WorldMatrix * bone.Parent.WorldMatrix.Inverse();
            }
            skeleton.Bones.Add(bone);

        }

        return skeleton;
    }

    private static void processNode(Scene scene, Assimp.Node node, Dictionary<string, Core.Resources.Bone> boneMap)
    {
        foreach (var meshIndex in node.MeshIndices)
        {
            var mesh = scene.Meshes[meshIndex];
            if (mesh.HasBones)
            {
                foreach(var assiBone in mesh.Bones)
                {
                    if (boneMap.ContainsKey(assiBone.Name))
                        continue;
                    var bone = new Core.Resources.Bone();
                    bone.Name = assiBone.Name;
                    bone.InverseWorldMatrix = assiBone.OffsetMatrix;
                    bone.WorldMatrix = assiBone.OffsetMatrix.Inverse();
                    bone.Index = boneMap.Count;
                    boneMap.Add(assiBone.Name, bone);
                }
            }    
        }
        foreach(var child in node.Children)
        {
            processNode(scene, child, boneMap);
        }
    }



}
