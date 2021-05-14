using System.Collections.Generic;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items;
using DigBuild.Engine.Render;
using DigBuild.Platform.Resource;
using DigBuild.Registries;
using DigBuild.Render.Models;

namespace DigBuild.Render
{
    public sealed class ModelManager
    {
        private readonly Dictionary<Block, IRawModel<IBlockModel>> _rawBlockModels = new();
        private readonly Dictionary<Item, IRawModel<IItemModel>> _rawItemModels = new();
        private readonly Dictionary<Entity, IRawModel<IEntityModel>> _rawEntityModels = new();

        private readonly Dictionary<Block, IBlockModel> _blockModels = new();
        private readonly Dictionary<Item, IItemModel> _itemModels = new();
        private readonly Dictionary<Entity, IEntityModel> _entityModels = new();
        
        public IReadOnlyDictionary<Block, IBlockModel> BlockModels => _blockModels;
        public IReadOnlyDictionary<Item, IItemModel> ItemModels => _itemModels;
        public IReadOnlyDictionary<Entity, IEntityModel> EntityModels => _entityModels;

        public void Load(ResourceManager resourceManager)
        {
            _rawBlockModels.Clear();
            _rawItemModels.Clear();
            _rawEntityModels.Clear();

            var rawMissingModelDefinition = resourceManager.Get<RawCuboidModelDefinition>(DigBuildGame.Domain, "missing")!;
            var rawMissingModel = new RawCuboidModel(rawMissingModelDefinition);

            LoadBlockModels(resourceManager, rawMissingModel);
            LoadItemModels(resourceManager, rawMissingModel);
        }

        private void LoadBlockModels(ResourceManager resourceManager, IRawModel<IBlockModel> missingModel)
        {
            foreach (var block in GameRegistries.Blocks.Values)
            {
                var rawModelDefinition = resourceManager.Get<RawCuboidModelDefinition>(block.Name);
                if (rawModelDefinition != null)
                {
                    _rawBlockModels[block] = new RawCuboidModel(rawModelDefinition);
                    continue;
                }

                var rawObjModel = resourceManager.Get<RawObjModel>(block.Name.Domain, $"blocks/{block.Name.Path}");
                if (rawObjModel != null)
                {
                    _rawBlockModels[block] = rawObjModel;
                    continue;
                }

                _rawBlockModels[block] = missingModel;
            }
        }

        private void LoadItemModels(ResourceManager resourceManager, IRawModel<IItemModel> missingModel)
        {
            foreach (var item in GameRegistries.Items.Values)
            {
                if (GameRegistries.Blocks.TryGet(item.Name, out var block))
                {
                    if (_rawBlockModels.TryGetValue(block, out var rawModel) && rawModel is IRawModel<IItemModel> rawItemModel)
                    {
                        _rawItemModels[item] = rawItemModel;
                        continue;
                    }

                    _rawItemModels[item] = missingModel;
                    continue;
                }

                var rawObjModel = resourceManager.Get<RawObjModel>(item.Name.Domain, $"items/{item.Name.Path}");
                if (rawObjModel != null)
                {
                    _rawItemModels[item] = rawObjModel;
                    continue;
                }

                _rawItemModels[item] = missingModel;
            }
        }

        public void LoadTextures(MultiSpriteLoader loader)
        {
            foreach (var model in _rawBlockModels.Values)
                model.LoadTextures(loader);
            foreach (var model in _rawItemModels.Values)
                model.LoadTextures(loader);
            foreach (var model in _rawEntityModels.Values)
                model.LoadTextures(loader);
        }

        public void Bake()
        {
            _blockModels.Clear();
            _itemModels.Clear();
            _entityModels.Clear();

            foreach (var (block, rawModel) in _rawBlockModels)
                _blockModels[block] = rawModel.Build();
            foreach (var (item, rawModel) in _rawItemModels)
                _itemModels[item] = rawModel.Build();
            foreach (var (entity, rawModel) in _rawEntityModels)
                _entityModels[entity] = rawModel.Build();
        }

        public IBlockModel this[Block block]
        {
            get => _blockModels[block];
            set => _blockModels[block] = value;
        }

        public IItemModel this[Item item]
        {
            get => _itemModels[item];
            set => _itemModels[item] = value;
        }

        public IEntityModel this[Entity entity]
        {
            get => _entityModels[entity];
            set => _entityModels[entity] = value;
        }
    }
}