using DigBuild.Behaviors;
using DigBuild.Controller;
using DigBuild.Engine.Entities;

namespace DigBuild.Players
{
    /// <summary>
    /// A player.
    /// </summary>
    public interface IPlayer
    {
        /// <summary>
        /// The gameplay controller.
        /// </summary>
        GameplayController GameplayController { get; }

        /// <summary>
        /// The player's entity.
        /// </summary>
        EntityInstance Entity { get; }
        /// <summary>
        /// The physics entity.
        /// </summary>
        IPhysicsEntity PhysicsEntity { get; }
        /// <summary>
        /// The inventory.
        /// </summary>
        IPlayerInventory Inventory { get; }
        /// <summary>
        /// The state.
        /// </summary>
        IPlayerState State { get; }

        /// <summary>
        /// Gets the interpolated camera.
        /// </summary>
        /// <param name="partialTick">The tick delta</param>
        /// <returns>The camera</returns>
        IPlayerCamera GetCamera(float partialTick);
    }
}