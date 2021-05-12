using DigBuild.Engine.Entities;
using DigBuild.Engine.Storage;
using DigBuild.Registries;

namespace DigBuild.Players
{
    public sealed class PlayerBehaviorData : IData<PlayerBehaviorData>, IPlayerEntity
    {
        public PlayerInventory Inventory { get; init; } = new();
        public PlayerState State { get; init; } = new();

        public PlayerBehaviorData Copy()
        {
            return new()
            {
                Inventory = Inventory.Copy(),
                State = State.Copy()
            };
        }
    }

    public sealed class PlayerBehavior : IEntityBehavior<PlayerBehaviorData>
    {
        public void Build(EntityBehaviorBuilder<PlayerBehaviorData, PlayerBehaviorData> entity)
        {
            entity.Add(EntityCapabilities.PlayerEntity, (_, data, _) => data);
        }
    }

    public interface IPlayerEntity
    {
        PlayerInventory Inventory { get; }

        PlayerState State { get; }
    }
}