using DigBuild.Audio;
using DigBuild.Engine.Registries;

namespace DigBuild.Registries
{
    /// <summary>
    /// The game's sounds.
    /// </summary>
    public class GameSounds
    {
        /// <summary>
        /// Nature ambiance 1.
        /// </summary>
        public static Sound Nature1 { get; private set; } = null!;
        /// <summary>
        /// Nature ambiance 2.
        /// </summary>
        public static Sound Nature2 { get; private set; } = null!;

        /// <summary>
        /// Interaction boop.
        /// </summary>
        public static Sound Boop { get; private set; } = null!;
        
        internal static void Register(RegistryBuilder<Sound> registry)
        {
            Nature1 = registry.Register(DigBuildGame.Domain, "bbc/nature1");
            Nature2 = registry.Register(DigBuildGame.Domain, "bbc/nature2");
            
            Boop = registry.Register(DigBuildGame.Domain, "boop");
        }
    }
}