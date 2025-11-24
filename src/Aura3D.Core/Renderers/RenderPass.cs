using Aura3D.Core.Math;
using Aura3D.Core.Nodes;
using Aura3D.Core.Resources;
using Aura3D.Core.Scenes;
using SharpGLTF.Transforms;
using Silk.NET.OpenGLES;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;

namespace Aura3D.Core.Renderers;

public partial class RenderPass
{
    public RenderPass(RenderPipeline renderPipeline)
    {
        this.renderPipeline = renderPipeline;
    }

    protected RenderPipeline renderPipeline;

    protected Scene Scene => renderPipeline.Scene;

    protected List<Mesh> Meshes => renderPipeline.Meshes;

    protected List<PointLight> PointLights => renderPipeline.PointLights;

    protected List<SpotLight> SpotLights => renderPipeline.SpotLights;

    protected GL gl => renderPipeline.gl!;
    public virtual void Setup()
    {

    }

    public bool EnableFrustumCulling => renderPipeline.EnableFrustumCulling;

    public virtual void BeforeRender(Camera camera)
    {

    }
    public virtual void Render(Camera camera)
    {

    }

    public virtual void AfterRender(Camera camera)
    {

    }


    public virtual void BeforeRender()
    {

    }
    public virtual void Render()
    {

    }

    public virtual void AfterRender()
    {

    }

    protected string? outputRenderTargetName;
    public RenderPass SetOutPutRenderTarget(string? renderTargetName)
    {
        this.outputRenderTargetName = renderTargetName;

        return this;
    }

    public void BindOutPutRenderTarget(Camera camera)
    {
        uint fbo = 0;

        if (outputRenderTargetName != null)
        {
            var rt = GetRenderTarget(outputRenderTargetName,
                new System.Drawing.Size((int)camera.RenderTarget.Width, (int)camera.RenderTarget.Height));
            fbo = rt.FrameBufferId;
        }
        else
        {
            fbo = camera.RenderTarget.FrameBufferId;
        }
        gl.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
        gl.Viewport(0, 0, camera.RenderTarget.Width, camera.RenderTarget.Height);
    }

    public RenderTarget GetRenderTarget(string name, Size size) => renderPipeline.GetRenderTarget(name, size);

    public unsafe virtual void RenderMesh(Mesh mesh, Matrix4x4 view, Matrix4x4 projection)
    {
        UniformMatrix4("modelMatrix", mesh.WorldTransform);
        gl.BindVertexArray(mesh.Geometry!.Vao);
        gl.DrawElements(GLEnum.Triangles, (uint)mesh.Geometry.IndicesCount, GLEnum.UnsignedInt, (void*)0);
    }

    public void RenderMeshes(Func<Mesh, bool> filter, Matrix4x4 view, Matrix4x4 projection)
    {
        foreach (var mesh in renderPipeline.Meshes)
        {
            if (mesh.Enable == false)
                continue;
            if (mesh.Geometry == null)
                continue;
            if (filter(mesh))
            {
                RenderMesh(mesh, view, projection);
            }
        }
    }
    List<Mesh> meshes = new List<Mesh>();
    Plane[] planes = new Plane[6];
    public void RenderStaticMeshes(Func<Mesh, bool> filter, Matrix4x4 view, Matrix4x4 projection)
    {
        var list = renderPipeline.Meshes;

        if (EnableFrustumCulling == true)
        {
            var viewProjection = view * projection;

            Matrix4x4.Invert(viewProjection, out Matrix4x4 invViewProj);

            Span<Vector3> ndcCorners = stackalloc Vector3[]
            {
                new Vector3(-1,-1,-1), new Vector3(1,-1,-1),
                new Vector3(-1, 1,-1), new Vector3(1, 1,-1),
                new Vector3(-1,-1, 1), new Vector3(1,-1, 1),
                new Vector3(-1, 1, 1), new Vector3(1, 1, 1)
            };

            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (var c in ndcCorners)
            {
                Vector4 p = new Vector4(c, 1.0f);
                Vector4 world = Vector4.Transform(p, invViewProj);
                world /= world.W;

                Vector3 wpos = new Vector3(world.X, world.Y, world.Z);
                min = Vector3.Min(min, wpos);
                max = Vector3.Max(max, wpos);
            }

            var cameraBoudingBox = new BoundingBox (min, max);


            MatrixHelper.ExtractPlanes(viewProjection, planes);

            meshes.Clear();

            this.Scene.StaticMeshOctree.Query(boundingBox =>
            {
                if (cameraBoudingBox.Intersects(boundingBox))
                {
                    if (boundingBox.IsBoxInsideFrustum(planes))
                    {
                        return true;
                    }
                }
                return false;

            }, meshes);

            list = meshes;

        }
        foreach (var mesh in list)
        {
            if (mesh.Enable == false)
                continue;
            if (mesh.Geometry == null)
                continue;
            if (IsSkeletonMesh(mesh) == true)
                continue;
            if (filter(mesh))
            {
                RenderMesh(mesh, view, projection);
            }
        }
    }

    public void RenderSkinnedMeshes(Func<Mesh, bool> filter, Matrix4x4 view, Matrix4x4 projection)
    {
        foreach (var mesh in renderPipeline.Meshes)
        {
            if (mesh.Enable == false)
                continue;
            if (mesh.Geometry == null)
                continue;
            if (IsSkeletonMesh(mesh) == false)
                continue;
            if (filter(mesh))
            {
                RenderMesh(mesh, view, projection);
            }
        }
    }

    protected bool IsMaterialBlendMode(Mesh mesh, BlendMode mode)
    {
        if (mesh.Material == null)
            if (mode == BlendMode.Opaque)
                return true;
            else
                return false;
        else
        {
            if (mesh.Material.BlendMode == mode)
                return true;
            return false;

        }
    }
    protected bool IsSkeletonMesh(Mesh mesh)
    {
        if (mesh.IsSkinnedMesh == true && mesh is SkinnedMesh skinnedMesh && skinnedMesh.Skeleton != null)
            return true;
        return false;
    }

    public virtual void SortMeshes(List<Mesh> Meshes, Camera camera)
    {
        renderPipeline.SortMeshes(Meshes, camera);
    }    

    public void RenderCube()
    {
        renderPipeline.RenderCube();
    }

    public void RenderQuad()
    {
        renderPipeline.RenderQuad();
    }

    public void Destroy()
    { 
        foreach(var shader in Shaders)
        {
            gl.DeleteProgram(shader.Value.ProgramId);
        }
        Shaders.Clear();
    }

}