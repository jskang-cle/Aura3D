using Assimp;
using Aura3D.Core;
using Aura3D.Core.Nodes;
using Aura3D.Core.Resources;
using Silk.NET.OpenGLES;
using StbImageSharp;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace Aura3D.Model;

public static class AssimpLoader
{
    public unsafe static Aura3D.Core.Nodes.Model Load(string path)
    {
        var model = new Aura3D.Core.Nodes.Model();

        Dictionary<int, Core.Resources.Material> materialsMap = new();

        var defaultFlags = PostProcessSteps.Triangulate
                  | PostProcessSteps.GenerateNormals
                  | PostProcessSteps.OptimizeMeshes
                  | PostProcessSteps.CalculateTangentSpace;

        var importer = new AssimpContext();

        var directory = Path.GetDirectoryName(path);


        var scene = importer.ImportFile(path, defaultFlags);

        processMaterial(scene, materialsMap, directory);

        processNode(scene, scene.RootNode, model, materialsMap);

        return model;
    }


    public static unsafe void processMaterial(Scene scene, Dictionary<int, Core.Resources.Material> materialsMap, string path)
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
                texture = processTexture(scene, slot, path);

               
            }
            else if (assimpMaterial.PBR.HasTextureBaseColor && assimpMaterial.PBR.TextureBaseColor.FilePath != null)
            {
                var slot = assimpMaterial.PBR.TextureBaseColor;
                texture = processTexture(scene, slot, path);

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
                texture = processTexture(scene, slot, path);

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


    private static unsafe Core.Resources.Texture processTexture(Scene scene, TextureSlot textureSlot, string path)
    {
        if (textureSlot.FilePath[0] == '*')
        {
            var assimpTexture = scene.Textures[int.Parse(textureSlot.FilePath.Skip(1).Take(textureSlot.FilePath.Length - 1).ToArray())];

            if (assimpTexture.HasCompressedData)
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
                for(int i = 0; i < assimpTexture.NonCompressedDataSize; i++)
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
            var filePath = Path.Combine(path, textureSlot.FilePath);

            StbImage.stbi_set_flip_vertically_on_load_thread(1);
            try
            {
                using (var sr = new StreamReader(filePath))
                {
                    return TextureLoader.LoadTexture(sr.BaseStream);
                }
            }
            finally
            {

                StbImage.stbi_set_flip_vertically_on_load_thread(0);
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

            geometry.SetVertexAttribute(BuildInVertexAttribute.Position, positions);

            if (assimpMesh.HasTextureCoords(0))
            {
                geometry.SetVertexAttribute(BuildInVertexAttribute.TexCoord, uvs);
            }


            List<uint> indices = [];

           ;
            foreach(var index in assimpMesh.GetIndices())
            {
                indices.Add((uint)index);
            }
            geometry.SetIndices(indices);


            if (assimpMesh.HasNormals)
            {
                geometry.SetVertexAttribute(BuildInVertexAttribute.Normal, normals);
                ModelHelper.CalcVerticsTbn(geometry.Indices, normals, uvs, out var tangents, out var bitangents);
                geometry.SetVertexAttribute(BuildInVertexAttribute.Tangent, tangents);
                geometry.SetVertexAttribute(BuildInVertexAttribute.Bitangent, bitangents);
            }


            mesh.Geometry = geometry;

            mesh.Material = materialMap[assimpMesh.MaterialIndex];
        }

        for(int i = 0; i < node.ChildCount; i ++)
        {
            processNode(scene, node.Children[i],currentNode, materialMap);
        }
    }

}
