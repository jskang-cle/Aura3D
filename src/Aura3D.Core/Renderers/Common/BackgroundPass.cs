using Aura3D.Core.Math;
using Aura3D.Core.Nodes;
using Silk.NET.OpenGLES;
using System.Drawing;
using System.Numerics;

namespace Aura3D.Core.Renderers;

public class BackgroundPass: RenderPass
{
    public BackgroundPass(RenderPipeline renderPipeline) : base(renderPipeline)
    {
        VertexShader = ShaderResource.BackgroundVert;
        FragmentShader = ShaderResource.BackgroundFrag;
        ShaderName = nameof(BackgroundPass);
    }


    public override void BeforeRender(Camera camera)
    {

        BindOutPutRenderTarget(camera);

        gl.ClearColor(camera.ClearColor);

        gl.DepthMask(true);

        if (camera.ClearType != ClearType.Color)
            gl.ClearColor(Color.Black);

        if (camera.ClearType != ClearType.OnlyDepth)
            gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        else
            gl.Clear(ClearBufferMask.DepthBufferBit);

        gl.DepthMask(false);
        gl.Disable(EnableCap.DepthTest);
        gl.Disable(EnableCap.CullFace);

    }
    public override void Render(Camera camera)
    {
        ClearTextureUnit(); 
        UseShader_Internal(null);
        if (camera.ClearType == ClearType.Skybox && camera.SkyboxTexture != null)
        {
            Matrix4x4 projection = default;
            if (camera.ProjectionType == ProjectionType.Orthographic)
            {
                UseShader("SKYBOX", "ORTHOGRAPHIC");
                UniformMatrix4("viewRot", camera.View);
                UniformFloat("farPlane", camera.FarPlane);
                float aspectRatio = camera.RenderTarget.Width / (float)camera.RenderTarget.Height;
                UniformVector2("orthoSize", new Vector2(100 * aspectRatio, 100));
                projection = camera.Projection;

            }
            else
            {
                UseShader("SKYBOX"); 

                var fovRadians = camera.FieldOfView.DegreeToRadians();

                var aspectRatio = camera.RenderTarget.Width / (float)camera.RenderTarget.Height;

                projection = Matrix4x4.CreatePerspectiveFieldOfView(fovRadians, aspectRatio, 10, 100);

            }
            UniformMatrix4("invViewProj", (camera.View * projection).Inverse());
            UniformTextureCubeMap("uSkybox", camera.SkyboxTexture);
            RenderQuad();
        }
        else if (camera.ClearType == ClearType.Texture && camera.BackgroundTexture != null)
        {
            UseShader("BACKGROUND_TEXTURE");
            UniformTexture("uBackgroundTexture", camera.BackgroundTexture);
            RenderQuad();
        }
    }

    public override void AfterRender(Camera camera)
    {
        gl.DepthMask(true);
        gl.Enable(EnableCap.DepthTest);
    }

}
