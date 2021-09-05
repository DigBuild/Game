using System.Collections.Generic;
using DigBuild.Engine.Blocks;
using DigBuild.Engine.Entities;
using DigBuild.Engine.Items;
using DigBuild.Engine.Render.Models;
using DigBuild.Platform.Resource;
using DigBuild.Registries;
using DigBuild.Render.Models;
using DigBuild.Render.Models.Json;

namespace DigBuild.Render
{
    /// <summary>
    /// A model loader and manager.
    /// </summary>
    public sealed class ModelManager
    {
        private readonly Dictionary<Block, IRawModel<IBlockModel>> _rawBlockModels = new();
        private readonly Dictionary<Item, IRawModel<IItemModel>> _rawItemModels = new();
        private readonly Dictionary<Entity, IRawModel<IEntityModel>> _rawEntityModels = new();

        private readonly Dictionary<Block, IBlockModel> _blockModels = new();
        private readonly Dictionary<Item, IItemModel> _itemModels = new();
        private readonly Dictionary<Entity, IEntityModel> _entityModels = new();
        
        /// <summary>
        /// The block models.
        /// </summary>
        public IReadOnlyDictionary<Block, IBlockModel> BlockModels => _blockModels;
        /// <summary>
        /// The item models.
        /// </summary>
        public IReadOnlyDictionary<Item, IItemModel> ItemModels => _itemModels;
        /// <summary>
        /// The entity models.
        /// </summary>
        public IReadOnlyDictionary<Entity, IEntityModel> EntityModels => _entityModels;

        /// <summary>
        /// Loads all the models.
        /// </summary>
        /// <param name="resourceManager">The resource manager</param>
        public void Load(ResourceManager resourceManager)
        {
            _rawBlockModels.Clear();
            _rawItemModels.Clear();
            _rawEntityModels.Clear();

            var rawMissingModel = resourceManager.Get<RawJsonModel>(DigBuildGame.Domain, "blocks/missing")!;

            LoadBlockModels(resourceManager, rawMissingModel);
            LoadItemModels(resourceManager, rawMissingModel);
        }

        private void LoadBlockModels(ResourceManager resourceManager, IRawModel<IBlockModel> missingModel)
        {
            foreach (var block in GameRegistries.Blocks.Values)
            {
                var rawJsonModel = resourceManager.Get<RawJsonModel>(block.Name.Domain, "blocks/" + block.Name.Path);
                if (rawJsonModel != null)
                {
                    _rawBlockModels[block] = rawJsonModel;
                    continue;
                }
                
                _rawBlockModels[block] = missingModel;
            }
        }

        private void LoadItemModels(ResourceManager resourceManager, IRawModel<IItemModel> missingModel)
        {
            foreach (var item in GameRegistries.Items.Values)
            {
                var rawJsonModel = resourceManager.Get<RawJsonModel>(item.Name.Domain, $"items/{item.Name.Path}");
                if (rawJsonModel != null)
                {
                    _rawItemModels[item] = rawJsonModel;
                    continue;
                }

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

                _rawItemModels[item] = missingModel;
            }
        }

        /// <summary>
        /// Loads all the textures the models depend on.
        /// </summary>
        /// <param name="loader">The sprite loader</param>
        public void LoadTextures(MultiSpriteLoader loader)
        {
            foreach (var model in _rawBlockModels.Values)
                model.LoadTextures(loader);
            foreach (var model in _rawItemModels.Values)
                model.LoadTextures(loader);
            foreach (var model in _rawEntityModels.Values)
                model.LoadTextures(loader);
        }

        /// <summary>
        /// Bakes all the models into their final geometry.
        /// </summary>
        public void Bake()
        {
            _blockModels.Clear();
            _itemModels.Clear();
            _entityModels.Clear();

            foreach (var (block, rawModel) in _rawBlockModels)
                _blockModels[block] = rawModel.Bake();
            foreach (var (item, rawModel) in _rawItemModels)
                _itemModels[item] = rawModel.Bake();
            foreach (var (entity, rawModel) in _rawEntityModels)
                _entityModels[entity] = rawModel.Bake();
        }

        /// <summary>
        /// The model for a given block.
        /// </summary>
        /// <param name="block">The block</param>
        /// <returns>The model</returns>
        public IBlockModel this[Block block]
        {
            get => _blockModels[block];
            set => _blockModels[block] = value;
        }
        
        /// <summary>
        /// The model for a given item.
        /// </summary>
        /// <param name="item">The item</param>
        /// <returns>The model</returns>
        public IItemModel this[Item item]
        {
            get => _itemModels[item];
            set => _itemModels[item] = value;
        }
        
        /// <summary>
        /// The model for a given entity.
        /// </summary>
        /// <param name="entity">The entity</param>
        /// <returns>The model</returns>
        public IEntityModel this[Entity entity]
        {
            get => _entityModels[entity];
            set => _entityModels[entity] = value;
        }
    }
}