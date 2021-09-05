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
            TextureHandles.Main
        );
        
        public static IRenderLayer<WorldVertex> Cutout { get; } = new SimpleRenderLayer<WorldVertex>(
            WorldVertex.CreateTransformer,
            new ResourceName(DigBuildGame.Domain, "world/base.vert"),
            new ResourceName(DigBuildGame.Domain, "world/cutout.frag"),
            TextureHandles.Main
        );
        
        public static IRenderLayer<WorldVertex> Water { get; } = new SimpleRenderLayer<WorldVertex>(
            WorldVertex.CreateTransformer,
            new ResourceName(DigBuildGame.Domain, "world/water.vert"),
            new ResourceName(DigBuildGame.Domain, "world/water.frag"),
            TextureHandles.Main,
            writeDepth: false,
            blend: new BlendOptions
            { 
                From = BlendFactor.SrcAlpha, 
                To = BlendFactor.OneMinusSrcAlpha, 
                Operation = BlendOperation.Add 
            } 
        );
    }
}