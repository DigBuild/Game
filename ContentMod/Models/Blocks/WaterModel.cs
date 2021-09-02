using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;

namespace DigBuild.Content.Models.Blocks
{
    public sealed class WaterModel : IBlockModel
    {
        private readonly IBlockModel _parent;

        public WaterModel(IBlockModel parent)
        {
            _parent = parent;
        }

        public void AddGeometry(IGeometryBuffer buffer, IReadOnlyModelData data, DirectionFlags visibleFaces)
        {
            var d = data.Get<WaterModelData>();
            if (d is not {TopFace: true})
                return;

            _parent.AddGeometry(buffer, data, DirectionFlags.PosY);
        }

        public bool HasDynamicGeometry => false;

        public void AddDynamicGeometry(IGeometryBuffer buffer, IReadOnlyModelData data, DirectionFlags visibleFaces, float partialTick)
        {
        }
    }

    public sealed class WaterModelData
    {
        public bool TopFace { get; set; }
    }
}