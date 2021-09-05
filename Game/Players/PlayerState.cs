namespace DigBuild.Players
{
    /// <summary>
    /// The player state.
    /// </summary>
    public interface IPlayerState
    {
    }
    
    /// <summary>
    /// The player state.
    /// </summary>
    public sealed class PlayerState : IPlayerState
    {
        /// <summary>
        /// Creates a deep copy of the state.
        /// </summary>
        /// <returns>A deep copy</returns>
        public PlayerState Copy()
        {
            return new PlayerState();
        }
    }
}