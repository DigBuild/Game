using DigBuild.Engine.Render;
using DigBuild.Engine.Ui;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;

namespace DigBuild.Render
{
    public static class UiRenderLayer
    {
        public static IRenderLayer<UiVertex> Text { get; } = new SimpleRenderLayer<UiVertex>(
            UiVertex.CreateTransformer,
            new ResourceName(DigBuildGame.Domain, "ui.vert"),
            new ResourceName(DigBuildGame.Domain, "ui.frag"),
            RenderTextures.UiText,
            false
        );

        public static IRenderLayer<UiVertex> Ui { get; } = new SimpleRenderLayer<UiVertex>(
            UiVertex.CreateTransformer,
            new ResourceName(DigBuildGame.Domain, "ui.vert"),
            new ResourceName(DigBuildGame.Domain, "ui.frag"),
            RenderTextures.UiMain,
            false,
            new BlendOptions
            {
                From = BlendFactor.SrcAlpha,
                To = BlendFactor.OneMinusSrcAlpha,
                Operation = BlendOperation.Add
            }
        );
    }
}