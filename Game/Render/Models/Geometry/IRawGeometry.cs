namespace DigBuild.Render.Models.Geometry
{
    /// <summary>
    /// A raw piece of geometry.
    /// </summary>
    public interface IRawGeometry
    {
        /// <summary>
        /// Loads all the textures required by this geometry.
        /// </summary>
        /// <param name="loader">The sprite loader</param>
        void LoadTextures(MultiSpriteLoader loader);

        /// <summary>
        /// Bakes the geometry into its final state.
        /// </summary>
        /// <returns>The baked geometry.</returns>
        IGeometry Bake();
    }
}