using System;
using System.Numerics;
using DigBuild.Blocks;
using DigBuild.Content.Registries;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Ticking;
using DigBuild.Engine.Worlds;
using DigBuild.Engine.Worlds.Impl;

namespace DigBuild.Content.Behaviors
{
    public sealed class CampfireBehavior : IBlockBehavior
    {
        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Subscribe(OnPlaced);
        }

        private void OnPlaced(BlockEvent.Placed evt, object data, Action next)
        {
            evt.World.TickScheduler.After(1).Tick += () => SpawnParticles(evt.World.TickScheduler, evt.World, evt.Pos, evt.Block);
        }

        private void SpawnParticles(Scheduler scheduler, IReadOnlyWorld world, BlockPos pos, Block block)
        {
            if (world.GetBlock(pos) != block)
                return;

            var rnd = new Random();
            var origin = (Vector3) pos + new Vector3(0.5f, 0.125f, 0.5f);

            var particles = ParticleSystems.Fire.Create(25);
            foreach (ref var particle in particles)
            {
                var angle = (float) rnd.NextDouble() * MathF.PI * 2;
                var spread = MathF.Pow((float) rnd.NextDouble(), 3);
                var offset = new Vector3(
                    MathF.Sin(angle),
                    ((float) rnd.NextDouble() - 0.5f + spread * 0.5f) * 0.4f,
                    MathF.Cos(angle)
                );
                var actualOffset = offset * spread * 0.55f;
                particle.Position = origin + actualOffset;
                particle.Age = (byte) (spread * spread * 12);
            }

            scheduler.After(1).Tick += () => SpawnParticles(scheduler, world, pos, block);
        }
    }
}