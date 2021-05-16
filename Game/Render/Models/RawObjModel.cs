using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using DigBuild.Engine.Render.Models;
using DigBuild.Platform.Resource;
using DigBuild.Render.Worlds;
using ObjLoader.Loader.Data;
using ObjLoader.Loader.Data.VertexData;
using ObjLoader.Loader.Loaders;

namespace DigBuild.Render.Models
{
    public sealed class RawObjModel : ICustomResource, IRawModel<IBlockModel>, IRawModel<IItemModel>
    {
        private static readonly Vector3 VertexOffset = new(0.5f, 0, 0.5f);
        private static readonly ObjLoaderFactory ObjLoaderFactory = new();

        private readonly Dictionary<Material, MultiSprite?> _sprites = new();

        public ResourceName Name { get; }
        public LoadResult RawObj { get; }

        public RawObjModel(ResourceName name, LoadResult rawObj)
        {
            Name = name;
            RawObj = rawObj;
        }

        public void LoadTextures(MultiSpriteLoader loader)
        {
            var fqn = new ResourceName(Name.Domain, $"models/{Name.Path}");
            foreach (var material in RawObj.Materials)
            {
                if (material.DiffuseTextureMap == null)
                    continue;
                var texturePath = fqn.GetSibling(material.DiffuseTextureMap);
                _sprites[material] = loader.Load(texturePath)!;
            }
        }

        private SimpleModel Build()
        {
            var vertices = new List<WorldVertex>();
            foreach (var objGroup in RawObj.Groups)
            {
                var sprite = _sprites[objGroup.Material];
                if (sprite == null)
                    continue;
                foreach (var face in objGroup.Faces)
                {
                    for (var i = 0; i < face.Count; i++)
                    {
                        var faceVert = face[i];
                        var uvs = RawObj.Textures[faceVert.TextureIndex - 1];
                        vertices.Add(new WorldVertex(
                            ToVector3(RawObj.Vertices[faceVert.VertexIndex - 1]) + VertexOffset,
                            ToVector3(RawObj.Normals[faceVert.NormalIndex - 1]),
                            sprite, 1 - uvs.X, 1 - uvs.Y,
                            1
                        ));
                    }
                }
            }
            return new SimpleModel(vertices.ToArray());
        }
        
        IBlockModel IRawModel<IBlockModel>.Build() => Build();
        IItemModel IRawModel<IItemModel>.Build() => Build();
        
        private static Vector3 ToVector3(Vertex vertex) => new(vertex.X, vertex.Y, vertex.Z);
        private static Vector3 ToVector3(Normal normal) => new(normal.X, normal.Y, normal.Z);

        public static RawObjModel? Load(ResourceManager resourceManager, ResourceName name)
        {
            try
            {
                var extendedName = new ResourceName(name.Domain, $"models/{name.Path}.obj");
                var loader = ObjLoaderFactory.Create(new MaterialStreamProvider(resourceManager, extendedName));
                var resource = resourceManager.GetResource(extendedName);
                if (resource == null)
                    return null;
                var result = loader.Load(resource.OpenStream());
                return new RawObjModel(name, result);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private sealed class MaterialStreamProvider : IMaterialStreamProvider
        {
            private readonly ResourceManager _resourceManager;
            private readonly ResourceName _name;

            public MaterialStreamProvider(ResourceManager resourceManager, ResourceName name)
            {
                _resourceManager = resourceManager;
                _name = name;
            }

            public Stream Open(string materialFilePath)
            {
                if (materialFilePath.Contains(':'))
                    return _resourceManager.GetResource(ResourceName.Parse(materialFilePath)!.Value)!.OpenStream();

                var sibling = _name.GetSibling(materialFilePath);
                return _resourceManager.GetResource(sibling)!.OpenStream();
            }
        }
    }
}