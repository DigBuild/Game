using System;
using System.Numerics;
using DigBuild.Engine.Render;
using DigBuild.Platform.Render;
using DigBuild.Platform.Resource;

namespace DigBuild.Render
{
    public interface ISkyRenderer : IDisposable
    {
        void Setup(RenderContext context, ResourceManager resourceManager, RenderStage renderStage);
        void Update(RenderContext context, ICamera camera, ViewFrustum viewFrustum, Matrix4x4 projection, float partialTick);
        void Record(RenderContext context, CommandBufferRecorder cmd);
    }
}