namespace DigBuild.Render.Models.Geometry
{
    public interface IRawGeometry
    {
        void LoadTextures(MultiSpriteLoader loader);
        IGeometry Build();
    }
}