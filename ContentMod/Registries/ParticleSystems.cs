using DigBuild.Content.Particles;
using DigBuild.Engine.Particles;
using DigBuild.Events;
using DigBuild.Platform.Resource;

namespace DigBuild.Content.Registries
{
    public static class ParticleSystems
    {
        public static ParticleSystem<FireParticle, GpuFireParticle> Fire { get; private set; } = null!;
        
        internal static void Register(ParticleSystemInitializationEvent evt)
        {
            Fire = evt.Create<FireParticle, GpuFireParticle>(
                new ResourceName(Game.Domain, "particles/fire.vert"),
                new ResourceName(Game.Domain, "particles/fire.frag"),
                new ResourceName(Game.Domain, "textures/particles/square_faded.png")
            );
        }
    }
}