using DigBuild.Engine.Events;
using DigBuild.Engine.Render;
using DigBuild.Engine.Textures;
using DigBuild.Platform.Resource;

namespace DigBuild.Events
{
    public sealed class TextureStitchingEvent : IEvent
    {
        public TextureType TextureType { get; }
        public TextureStitcher Stitcher { get; }
        public ResourceManager ResourceManager { get; }

        public TextureStitchingEvent(TextureType textureType, TextureStitcher stitcher, ResourceManager resourceManager)
        {
            TextureType = textureType;
            Stitcher = stitcher;
            ResourceManager = resourceManager;
        }
    }
}