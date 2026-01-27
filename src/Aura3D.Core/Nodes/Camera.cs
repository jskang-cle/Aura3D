using System.Drawing;
using System.Numerics;
using Aura3D.Core.Renderers;
using Aura3D.Core.Math;
using Aura3D.Core.Resources;

namespace Aura3D.Core.Nodes;

public class Camera : Node
{
    public static ControlRenderTarget? ControlRenderTarget;

    public Camera()
    {
        if (ControlRenderTarget == null)
            throw new Exception("ControlRenderTarget is null, please set Camera.ControlRenderTarget before create Camera instance.");
        RenderTarget = ControlRenderTarget;
    }

    public float NearPlane { get; set; } = 1f; // 近裁剪面

    public float FarPlane { get; set; } = 100f; // 远裁剪面

    public float FieldOfView { get; set; } = 75f; // 视野角度（度数）

    public float OrthographicSize { get; set; } = 5f; // 正交投影时的大小

    public Matrix4x4 View
    {
        get
        {
            var worldTransform = WorldTransform;

            return Matrix4x4.CreateLookAt(worldTransform.Translation, worldTransform.Translation + worldTransform.ForwardVector(), worldTransform.UpVector());

        }
    }

    public Matrix4x4 Projection
    {
        get
        {
            if (ProjectionType == ProjectionType.Perspective)
            {
                var fovRadians = FieldOfView.DegreeToRadians();

                var aspectRatio = RenderTarget.Width / (float)RenderTarget.Height;

                var projection =  Matrix4x4.CreatePerspectiveFieldOfView(fovRadians, aspectRatio, NearPlane, FarPlane);

                return projection;
            }
            else // Orthographic
            {
                float aspectRatio = RenderTarget.Width / (float)RenderTarget.Height;
                return Matrix4x4.CreateOrthographic(
                    OrthographicSize * aspectRatio, // 宽度
                    OrthographicSize, // 高度
                    NearPlane,
                    FarPlane);
            }
        }
    }

    public Matrix4x4 ViewProjection => View * Projection;

    public Color ClearColor { get; set; } = Color.FromArgb(0, 0, 0, 0); // 清除颜色

    public ProjectionType ProjectionType { get; set; } = ProjectionType.Perspective; // 投影类型

    public IRenderTarget RenderTarget { get; set; } = new ControlRenderTarget();

    public ClearType ClearType { get; set; } = ClearType.Color; // 清除类型


    private CubeTexture? skyboxTexture = null;

    public  CubeTexture? SkyboxTexture
    {
        get => skyboxTexture;
        set
        {
            if (value != null && CurrentScene != null)
            {
                CurrentScene.RenderPipeline.AddGpuResource(value);
            }
            skyboxTexture = value;
        }
    }

    private Texture? backgroundTexture = null;
    public Texture? BackgroundTexture 
    { 
        get => backgroundTexture;
        set
        {
            if (value != null && CurrentScene != null)
            {
                CurrentScene.RenderPipeline.AddGpuResource(value);
            }
            backgroundTexture = value;
        }
    }

    public override List<IGpuResource> GetGpuResources()
    {
        var list = new List<IGpuResource>();

        if (SkyboxTexture != null)
        {
            list.Add(SkyboxTexture);
        }
        if (BackgroundTexture != null)
        {
            list.Add(BackgroundTexture);
        }
        return list;
    }

    public void LookAt(Vector3 target)
    {
        var camera = this;

        Vector3 cameraPos = camera.Position;

        Vector3 forward = Vector3.Normalize(target - cameraPos);

        Vector3 up = Vector3.UnitY; // 假设世界上方向为Y轴

        // 计算右向量
        Vector3 right = Vector3.Cross(forward, up);
        // 重新计算正交上向量
        up = Vector3.Cross(right, forward);

        // 构建旋转矩阵
        Matrix4x4 rotation = Matrix4x4.Identity;
        rotation.M11 = right.X;
        rotation.M21 = right.Y;
        rotation.M31 = right.Z;
        rotation.M12 = up.X;
        rotation.M22 = up.Y;
        rotation.M32 = up.Z;
        rotation.M13 = -forward.X;
        rotation.M23 = -forward.Y;
        rotation.M33 = -forward.Z;

        // 从旋转矩阵提取欧拉角（弧度）
        float pitch = MathF.Asin(-rotation.M23);
        float yaw = MathF.Atan2(rotation.M13, rotation.M33);
        float roll = MathF.Atan2(rotation.M21, rotation.M22);

        // 转换为角度并设置
        camera.RotationDegrees = new Vector3(
            pitch.RadiansToDegree(),
            yaw.RadiansToDegree(),
            roll.RadiansToDegree()
        );
    }

    public void FitToBoundingBox(BoundingBox aabb, float padding = 0.1f, float preferredDistance = 0)
    {
        var camera = this;
        if (camera == null) throw new ArgumentNullException(nameof(camera));
        if (aabb == null) throw new ArgumentNullException(nameof(aabb));
        if (padding < 0 || padding > 1) throw new ArgumentOutOfRangeException(nameof(padding));

        // 1. 计算包围盒中心和尺寸
        Vector3 boxCenter = aabb.Center;
        Vector3 boxSize = aabb.Size;

        // 2. 计算所需的观察距离
        float distance;
        if (preferredDistance > 0)
        {
            distance = preferredDistance;
        }
        else
        {
            // 根据视野和包围盒尺寸计算最小观察距离
            float fovRadians = camera.FieldOfView.DegreeToRadians();
            float aspectRatio = camera.RenderTarget.Width / (float)camera.RenderTarget.Height;

            // 计算包围盒对角线一半（考虑最大维度）
            float maxExtent = MathF.Max(boxSize.X, MathF.Max(boxSize.Y, boxSize.Z)) / 2f;

            // 考虑边距和宽高比的距离计算
            distance = maxExtent / MathF.Sin(fovRadians / 2f) * (1 + padding);
            distance = MathF.Max(distance, maxExtent / (MathF.Sin(fovRadians / 2f) * aspectRatio) * (1 + padding));
        }

        // 3. 设置摄像机位置（从包围盒中心沿摄像机当前前方向后移动指定距离）
        Vector3 cameraDirection = camera.Forward;
        camera.Position = boxCenter - cameraDirection * distance;

        // 4. 调整近/远裁剪面（确保完整包含物体）
        float boxDiagonal = boxSize.Length();
        camera.NearPlane = distance - boxDiagonal * 0.6f; // 近裁剪面稍微靠近物体
        camera.FarPlane = distance + boxDiagonal * 1.2f; // 远裁剪面留出足够空间

        // 5. 确保摄像机始终看向包围盒中心
        camera.LookAt(boxCenter);
    }
}

public enum ProjectionType
{
    Perspective, // 透视投影
    Orthographic // 正交投影
}

public enum ClearType
{
    OnlyDepth, // 仅清除颜色缓冲区
    Color,
    Skybox,
    Texture
}