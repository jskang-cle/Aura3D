using System.Numerics;

namespace Aura3D.Core.Math
{
    /// <summary>
    /// 轴对齐边界框（Axis-Aligned Bounding Box）
    /// 用于碰撞检测和视锥体剔除
    /// </summary>
    public struct BoundingBox(object obj)
    {
        /// <summary>
        /// 关联的对象
        /// </summary>
        public object Object = obj;

        /// <summary>
        /// 边界框的最小点
        /// </summary>
        public Vector3 Min;
        
        /// <summary>
        /// 边界框的最大点
        /// </summary>
        public Vector3 Max;

        /// <summary>
        /// 边界框的中心点
        /// </summary>
        public Vector3 Center => (Min + Max) * 0.5f;

        /// <summary>
        /// 边界框的尺寸
        /// </summary>
        public Vector3 Size => Max - Min;

        /// <summary>
        /// 边界框的半径（从中心到最远点的距离）
        /// </summary>
        public float Radius => (Max - Min).Length() * 0.5f;

        /// <summary>
        /// 构造一个新的边界框
        /// </summary>
        /// <param name="min">最小点</param>
        /// <param name="max">最大点</param>
        public BoundingBox(Vector3 min, Vector3 max, object obj) : this(obj)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// 从点列表创建边界框
        /// </summary>
        /// <param name="points">点列表</param>
        /// <returns>包含所有点的边界框</returns>
        public static BoundingBox CreateFromPoints(IEnumerable<Vector3> points, object obj)
        {
            if (points == null || !points.Any())
                return new BoundingBox(Vector3.Zero, Vector3.Zero, obj);

            var min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var point in points)
            {
                min = Vector3.Min(min, point);
                max = Vector3.Max(max, point);
            }

            return new BoundingBox(min, max, obj);
        }

        /// <summary>
        /// 扩展边界框以包含指定点
        /// </summary>
        /// <param name="point">要包含的点</param>
        public void Include(Vector3 point)
        {
            Min = Vector3.Min(Min, point);
            Max = Vector3.Max(Max, point);
        }

        /// <summary>
        /// 扩展边界框以包含另一个边界框
        /// </summary>
        /// <param name="other">要包含的边界框</param>
        public void Include(BoundingBox other)
        {
            Min = Vector3.Min(Min, other.Min);
            Max = Vector3.Max(Max, other.Max);
        }

        /// <summary>
        /// 检查点是否在边界框内部或边界上
        /// </summary>
        /// <param name="point">要检查的点</param>
        /// <returns>如果点在边界框内部或边界上则返回true</returns>
        public bool Contains(Vector3 point)
        {
            return point.X >= Min.X && point.X <= Max.X &&
                   point.Y >= Min.Y && point.Y <= Max.Y &&
                   point.Z >= Min.Z && point.Z <= Max.Z;
        }

        /// <summary>
        /// 检查两个边界框是否相交
        /// </summary>
        /// <param name="other">另一个边界框</param>
        /// <returns>如果边界框相交则返回true</returns>
        public bool Intersects(BoundingBox other)
        {
            return Min.X <= other.Max.X && Max.X >= other.Min.X &&
                   Min.Y <= other.Max.Y && Max.Y >= other.Min.Y &&
                   Min.Z <= other.Max.Z && Max.Z >= other.Min.Z;
        }

        /// <summary>
        /// 变换边界框
        /// </summary>
        /// <param name="matrix">变换矩阵</param>
        /// <returns>变换后的边界框</returns>
        public BoundingBox Transform(Matrix4x4 matrix)
        {
            // 计算变换后的8个顶点
            var vertices = new[]
            {
                Vector3.Transform(new Vector3(Min.X, Min.Y, Min.Z), matrix),
                Vector3.Transform(new Vector3(Max.X, Min.Y, Min.Z), matrix),
                Vector3.Transform(new Vector3(Min.X, Max.Y, Min.Z), matrix),
                Vector3.Transform(new Vector3(Max.X, Max.Y, Min.Z), matrix),
                Vector3.Transform(new Vector3(Min.X, Min.Y, Max.Z), matrix),
                Vector3.Transform(new Vector3(Max.X, Min.Y, Max.Z), matrix),
                Vector3.Transform(new Vector3(Min.X, Max.Y, Max.Z), matrix),
                Vector3.Transform(new Vector3(Max.X, Max.Y, Max.Z), matrix)
            };

            // 从变换后的顶点创建新的边界框
            return CreateFromPoints(vertices, this);
        }

        /// <summary>
        /// 视锥体剔除测试
        /// </summary>
        /// <param name="planes">视锥体的6个平面</param>
        /// <returns>如果边界框完全在视锥体外则返回true，表示应该被剔除</returns>
        public bool ShouldBeCulled(Plane[] planes)
        {
            // 检查边界框是否完全在任何一个视锥体平面的外部
            foreach (var plane in planes)
            {
                // 找到边界框上离平面最远的点
                var point = Min;
                if (plane.Normal.X > 0) point.X = Max.X;
                if (plane.Normal.Y > 0) point.Y = Max.Y;
                if (plane.Normal.Z > 0) point.Z = Max.Z;

                // 如果最远点在平面后面，则整个边界框都在平面后面
                if (Plane.DotCoordinate(plane, point) < 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// 计算边界框与射线的相交
        /// </summary>
        /// <param name="rayOrigin">射线原点</param>
        /// <param name="rayDirection">射线方向（单位向量）</param>
        /// <param name="tMin">射线参数最小值</param>
        /// <param name="tMax">射线参数最大值</param>
        /// <returns>如果射线与边界框相交则返回true</returns>
        public bool RayIntersect(Vector3 rayOrigin, Vector3 rayDirection, out float tMin, out float tMax)
        {
            tMin = 0.0f;
            tMax = float.MaxValue;

            // X轴方向
            if (MathF.Abs(rayDirection.X) < 1e-6f)
            {
                // 射线平行于X平面
                if (rayOrigin.X < Min.X || rayOrigin.X > Max.X)
                    return false;
            }
            else
            {
                float t1 = (Min.X - rayOrigin.X) / rayDirection.X;
                float t2 = (Max.X - rayOrigin.X) / rayDirection.X;

                tMin = MathF.Max(tMin, MathF.Min(t1, t2));
                tMax = MathF.Min(tMax, MathF.Max(t1, t2));
            }

            // Y轴方向
            if (MathF.Abs(rayDirection.Y) < 1e-6f)
            {
                // 射线平行于Y平面
                if (rayOrigin.Y < Min.Y || rayOrigin.Y > Max.Y)
                    return false;
            }
            else
            {
                float t1 = (Min.Y - rayOrigin.Y) / rayDirection.Y;
                float t2 = (Max.Y - rayOrigin.Y) / rayDirection.Y;

                tMin = MathF.Max(tMin, MathF.Min(t1, t2));
                tMax = MathF.Min(tMax, MathF.Max(t1, t2));
            }

            // Z轴方向
            if (MathF.Abs(rayDirection.Z) < 1e-6f)
            {
                // 射线平行于Z平面
                if (rayOrigin.Z < Min.Z || rayOrigin.Z > Max.Z)
                    return false;
            }
            else
            {
                float t1 = (Min.Z - rayOrigin.Z) / rayDirection.Z;
                float t2 = (Max.Z - rayOrigin.Z) / rayDirection.Z;

                tMin = MathF.Max(tMin, MathF.Min(t1, t2));
                tMax = MathF.Min(tMax, MathF.Max(t1, t2));
            }

            return tMin <= tMax && tMax >= 0;
        }
    }
}