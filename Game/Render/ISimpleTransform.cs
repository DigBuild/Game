using System.Numerics;
using DigBuild.Platform.Render;
using DigBuild.Render.GeneratedUniforms;

namespace DigBuild.Render
{
    public interface ISimpleTransform : IUniform<SimpleTransform>
    {
        public Matrix4x4 Matrix { get; set; }
    }
}