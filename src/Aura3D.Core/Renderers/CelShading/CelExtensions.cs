using Aura3D.Core.Resources;
using SharpGLTF.Schema2;
using System;
using System.Threading.Channels;
using FIELDINFO = SharpGLTF.Reflection.FieldInfo;
using JSONREADER = System.Text.Json.Utf8JsonReader;
using JSONWRITER = System.Text.Json.Utf8JsonWriter;

namespace Aura3D.Core;

public class Aura3DCelTextures : ExtraProperties
{
    public string Name => SCHEMANAME;

    public new const string SCHEMANAME = "AURA3D_TEXTURES_CELSHADING";
    protected override string GetSchemaName() => SCHEMANAME;

    public Aura3DCelTextures(){}

    #region reflection
    protected override IEnumerable<string> ReflectFieldsNames()
    {
        yield return "ILM";
        yield return "SDF";
        yield return "ShadowRamp";
        yield return "SpecularRamp";
        foreach (var f in base.ReflectFieldsNames()) yield return f;
    }

    protected override bool TryReflectField(string name, out FIELDINFO value)
    {
        switch (name)
        {
            case "ILM": value = FIELDINFO.From("articulationName", this, instance => instance.ILM); return true;
            case "SDF": value = FIELDINFO.From("isAttachPoint", this, instance => instance.SDF); return true;
            case "ShadowRamp": value = FIELDINFO.From("isAttachPoint", this, instance => instance.ShadowRamp); return true;
            case "SpecularRamp": value = FIELDINFO.From("isAttachPoint", this, instance => instance.SpecularRamp); return true;

            default: return base.TryReflectField(name, out value);
        }
    }

    #endregion

    #region data

    private int ILM;

    private int SDF;

    private int ShadowRamp;

    private int SpecularRamp;

    #endregion

    private static Resources.Texture? GetTextureAtIndex(ModelRoot modelRoot, int index)
    {
        if (index < 0 || index > modelRoot.LogicalTextures.Count)
            return null;
        SharpGLTF.Schema2.Texture glTexture = modelRoot.LogicalTextures[index];

        var data = glTexture.PrimaryImage.Content.Content;
        var tex = TextureLoader.LoadTexture(data.ToArray());
        return tex;
    }

    public static List<Resources.Channel> GetExtenionChannels(ModelRoot modelRoot, SharpGLTF.Schema2.Material material)
    {
        List<Resources.Channel> channels = new List<Resources.Channel>();
        foreach(var extension in material.Extensions)
        {
            if(extension.GetType() == typeof(Aura3DCelTextures))
            {
                Aura3DCelTextures celExt = (Aura3DCelTextures)extension;
                var ILMTexture = GetTextureAtIndex(modelRoot, celExt.ILM);

                string[] texturesNames = { "ILM", "SDF", "ShadowRamp", "SpecularRamp" };
                int i = 0;
                foreach (int textureIdx in new int[] {celExt.ILM, celExt.SDF, celExt.ShadowRamp, celExt.SpecularRamp })
                {
                    var texture = GetTextureAtIndex(modelRoot, textureIdx);
                    if (texture == null)
                    {
                        continue;
                    }
                    var channel = new Resources.Channel();
                    channel.Texture = texture;
                    channel.Name = texturesNames[i];
                    channels.Add(channel);
                    ++i;
                }
            }
        }

        return channels;
    }

    #region serialization

    protected override void SerializeProperties(JSONWRITER writer)
    {
        base.SerializeProperties(writer);
        SerializeProperty(writer, "ILM", ILM);
        SerializeProperty(writer, "SDF", SDF);
        SerializeProperty(writer, "ShadowRamp", ShadowRamp);
        SerializeProperty(writer, "SpecularRamp", SpecularRamp);
    }

    protected override void DeserializeProperty(string jsonPropertyName, ref JSONREADER reader)
    {
        switch (jsonPropertyName)
        {
            case "ILM": DeserializePropertyValue<Aura3DCelTextures, int>(ref reader, this, out ILM); break;
            case "SDF": DeserializePropertyValue<Aura3DCelTextures, int>(ref reader, this, out SDF); break;
            case "ShadowRamp": DeserializePropertyValue<Aura3DCelTextures, int>(ref reader, this, out ShadowRamp); break;
            case "SpecularRamp": DeserializePropertyValue<Aura3DCelTextures, int>(ref reader, this, out SpecularRamp); break;
            default: base.DeserializeProperty(jsonPropertyName, ref reader); break;
        }
    }

    #endregion
}
