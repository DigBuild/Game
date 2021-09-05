using DigBuild.Engine.Events;
using DigBuild.Engine.Render;
using DigBuild.Engine.Textures;
using DigBuild.Platform.Resource;

namespace DigBuild.Events
{
    /// <summary>
    /// Fired when a texture is about to be stitched.
    /// </summary>
    public sealed class TextureStitchingEvent : IEvent
    {
        /// <summary>
        /// The texture handle.
        /// </summary>
        public TextureType TextureType { get; }
        /// <summary>
        /// The texture stitcher.
        /// </summary>
        public TextureStitcher Stitcher { get; }
        /// <summary>
        /// The resource manager.
        /// </summary>
        public ResourceManager ResourceManager { get; }

        public TextureStitchingEvent(TextureType textureType, TextureStitcher stitcher, ResourceManager resourceManager)
        {
            TextureType = textureType;
            Stitcher = stitcher;
            ResourceManager = resourceManager;
        }
    }
}