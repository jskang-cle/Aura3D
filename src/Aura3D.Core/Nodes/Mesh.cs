using Aura3D.Core.Math;
using Aura3D.Core.Resources;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;

namespace Aura3D.Core.Nodes;

public class Mesh : Node, IOctreeObject
{
    private Material? material;
    public Material? Material 
    { 
        get => material;
        set
        {
            if (value != null && CurrentScene != null)
            {
                foreach (var channel in value.Channels)
                {
                    if (channel.Texture != null && channel.Texture is IGpuResource gpuResource)
                    {
                        CurrentScene.RenderPipeline.AddGpuResource(gpuResource);
                    }
                }
            }
            material = value;
        }
    }

    private Geometry? geometry;

    private BoundingBox? boundingBox;

    [MemberNotNullWhen(returnValue: true, nameof(Model), nameof(Skeleton))]
    public bool IsSkinnedMesh => Model != null && Model.Skeleton != null;
    
    /// <summary>
    /// 网格的边界框
    /// </summary>
    public BoundingBox? BoundingBox
    {
        get
        {
            UpdateTransform();

            return boundingBox;
        }
    }

    public Geometry? Geometry 
    { 
        get => geometry;
        set
        {
            if (value != null && CurrentScene != null)
            {
                CurrentScene.RenderPipeline.AddGpuResource(value);
            }
            geometry = value;
            InitBoundingBox();
        }
    }


    public override List<IGpuResource> GetGpuResources()
    {
        if (Geometry == null)
        {
            return [];
        }
        var list = new List<IGpuResource>()
        {
            Geometry
        };

        if (Material != null)
        {
            foreach(var channel in Material.Channels)
            {
                if (channel.Texture != null && channel.Texture is IGpuResource gpuResource)
                {
                    list.Add(gpuResource);
                }
            }
        }
        return list;
    }

    public Model? Model { get; set; }
    public List<object> BelongingNodes => belongingNodes;

    private List<object> belongingNodes = [];

    /// <summary>
    /// 局部空间中的边界框
    /// </summary>
    private BoundingBox? localBoundingBox;

    public event Action<IOctreeObject>? OnChanged = delegate { };

    /// <summary>
    /// 更新边界框
    /// </summary>
    public virtual void InitBoundingBox()
    {
        if (Geometry == null)
        {
            localBoundingBox = null;
            boundingBox = null;
            OnChanged?.Invoke(this);
            return;
        }
        
        if (IsSkinnedMesh == false)
        {

            // 获取顶点位置数据
            var positionData = Geometry.GetAttributeData(BuildInVertexAttribute.Position);
            if (positionData == null || positionData.Count < 3)
            {
                localBoundingBox = null;
                boundingBox = null;
                OnChanged?.Invoke(this);
                return;
            }

            // 将float列表转换为Vector3列表
            var positions = new List<Vector3>();
            for (int i = 0; i < positionData.Count; i += 3)
            {
                if (i + 2 < positionData.Count)
                {
                    positions.Add(new Vector3(
                        positionData[i],
                        positionData[i + 1],
                        positionData[i + 2]
                    ));
                }
            }

            // 从顶点位置创建局部空间边界框
            localBoundingBox = Math.BoundingBox.CreateFromPoints(positions);
        }
        else
        {
            CalcSkeletalMeshBoundingBox();
        }
        // 更新世界空间中的边界框
        UpdateWorldBoundingBox();
        OnChanged?.Invoke(this);
    }
    
    /// <summary>
    /// 更新世界空间中的边界框
    /// </summary>
    public virtual void UpdateWorldBoundingBox()
    {
        if (localBoundingBox == null)
        {
            boundingBox = null;
            return;
        }
        
        // 应用变换矩阵到边界框
        // 注意：对于有旋转的变换，轴对齐边界框需要重新计算
        boundingBox = localBoundingBox?.Transform(WorldTransform);
    }
    
    /// <summary>
    /// 重写UpdateTransform方法，确保在变换更新后也更新世界空间边界框
    /// </summary>
    public override void UpdateTransform()
    {
        if (_transformDirty == true)
        {
            base.UpdateTransform();
            UpdateWorldBoundingBox();
            OnChanged?.Invoke(this);
        }
    }

    public Skeleton? Skeleton => Model?.Skeleton;


    private Dictionary<int, BoundingBox> SkeletalMeshBoudingBox = new ();

    private List<BoundingBox> skeletalMeshBoudingBox2 = new ();

    public void CalcSkeletalMeshBoundingBox()
    {
        if (IsSkinnedMesh == false)
            return;
        SkeletalMeshBoudingBox.Clear();
        Dictionary<int, List<Vector3>> JointPoints = new Dictionary<int, List<Vector3>>();
        var mesh = this;

        if (mesh.Geometry == null)
            return;

        var positions = mesh.Geometry.GetAttributeData(BuildInVertexAttribute.Position);

        var joints = mesh.Geometry.GetAttributeData(BuildInVertexAttribute.Jonits_0);

        var weights = mesh.Geometry.GetAttributeData(BuildInVertexAttribute.Weights_0);

        if (positions != null && joints != null && weights != null)
        {
            for (var i = 0; i < positions.Count / 3; i++)
            {
                var position = new Vector3(positions[i * 3], positions[i * 3 + 1], positions[i * 3 + 2]);
                for (var j = 0; j < 4; j++)
                {
                    if (weights[i * 4 + j] > 0.3f)
                    {
                        var jointIndex = (int)joints[i * 4 + j];
                        if (JointPoints.TryGetValue(jointIndex, out _) == false)
                            JointPoints[jointIndex] = new();
                        JointPoints[jointIndex].Add(position);
                    }
                }

            }
        }

        foreach (var (index, points) in JointPoints)
        {
            var boundingBox = BoundingBox.CreateFromPoints(points);
            SkeletalMeshBoudingBox.Add(index, boundingBox);

            localBoundingBox = BoundingBox.CreateMerged(SkeletalMeshBoudingBox.Values);
        }

    }

    public void CalcSkeletalMeshBoundingBoxInPlayAnimation(IReadOnlyList<Matrix4x4> boneTransforms)
    {
        if (IsSkinnedMesh == false)
            return;
        if (SkeletalMeshBoudingBox.Count <= 0)
        {
            CalcSkeletalMeshBoundingBox();
        }
        skeletalMeshBoudingBox2.Clear();
        foreach (var (index, boundingBox) in SkeletalMeshBoudingBox)
        {
            if (index < boneTransforms.Count)
            {
                skeletalMeshBoudingBox2.Add(boundingBox.Transform(Skeleton.Bones[index].InverseWorldMatrix * boneTransforms[index]));
            }
        }

        localBoundingBox = BoundingBox.CreateMerged(skeletalMeshBoudingBox2);

        if (_transformDirty == true)
        {
            base.UpdateTransform();
        }
        UpdateWorldBoundingBox();

        OnChanged?.Invoke(this);
    }
}
