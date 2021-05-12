using DigBuild.Content.Particles;
using DigBuild.Engine.Particles;
using DigBuild.Events;
using DigBuild.Platform.Resource;
using DigBuild.Registries;

namespace DigBuild.Content.Registries
{
    public static class ParticleSystems
    {
        public static ParticleSystem<FireParticle, GpuFireParticle> Fire { get; private set; } = null!;
        
        internal static void Register(RegistryBuildingEvent<IParticleSystemData> evt)
        {
            Fire = evt.Registry.Create<FireParticle, GpuFireParticle>(
                DigBuildGame.Domain, "fire",
                new ResourceName(DigBuildGame.Domain, "particles/fire.vert"),
                new ResourceName(DigBuildGame.Domain, "particles/fire.frag"),
                new ResourceName(DigBuildGame.Domain, "textures/particles/square_faded.png")
            );
        }
    }
}