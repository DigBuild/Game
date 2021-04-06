using DigBuild.Engine.Textures;

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
    }
}