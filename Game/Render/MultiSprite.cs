using DigBuild.Engine.Textures;
using DigBuild.Platform.Resource;

namespace DigBuild.Render
{
    /// <summary>
    /// A combination color + bloom sprite.
    /// </summary>
    public sealed class MultiSprite
    {
        /// <summary>
        /// The color sprite.
        /// </summary>
        public ISprite Color { get; init; }

        /// <summary>
        /// The bloom sprite.
        /// </summary>
        public ISprite Bloom { get; init; }

        public MultiSprite(ISprite color, ISprite bloom)
        {
            Color = color;
            Bloom = bloom;
        }
        
        /// <summary>
        /// Loads a multi-sprite.
        /// </summary>
        /// <param name="manager">The resource manager</param>
        /// <param name="stitcher">The stitcher</param>
        /// <param name="domain">The domain</param>
        /// <param name="path">The path</param>
        /// <returns>The multi-sprite if successful, otherwise null</returns>
        internal static MultiSprite? Load(ResourceManager manager, TextureStitcher stitcher, string domain, string path)
        {
            return Load(manager, stitcher, new ResourceName(domain, path));
        }
        
        /// <summary>
        /// Loads a multi-sprite.
        /// </summary>
        /// <param name="manager">The resource manager</param>
        /// <param name="stitcher">The stitcher</param>
        /// <param name="name">The name</param>
        /// <returns>The multi-sprite if successful, otherwise null</returns>
        internal static MultiSprite? Load(ResourceManager manager, TextureStitcher stitcher, ResourceName name)
        {
            var actualPath = name;
            if (!actualPath.Path.EndsWith(".png"))
                actualPath = new ResourceName(name.Domain, $"textures/{name.Path}.png");

            if (!manager.TryGet<BitmapTexture>(actualPath, out var colorTexture))
                return null;

            var bloomPath = new ResourceName(name.Domain, $"textures/{name.Path}.glow.png");
            if (!manager.TryGet<BitmapTexture>(bloomPath, out var bloomTexture))
                bloomTexture = manager.Get<BitmapTexture>(DigBuildGame.Domain, "textures/noglow.png")!;
                
            return new MultiSprite(stitcher.Add(colorTexture), stitcher.Add(bloomTexture));
        }
    }
}