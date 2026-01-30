using Aura3D.Core.Renderers;
using System.Drawing;

namespace Aura3D.Core.Resources;

public class Material : IClone<Material>, IGpuResource
{
    public bool NeedsUpload { get; set; } = false;
    public List<Channel> Channels { get; set; } = [];
    
    public BlendMode BlendMode { get; set; } = BlendMode.Opaque;

    public bool DoubleSided { get; set; } = false;

    public float AlphaCutoff { get; set; } = 0.5f;

    public IReadOnlyDictionary<string, string> VertexShaders => _vertexShaders;

    private Dictionary<string, string> _vertexShaders = new Dictionary<string, string>();

    private Dictionary<string, string> _fragmentShaders = new Dictionary<string, string>();

    public IReadOnlyDictionary<string, string> FragmentShaders => _fragmentShaders;

    public Dictionary<string, Shader> Shaders { get; } = new Dictionary<string, Shader>();

    public Material Clone()
    {
        return new Material
        {
            BlendMode = this.BlendMode,
            DoubleSided = this.DoubleSided,
            AlphaCutoff = this.AlphaCutoff,
            Channels = Channels
        };
    }

    public Material DeepClone()
    {
        var material = Clone();

        material.Channels = new List<Channel>();

        foreach(var channel in Channels)
        {
            var newChannel = new Channel
            {
                Name = channel.Name,
                Color = channel.Color,
                Texture = channel.Texture is Texture texture? texture.Clone() : null
            };
            material.Channels.Add(newChannel);
        }

        return material;
    }

    public void SetShaderSource(string key, ShaderType shaderType, string shader)
    {
        if (shaderType == ShaderType.Fragment)
        {
            _fragmentShaders[key] = shader;
        }
        else if (shaderType == ShaderType.Vertex)
        {
            _vertexShaders[key] = shader;
        }
    }

    public (string? vertexShader, string? fragmentShader) GetShaderSource(string key)
    {
        string? vertexShader = null;

        string? fragmentShader = null;


        _vertexShaders.TryGetValue(key, out vertexShader);

        _fragmentShaders.TryGetValue(key, out fragmentShader);

       return (vertexShader, fragmentShader);
    }

    public void RemoveShader(string key, ShaderType shaderType)
    {
        if (shaderType == ShaderType.Fragment)
        {
            _fragmentShaders.Remove(key);
        }
        else if (shaderType == ShaderType.Vertex)
        {
            _vertexShaders.Remove(key);
        }
    }


    public void Upload(Silk.NET.OpenGLES.GL gl)
    {
    }

    public void Destroy(Silk.NET.OpenGLES.GL gl)
    {
        foreach(var shader in Shaders)
        {
            gl.DeleteProgram(shader.Value.ProgramId);
        }
        Shaders.Clear();
    }
}


public class Channel
{
    public string Name { get; set; } = string.Empty;

    public ITexture? Texture { get; set; }

    public Color Color { get; set; } = Color.White;
}

public enum BlendMode
{
    Opaque,
    Masked,
    Translucent,
}

public enum ShaderType
{
    Vertex,
    Fragment,
}