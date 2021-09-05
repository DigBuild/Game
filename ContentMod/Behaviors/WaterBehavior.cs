using System;
using DigBuild.Content.Models.Blocks;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render.Models;
using DigBuild.Engine.Worlds.Impl;
using DigBuild.Registries;

namespace DigBuild.Content.Behaviors
{
    public sealed class WaterBehavior : IBlockBehavior
    {
        public void Build(BlockBehaviorBuilder<object, object> block)
        {
            block.Add(ModelData.BlockAttribute, GetModelData);
            block.Add(GameBlockAttributes.Water, (_, _, _) => true);
        }

        private static ModelData GetModelData(IReadOnlyBlockContext context, object _, Func<ModelData> next)
        {
            var data = next();

            var above = context.World.GetBlock(context.Pos.Offset(Direction.PosY));
            var same = above == context.Block;

            data.CreateOrExtend((WaterModelData d) => d.TopFace = !same);

            return data;
        }
    }
}