using DigBuild.Engine.Events;
using DigBuild.Engine.Render;
using DigBuild.Engine.Textures;
using DigBuild.Platform.Resource;

namespace DigBuild.Events
{
    public sealed class TextureStitchingEvent : IEvent
    {
        public RenderTexture Texture { get; }
        public TextureStitcher Stitcher { get; }
        public ResourceManager ResourceManager { get; }

        public TextureStitchingEvent(RenderTexture texture, TextureStitcher stitcher, ResourceManager resourceManager)
        {
            Texture = texture;
            Stitcher = stitcher;
            ResourceManager = resourceManager;
        }
    }
}