using DigBuild.Engine.Events;
using DigBuild.Engine.Textures;
using DigBuild.Platform.Resource;

namespace DigBuild.Events
{
    public sealed class UiTextureStitchingEvent : IEvent
    {
        public TextureStitcher Stitcher { get; }
        public ResourceManager ResourceManager { get; }

        public UiTextureStitchingEvent(TextureStitcher stitcher, ResourceManager resourceManager)
        {
            Stitcher = stitcher;
            ResourceManager = resourceManager;
        }
    }
}