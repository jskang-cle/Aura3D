using Aura3D.Core.Math;
using Aura3D.Core.Nodes;
using Aura3D.Core.Renderers;
using System.Drawing;

namespace Aura3D.Core.Scenes;

public class Scene
{
    public IReadOnlySet<Node> Nodes => _nodes;

    private readonly HashSet<Node> _nodes = [];

    private readonly HashSet<Node> _dirtyNodes = [];

    public Camera MainCamera { get; private set; }

    public Octree StaticMeshOctree { get; set; }

    public RenderPipeline RenderPipeline { get; set; }

    public Scene(Func<Scene, RenderPipeline> createRenderPipeline)
    {
        RenderPipeline = createRenderPipeline(this);

        StaticMeshOctree = new Octree(new System.Numerics.Vector3(1000, 1000, 1000), 5);

        MainCamera = new Camera();

        MainCamera.ClearColor = Color.AliceBlue;

        MainCamera.ClearType = ClearType.Color;

        AddNode(MainCamera);
    }

    public HashSet<ControlRenderTarget> ControlRenderTargets { get; } = new HashSet<ControlRenderTarget>();

    public void AddNode(Node node)
    {
        if (node.CurrentScene != null)
            throw new InvalidOperationException("Node already add to scene");

        if (Nodes.Contains(node))
            throw new InvalidOperationException("Node already exits");

        _nodes.Add(node);

        node.CurrentScene = this;

        RenderPipeline.AddNode(node);

        if (node is Camera camera)
        {
            if (camera.RenderTarget != null && camera.RenderTarget is ControlRenderTarget controlRenderTarget)
            {
                if (ControlRenderTargets.Contains(controlRenderTarget) == false)
                {
                    ControlRenderTargets.Add(controlRenderTarget);
                }
            }
        }


        if (node is IOctreeObject otreeObject)
        {
            otreeObject.OnChanged += OnNodeTransformDirty;
        }

        if (node is Mesh mesh)
        {
            StaticMeshOctree.Add(mesh);
        }

        foreach (var child in node.Children)
        {
            AddNode(child);
        }
    }

    public void RemoveNode(Node node)
    {
        if (node.CurrentScene == null) 
            throw new InvalidOperationException("Node is not attached to any scene.");

        if (Nodes.Contains(node) == false)
            throw new InvalidOperationException("Node does not exist in this scene.");

        _nodes.Remove(node);

        node.CurrentScene = null;

        RenderPipeline.RemoveNode(node);


        if (node is Camera camera)
        {
            if (camera.RenderTarget != null && camera.RenderTarget is ControlRenderTarget controlRenderTarget)
            {
                ControlRenderTargets.Remove(controlRenderTarget);
            }
        }

        if (node is IOctreeObject otreeObject)
        {
            otreeObject.OnChanged -= OnNodeTransformDirty;
        }


        if (node is Mesh mesh)
        {
            StaticMeshOctree.Remove(mesh);
        }

        foreach (var child in node.Children)
        {
            RemoveNode(child);
        }
    }

    public void AddNodeTransformDirty(Node node)
    {
        if (_nodes.Contains(node) == false)
            return;
        if (_dirtyNodes.Contains(node) == true)
            return;
        _dirtyNodes.Add(node);
    }
    void OnNodeTransformDirty(IOctreeObject otreeObject)
    {
        if (otreeObject is not Node node)
            return;
        AddNodeTransformDirty(node);
    }
    public void Update(double deltaTime)
    {
        foreach(var node in _dirtyNodes)
        {
            if (_nodes.Contains(node) == false)
                continue;

            node.UpdateTransform();

            if (node is Mesh mesh)
            {
                StaticMeshOctree.Update(mesh);
            }
        }
        _dirtyNodes.Clear();
        foreach(var node in Nodes)
        {
            node.Update(deltaTime);
        }
    }
}
