using System;
using System.Collections.Generic;
using DigBuild.Blocks;
using DigBuild.Content.Registries;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Math;
using DigBuild.Engine.Storage;
using DigBuild.Engine.Worlds;
using DigBuild.Engine.Worlds.Impl;
using DigBuild.Registries;

namespace DigBuild.Content.Behaviors
{
    public interface IMultiblockBehavior
    {
        Vector3I Master { get; set; }

        MultiblockMasterInfo? MasterInfo { get; set; }
    }

    public class MultiblockBehavior : IBlockBehavior<IMultiblockBehavior>
    {
        public void Build(BlockBehaviorBuilder<IMultiblockBehavior, IMultiblockBehavior> block)
        {
            block.Add(BlockCapabilities.InternalMultiblock, GetMultiblock);
            block.Subscribe(OnBreaking);
        }

        private IInternalMultiblock GetMultiblock(IBlockContext context, IMultiblockBehavior data, Func<IInternalMultiblock?> next)
        {
            return new InternalMultiblock(data);
        }

        private void OnBreaking(BlockEvent.Breaking evt, IMultiblockBehavior data, Action next)
        {
            var masterPos = evt.Pos + data.Master;
            var masterBlock = evt.World.GetBlock(masterPos);
            if (masterBlock?.Get(evt.World, masterPos, BlockCapabilities.InternalMultiblock) is not InternalMultiblock multiblock)
            {
                next();
                return;
            }

            foreach (var offsetFromMaster in multiblock.MasterInfo!.Blocks)
            {
                var pos = masterPos + offsetFromMaster;
                evt.World.SetBlock(pos, null);
            }
        }

        public static void InitSlave(IWorld world, BlockPos pos, BlockPos masterPos)
        {
            var block = world.GetBlock(pos);
            if (block?.Get(world, pos, BlockCapabilities.InternalMultiblock) is not InternalMultiblock multiblock)
                return;

            multiblock.Master = new Vector3I(masterPos - pos);
        }

        public static void InitMaster(IWorld world, BlockPos pos, HashSet<Vector3I> blocks)
        {
            var block = world.GetBlock(pos);
            if (block?.Get(world, pos, BlockCapabilities.InternalMultiblock) is not InternalMultiblock multiblock)
                return;

            multiblock.MasterInfo = new MultiblockMasterInfo(blocks);
        }

        private sealed class InternalMultiblock : IInternalMultiblock
        {
            private readonly IMultiblockBehavior _data;

            public Vector3I Master
            {
                set => _data.Master = value;
            }
            public MultiblockMasterInfo? MasterInfo {
                get => _data.MasterInfo;
                set => _data.MasterInfo = value;
            }

            public InternalMultiblock(IMultiblockBehavior data)
            {
                _data = data;
            }
        }
    }

    public sealed class MultiblockMasterInfo
    {
        public HashSet<Vector3I> Blocks { get; }

        public MultiblockMasterInfo(HashSet<Vector3I> blocks)
        {
            Blocks = blocks;
        }
    }

    public interface IInternalMultiblock
    {
    }

    public sealed class MultiblockData : IData<MultiblockData>, IMultiblockBehavior
    {
        public Vector3I Master { get; set; }
        public MultiblockMasterInfo? MasterInfo { get; set; }

        public MultiblockData Copy()
        {
            return new()
            {
                Master = Master,
                MasterInfo = MasterInfo == null ? null : new MultiblockMasterInfo(MasterInfo.Blocks)
            };
        }
    }
}