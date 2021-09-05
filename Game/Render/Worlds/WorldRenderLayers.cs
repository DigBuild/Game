using DigBuild.Engine.Render;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;

namespace DigBuild.Render.Worlds
{
    /// <summary>
    /// The game's world render layers.
    /// </summary>
    public static class WorldRenderLayers
    {
        /// <summary>
        /// The opaque layer.
        /// </summary>
        public static IRenderLayer<WorldVertex> Opaque { get; } = new SimpleRenderLayer<WorldVertex>(
            WorldVertex.CreateTransformer,
            new ResourceName(DigBuildGame.Domain, "world/base.vert"),
            new ResourceName(DigBuildGame.Domain, "world/opaque.frag"),
            TextureTypes.WorldMain
        );
        
        /// <summary>
        /// The cutout layer.
        /// </summary>
        public static IRenderLayer<WorldVertex> Cutout { get; } = new SimpleRenderLayer<WorldVertex>(
            WorldVertex.CreateTransformer,
            new ResourceName(DigBuildGame.Domain, "world/base.vert"),
            new ResourceName(DigBuildGame.Domain, "world/cutout.frag"),
            TextureTypes.WorldMain
        );
        
        /// <summary>
        /// The water layer.
        /// </summary>
        public static IRenderLayer<WorldVertex> Water { get; } = new SimpleRenderLayer<WorldVertex>(
            WorldVertex.CreateTransformer,
            new ResourceName(DigBuildGame.Domain, "world/water.vert"),
            new ResourceName(DigBuildGame.Domain, "world/water.frag"),
            TextureTypes.WorldMain,
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