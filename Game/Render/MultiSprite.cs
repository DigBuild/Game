using DigBuild.Engine.Textures;
using DigBuild.Platform.Resource;

namespace DigBuild.Render
{
    public sealed class MultiSprite
    {
        public ISprite Color { get; init; }

        public ISprite Bloom { get; init; }

        public MultiSprite(ISprite color, ISprite bloom)
        {
            Color = color;
            Bloom = bloom;
        }

        public static MultiSpriteLoader Loader(ResourceManager manager, TextureStitcher stitcher)
        {
            return new(manager, stitcher);
        }

        public static MultiSprite? Load(ResourceManager manager, TextureStitcher stitcher, string domain, string path)
        {
            return Load(manager, stitcher, new ResourceName(domain, path));
        }

        public static MultiSprite? Load(ResourceManager manager, TextureStitcher stitcher, ResourceName name)
        {
            var actualPath = name;
            if (!actualPath.Path.EndsWith(".png"))
                actualPath = new ResourceName(name.Domain, $"textures/{name.Path}.png");

            if (!manager.TryGet<BitmapTexture>(actualPath, out var colorTexture))
                return null;

            var bloomPath = new ResourceName(name.Domain, $"textures/{name.Path}.glow.png");
            if (!manager.TryGet<BitmapTexture>(bloomPath, out var bloomTexture))
                bloomTexture = manager.Get<BitmapTexture>(Game.Domain, "textures/noglow.png")!;
                
            return new MultiSprite(stitcher.Add(colorTexture), stitcher.Add(bloomTexture));
        }
    }

    public sealed class MultiSpriteLoader
    {
        private readonly ResourceManager _manager;
        private readonly TextureStitcher _stitcher;

        internal MultiSpriteLoader(ResourceManager manager, TextureStitcher stitcher)
        {
            _manager = manager;
            _stitcher = stitcher;
        }

        public MultiSprite? Load(string domain, string path)
        {
            return MultiSprite.Load(_manager, _stitcher, domain, path);
        }

        public MultiSprite? Load(ResourceName name)
        {
            return MultiSprite.Load(_manager, _stitcher, name);
        }
    }
}