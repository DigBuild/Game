using System;
using System.Numerics;
using DigBuild.Engine.Particles;

namespace DigBuild.Content.Particles
{
    public struct FireParticle : IParticle<GpuFireParticle>
    {
        public const byte MaxAge = 22;

        public byte Age;
        public Vector3 Position;
        public Vector3 Velocity;

        public bool Update(IParticleUpdateContext context)
        {
            var relAge = Age / (float) MaxAge;
            var ageThing = MathF.Abs(relAge - 0.45f) * 1.8f + 0.19f;

            Position += Velocity;
            Velocity.X = (float) ((context.Random.NextDouble() - 0.5) * 0.15) * ageThing * ageThing * ageThing;
            Velocity.Y = (float) (context.Random.NextDouble() * 0.2 + 0.1) * 0.28f * (1.5f - relAge * relAge * relAge * relAge * relAge) / 1.5f;
            Velocity.Z = (float) ((context.Random.NextDouble() - 0.5) * 0.15) * ageThing * ageThing * ageThing;
            Velocity += context.GetWindAt(Position) * 0.1f * (float) context.Random.NextDouble();
            return ++Age < MaxAge;
        }

        public void UpdateGpu(ref GpuFireParticle gpu, float partialTick)
        {
            gpu.Position = Position + Velocity * partialTick;
            gpu.Age = (Age + partialTick) / MaxAge;
        }
    }

    public struct GpuFireParticle
    {
        public Vector3 Position;
        public float Age;
    }
}