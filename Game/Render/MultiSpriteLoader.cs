using DigBuild.Engine.Textures;
using DigBuild.Platform.Resource;

namespace DigBuild.Render
{
    /// <summary>
    /// A multi-sprite loader.
    /// </summary>
    public sealed class MultiSpriteLoader
    {
        private readonly ResourceManager _manager;
        private readonly TextureStitcher _stitcher;

        internal MultiSpriteLoader(ResourceManager manager, TextureStitcher stitcher)
        {
            _manager = manager;
            _stitcher = stitcher;
        }

        /// <summary>
        /// Loads a multi-sprite.
        /// </summary>
        /// <param name="domain">The domain</param>
        /// <param name="path">The path</param>
        /// <returns>The multi-sprite if successful, otherwise null</returns>
        public MultiSprite? Load(string domain, string path)
        {
            return MultiSprite.Load(_manager, _stitcher, domain, path);
        }
        
        /// <summary>
        /// Loads a multi-sprite.
        /// </summary>
        /// <param name="name">The name</param>
        /// <returns>The multi-sprite if successful, otherwise null</returns>
        public MultiSprite? Load(ResourceName name)
        {
            return MultiSprite.Load(_manager, _stitcher, name);
        }
    }
}