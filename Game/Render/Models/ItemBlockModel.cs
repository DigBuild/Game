using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;

namespace DigBuild.Render.Models
{
    public sealed class ItemBlockModel : IItemModel
    {
        private readonly IBlockModel _parent;

        public ItemBlockModel(IBlockModel parent)
        {
            _parent = parent;
        }

        public void AddGeometry(IGeometryBuffer buffer, IReadOnlyModelData data, ItemModelTransform transform, float partialTick)
        {
            buffer.Transform = transform.GetMatrix() * buffer.Transform;

            var modelData = new ModelData();
            _parent.AddGeometry(buffer, modelData, DirectionFlags.All);
            if (_parent.HasDynamicGeometry)
                _parent.AddDynamicGeometry(buffer, modelData, DirectionFlags.All, partialTick);
        }
    }
}