using System.Numerics;
using DigBuild.Engine.Entities;

namespace DigBuild.Entities
{
    public interface IPhysicalEntityBehavior
    {
        public Vector3 Position { get; set; }

        public IPhysicalEntity? Capability { get; set; }
    }

    public sealed class PhysicalEntityBehavior : IEntityBehavior<IPhysicalEntityBehavior>
    {
        public void Init(IPhysicalEntityBehavior data)
        {
            data.Capability = new PhysicalEntity(data);
        }

        public void Build(EntityBehaviorBuilder<IPhysicalEntityBehavior, IPhysicalEntityBehavior> entity)
        {
            entity.Add(EntityAttributes.Position, (_, data, _, _) => data.Position);
            entity.Add(EntityCapabilities.PhysicalEntity, (_, data, _, _) => data.Capability!);
        }

        private sealed class PhysicalEntity : IPhysicalEntity
        {
            private readonly IPhysicalEntityBehavior _data;

            public PhysicalEntity(IPhysicalEntityBehavior data)
            {
                _data = data;
            }

            public Vector3 Position
            {
                get => _data.Position;
                set => _data.Position = value;
            }
        }
    }

    public interface IPhysicalEntity
    {
        public Vector3 Position { get; set; }
    }
}