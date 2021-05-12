namespace DigBuild.Players
{
    public interface IPlayerState
    {
    }

    public sealed class PlayerState : IPlayerState
    {
        public PlayerState Copy()
        {
            return new();
        }
    }
}