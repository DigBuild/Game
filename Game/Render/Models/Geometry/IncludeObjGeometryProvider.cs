using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using DigBuild.Platform.Resource;
using DigBuild.Render.Models.Expressions;
using DigBuild.Render.Worlds;
using DigBuild.Serialization;
using ObjLoader.Loader.Data;
using ObjLoader.Loader.Data.VertexData;
using ObjLoader.Loader.Loaders;

namespace DigBuild.Render.Models.Geometry
{
    /// <summary>
    /// A geometry provider that loads a geometry OBJ.
    /// </summary>
    public class IncludeObjGeometryProvider : IGeometryProvider
    {
        private static readonly Vector3 VertexOffset = new(0.5f, 0, 0.5f);
        private static readonly ObjLoaderFactory ObjLoaderFactory = new();

        public IPartialGeometry Provide(
            JsonElement json, JsonSerializerOptions jsonOptions,
            ResourceManager resourceManager, ResourceName currentFile
        )
        {
            var modelPath = json.GetProperty<string>("model");
            var modelName = currentFile.GetSibling(modelPath);
            var actualModelName = new ResourceName(modelName.Domain, $"geometry/{modelName.Path}.obj");
            var loader = ObjLoaderFactory.Create(new MaterialStreamProvider(resourceManager, actualModelName));
            var resource = resourceManager.GetResource(actualModelName)!;
            var result = loader.Load(resource.OpenStream())!;
            return new PartialObjGeometry(actualModelName, result);
        }

        private sealed class PartialObjGeometry : IPartialGeometry
        {
            private readonly ResourceName _fullyQualifiedName;
            private readonly LoadResult _rawObj;

            public PartialObjGeometry(ResourceName fullyQualifiedName, LoadResult rawObj)
            {
                _fullyQualifiedName = fullyQualifiedName;
                _rawObj = rawObj;
            }

            public IEnumerable<string> RequiredVariables => Array.Empty<string>();

            public IPartialGeometry ApplySubstitutions(IReadOnlyDictionary<string, IModelExpression> variables) => this;

            public IRawGeometry Prime()
            {
                return new RawObjGeometry(_fullyQualifiedName, _rawObj);
            }
        }

        private sealed class RawObjGeometry : IRawGeometry
        {
            private readonly ResourceName _fullyQualifiedName;
            private readonly LoadResult _rawObj;
            private readonly Dictionary<Material, MultiSprite?> _sprites = new();

            public RawObjGeometry(ResourceName fullyQualifiedName, LoadResult rawObj)
            {
                _fullyQualifiedName = fullyQualifiedName;
                _rawObj = rawObj;
            }

            public void LoadTextures(MultiSpriteLoader loader)
            {
                foreach (var material in _rawObj.Materials)
                {
                    if (material.DiffuseTextureMap == null)
                        continue;
                    var texturePath = _fullyQualifiedName.GetSibling(material.DiffuseTextureMap);
                    _sprites[material] = loader.Load(texturePath)!;
                }
            }

            public IGeometry Bake()
            {
                var vertices = new List<WorldVertex>();
                foreach (var objGroup in _rawObj.Groups)
                {
                    var sprite = _sprites[objGroup.Material];
                    if (sprite == null)
                        continue;
                    foreach (var face in objGroup.Faces)
                    {
                        for (var i = 0; i < face.Count; i++)
                        {
                            var faceVert = face[i];
                            var uvs = _rawObj.Textures[faceVert.TextureIndex - 1];
                            vertices.Add(new WorldVertex(
                                ToVector3(_rawObj.Vertices[faceVert.VertexIndex - 1]) + VertexOffset,
                                ToVector3(_rawObj.Normals[faceVert.NormalIndex - 1]),
                                sprite, 1 - uvs.X, 1 - uvs.Y,
                                1
                            ));
                        }
                    }
                }
                return new SimpleGeometry(vertices.ToArray(), WorldRenderLayers.Opaque);
            }

            private static Vector3 ToVector3(Vertex vertex) => new(vertex.X, vertex.Y, vertex.Z);
            private static Vector3 ToVector3(Normal normal) => new(normal.X, normal.Y, normal.Z);
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