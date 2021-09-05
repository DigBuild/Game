using DigBuild.Engine.Render;
using DigBuild.Engine.Ui;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;

namespace DigBuild.Render
{
    /// <summary>
    /// The game's UI render layers.
    /// </summary>
    public static class UiRenderLayers
    {
        /// <summary>
        /// The text render layer.
        /// </summary>
        public static IRenderLayer<UiVertex> Text { get; } = new SimpleRenderLayer<UiVertex>(
            UiVertex.CreateTransformer,
            new ResourceName(DigBuildGame.Domain, "ui.vert"),
            new ResourceName(DigBuildGame.Domain, "ui.frag"),
            TextureTypes.UiText,
            false
        );

        /// <summary>
        /// The base UI render layer.
        /// </summary>
        public static IRenderLayer<UiVertex> Ui { get; } = new SimpleRenderLayer<UiVertex>(
            UiVertex.CreateTransformer,
            new ResourceName(DigBuildGame.Domain, "ui.vert"),
            new ResourceName(DigBuildGame.Domain, "ui.frag"),
            TextureTypes.UiMain,
            false, true,
            new BlendOptions
            {
                From = BlendFactor.SrcAlpha,
                To = BlendFactor.OneMinusSrcAlpha,
                Operation = BlendOperation.Add
            }
        );

        /// <summary>
        /// The overlay render layer.
        /// </summary>
        public static IRenderLayer<UiVertex> UiOverlay { get; } = new SimpleRenderLayer<UiVertex>(
            UiVertex.CreateTransformer,
            new ResourceName(DigBuildGame.Domain, "ui.vert"),
            new ResourceName(DigBuildGame.Domain, "ui.frag"),
            TextureTypes.UiMain,
            false, true,
            new BlendOptions
            {
                From = BlendFactor.SrcAlpha,
                To = BlendFactor.OneMinusSrcAlpha,
                Operation = BlendOperation.Add
            }
        );
    }
}