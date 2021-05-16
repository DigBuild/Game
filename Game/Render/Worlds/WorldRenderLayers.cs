using DigBuild.Engine.Render;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;

namespace DigBuild.Render.Worlds
{
    public static class WorldRenderLayers
    {
        public static IRenderLayer<WorldVertex> Opaque { get; } = new SimpleRenderLayer<WorldVertex>(
            WorldVertex.CreateTransformer,
            new ResourceName(DigBuildGame.Domain, "world/base.vert"),
            new ResourceName(DigBuildGame.Domain, "world/opaque.frag"),
            RenderTextures.Main
        );
        
        public static IRenderLayer<WorldVertex> Cutout { get; } = new SimpleRenderLayer<WorldVertex>(
            WorldVertex.CreateTransformer,
            new ResourceName(DigBuildGame.Domain, "world/base.vert"),
            new ResourceName(DigBuildGame.Domain, "world/cutout.frag"),
            RenderTextures.Main
        );
        
        public static IRenderLayer<WorldVertex> Translucent { get; } = new SimpleRenderLayer<WorldVertex>(
            WorldVertex.CreateTransformer,
            new ResourceName(DigBuildGame.Domain, "world/base.vert"),
            new ResourceName(DigBuildGame.Domain, "world/translucent.frag"),
            RenderTextures.Main,
            blend: new BlendOptions
            {
                From = BlendFactor.SrcAlpha,
                To = BlendFactor.OneMinusSrcAlpha,
                Operation = BlendOperation.Add
            }
        );
    }
}