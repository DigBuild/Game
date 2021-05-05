using DigBuild.Render;

namespace DigBuild.Client.Models
{
    public interface IRawModel<out T>
    {
        void LoadTextures(MultiSpriteLoader loader);

        T Build();
    }
}