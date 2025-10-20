using Aura3D.Core.Resources;
using Aura3D.Core.Math;
using System.Numerics;

namespace Aura3D.Core.Nodes;

public class Mesh : Node
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

    public virtual bool IsSkinnedMesh => false;
    
    /// <summary>
    /// 网格的边界框
    /// </summary>
    public BoundingBox? BoundingBox => boundingBox;

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
            UpdateBoundingBox();
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
    
    /// <summary>
    /// 局部空间中的边界框
    /// </summary>
    private BoundingBox? localBoundingBox;
    
    /// <summary>
    /// 更新边界框
    /// </summary>
    protected virtual void UpdateBoundingBox()
    {
        if (Geometry == null)
        {
            localBoundingBox = null;
            boundingBox = null;
            return;
        }
        
        // 获取顶点位置数据
        var positionData = Geometry.GetAttributeData(BuildInVertexAttribute.Position);
        if (positionData == null || positionData.Count < 3)
        {
            localBoundingBox = null;
            boundingBox = null;
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
        
        // 更新世界空间中的边界框
        UpdateWorldBoundingBox();
    }
    
    /// <summary>
    /// 更新世界空间中的边界框
    /// </summary>
    protected virtual void UpdateWorldBoundingBox()
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
        base.UpdateTransform();
        UpdateWorldBoundingBox();
    }
}
