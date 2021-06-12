using System;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Worlds;
using DigBuild.Engine.Worlds;
using DigBuild.Platform.Render;
using DigBuild.Render.GeneratedUniforms;
using DigBuild.Worlds;

namespace DigBuild.Render.Worlds
{
    public sealed class WorldTimeInjector : IWorldRenderer
    {
        private readonly IReadOnlyWorld _world;

        public WorldTimeInjector(IReadOnlyWorld world)
        {
            _world = world;
        }

        public void Dispose()
        {
        }

        public void Update(RenderContext context, WorldView worldView, float partialTick)
        {
        }

        public void BeforeDraw(RenderContext context, CommandBufferRecorder cmd, UniformBufferSet uniforms, WorldView worldView, float partialTick)
        {
            var timeOfDay = (_world.AbsoluteTime % World.DayDuration) / (float) World.DayDuration;
            var timeFactor = MathF.Sin(timeOfDay * 2 * MathF.PI) * 0.5f + 0.5f;
            uniforms.Push(RenderUniforms.WorldTime, new WorldTimeUniform {WorldTime = timeFactor});
        }

        public void Draw(
            RenderContext context, CommandBufferRecorder cmd, IRenderLayer layer, RenderLayerBindingSet bindings,
            IReadOnlyUniformBufferSet uniforms, WorldView worldView, float partialTick
        )
        {
        }

        public void AfterDraw(RenderContext context, CommandBufferRecorder cmd, WorldView worldView, float partialTick)
        {
        }
    }
}