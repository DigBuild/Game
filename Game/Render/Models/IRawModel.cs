namespace DigBuild.Render.Models
{
    public interface IRawModel<out T>
    {
        void LoadTextures(MultiSpriteLoader loader);

        T Build();
    }
}