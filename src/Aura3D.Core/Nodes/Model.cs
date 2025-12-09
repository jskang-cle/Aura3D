using Aura3D.Core.Math;
using Silk.NET.OpenGLES;
using System.Numerics;
using System.Reflection.Metadata.Ecma335;

namespace Aura3D.Core.Nodes;

public class Model : Node
{
    public IReadOnlyList<Mesh> Meshes => GetNodesInChildren<Mesh>();

    public virtual Model Clone(CopyType copyType = CopyType.SharedResource)
    {
        var model = (Model)clone(this, null);

        foreach(var mesh in model.Meshes)
        {
            if (copyType == CopyType.SharedResourceData)
            {
                mesh.Geometry = mesh.Geometry?.Clone();
                mesh.Material = mesh.Material?.DeepClone();
            }
            else if (copyType == CopyType.FullCopy)
            {
                mesh.Geometry = mesh.Geometry?.DeepClone();
                mesh.Material = mesh.Material?.DeepClone();
            }
        }

        return model;
    }

    protected Node clone(Node node, Node? parentNode)
    {
        Node? cloneNode = null;
        if (node is SkinnedModel skinnedModel)
        {
            cloneNode = new SkinnedModel();
            ((SkinnedModel)cloneNode).Skeleton = skinnedModel.Skeleton;
            ((SkinnedModel)cloneNode).AnimationSampler = skinnedModel.AnimationSampler;

        }
        else if (node is Model model)
        {
            cloneNode = new Model();
        }
        else if (node is SkinnedMesh skinnedMesh)
        {
            cloneNode = new SkinnedMesh();
            ((SkinnedMesh)cloneNode).Skeleton = skinnedMesh.Skeleton;
            ((SkinnedMesh)cloneNode).SkinnedModel = skinnedMesh.SkinnedModel;
            ((SkinnedMesh)cloneNode).Geometry = skinnedMesh.Geometry;
            ((SkinnedMesh)cloneNode).Material = skinnedMesh.Material;
        }
        else if (node is Mesh mesh)
        {
            cloneNode = new Mesh();
            ((Mesh)cloneNode).Geometry = mesh.Geometry;
            ((Mesh)cloneNode).Material = mesh.Material;
        }
        else
        {
            cloneNode = new Node();
        }
        
        if (parentNode != null)
        {
            parentNode.AddChild(cloneNode);
        }

        cloneNode.LocalTransform = node.LocalTransform;
        cloneNode.Enable = node.Enable;
        cloneNode.Name = node.Name;

        foreach (var child in node.Children)
        {
            clone(child, cloneNode);
        }
        return cloneNode;

    }

    public BoundingBox BoundingBox
    {
        get 
        {
            List<BoundingBox> boundingBoxes = [];
            if (Meshes.Count > 0)
            {
                foreach (var mesh in Meshes)
                {
                    if (mesh == null)
                        continue;
                    if (mesh.BoundingBox == null)
                        continue;
                    boundingBoxes.Add(mesh.BoundingBox);
                }
            }
            return BoundingBox.CreateMerged(boundingBoxes);
        }
    }
}


public static class ModelHelper
{

    public static void CalcVerticsTbn(List<uint> indices, List<float> vertexNormals, List<float> uvs, out List<float> tangents, out List<float> bitangents)
    {
        tangents = [.. new float[vertexNormals.Count]];

        bitangents = [.. new float[vertexNormals.Count]];

        for (int i = 0; i < indices.Count - 2; i += 3)
        {
            var p1Index = (int)indices[i] * 3;
            var p2Index = (int)indices[i + 1] * 3;
            var p3Index = (int)indices[i + 2] * 3;

            var p1 = new Vector3(vertexNormals[p1Index], vertexNormals[p1Index + 1], vertexNormals[p1Index + 2]);
            var p2 = new Vector3(vertexNormals[p2Index], vertexNormals[p2Index + 1], vertexNormals[p2Index + 2]);
            var p3 = new Vector3(vertexNormals[p3Index], vertexNormals[p3Index + 1], vertexNormals[p3Index + 2]);

            var uv1 = new Vector2(uvs[p1Index / 3 * 2], uvs[p1Index / 3 * 2 + 1]);
            var uv2 = new Vector2(uvs[p2Index / 3 * 2], uvs[p2Index / 3 * 2 + 1]);
            var uv3 = new Vector2(uvs[p3Index / 3 * 2], uvs[p3Index / 3 * 2 + 1]);


            Vector3 edge1 = p2 - p1;
            Vector3 edge2 = p3 - p1;

            Vector2 deltaUv1 = uv2 - uv1;
            Vector2 deltaUv2 = uv3 - uv1;

            float f = 1.0f / (deltaUv1.X * deltaUv2.Y - deltaUv2.X * deltaUv1.Y);

            Vector3 tangent1;
            Vector3 bitangent1;

            tangent1.X = f * (deltaUv2.Y * edge1.X - deltaUv1.Y * edge2.X);
            tangent1.Y = f * (deltaUv2.Y * edge1.Y - deltaUv1.Y * edge2.Y);
            tangent1.Z = f * (deltaUv2.Y * edge1.Z - deltaUv1.Y * edge2.Z);
            tangent1 = Vector3.Normalize(tangent1);

            bitangent1.X = f * (-deltaUv2.X * edge1.X + deltaUv1.X * edge2.X);
            bitangent1.Y = f * (-deltaUv2.X * edge1.Y + deltaUv1.X * edge2.Y);
            bitangent1.Z = f * (-deltaUv2.X * edge1.Z + deltaUv1.X * edge2.Z);
            bitangent1 = Vector3.Normalize(bitangent1);

            tangents[(int)indices[i] * 3] = tangent1.X;
            tangents[(int)indices[i] * 3 + 1] = tangent1.Y;
            tangents[(int)indices[i] * 3 + 2] = tangent1.Z;

            tangents[(int)indices[i + 1] * 3] = tangent1.X;
            tangents[(int)indices[i + 1] * 3 + 1] = tangent1.Y;
            tangents[(int)indices[i + 1] * 3 + 2] = tangent1.Z;

            tangents[(int)indices[i + 2] * 3] = tangent1.X;
            tangents[(int)indices[i + 2] * 3 + 1] = tangent1.Y;
            tangents[(int)indices[i + 2] * 3 + 2] = tangent1.Z;

            bitangents[(int)indices[i] * 3] = bitangent1.X;
            bitangents[(int)indices[i] * 3 + 1] = bitangent1.Y;
            bitangents[(int)indices[i] * 3 + 2] = bitangent1.Z;
            bitangents[(int)indices[i + 1] * 3] = bitangent1.X;

            bitangents[(int)indices[i + 1] * 3 + 1] = bitangent1.Y;
            bitangents[(int)indices[i + 1] * 3 + 2] = bitangent1.Z;

            bitangents[(int)indices[i + 2] * 3] = bitangent1.X;
            bitangents[(int)indices[i + 2] * 3 + 1] = bitangent1.Y;
            bitangents[(int)indices[i + 2] * 3 + 2] = bitangent1.Z;
        }
    }
}
