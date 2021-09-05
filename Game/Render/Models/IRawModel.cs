namespace DigBuild.Render.Models
{
    /// <summary>
    /// A raw un-baked model.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRawModel<out T>
    {
        /// <summary>
        /// Loads the textures required by this model.
        /// </summary>
        /// <param name="loader">The sprite loader</param>
        void LoadTextures(MultiSpriteLoader loader);

        /// <summary>
        /// Bakes this model into its final form.
        /// </summary>
        /// <returns>The baked model.</returns>
        T Bake();
    }
}