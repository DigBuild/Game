using System;
using DigBuild.Engine.Render.Worlds;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;

namespace DigBuild.Render.Worlds
{
    public interface ISkyRenderer : IDisposable
    {
        void Setup(RenderContext context, ResourceManager resourceManager, RenderStage renderStage);
        void Update(RenderContext context, WorldView worldView, float partialTick);
        void Record(RenderContext context, CommandBufferRecorder cmd);
    }
}