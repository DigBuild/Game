using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;
using DigBuild.Platform.Resource;
using DigBuild.Render.Models.Expressions;
using DigBuild.Render.Models.Json;
using DigBuild.Serialization;

namespace DigBuild.Render.Models.Geometry
{
    public sealed class TransformGeometryProvider : IGeometryProvider
    {
        public IPartialGeometry Provide(
            JsonElement json, JsonSerializerOptions jsonOptions,
            ResourceManager resourceManager, ResourceName currentFile
        )
        {
            var rotationJson = json.GetProperty("rotation");
            var actualRotation = rotationJson.EnumerateObject().First();
            
            var axis = actualRotation.Name switch
            {
                "x" => Axis.X,
                "y" => Axis.Y,
                "z" => Axis.Z,
                _ => throw new ArgumentException("Rotation axis must be x, y or z.")
            };
            var angleExp = ModelExpressionParser.Parse(
                actualRotation.Value.ValueKind == JsonValueKind.Number ?
                    $"{actualRotation.Value.GetDouble()}" :
                    actualRotation.Value.GetString()!
            );

            var center = json.TryGetProperty("center", out var centerJson) ? centerJson.Get<Vector3>(jsonOptions) : Vector3.One / 2;

            var element = GeometryJson.ParseElement(json.GetProperty("element"), jsonOptions, resourceManager, currentFile);

            return new PartialTransformGeometry(element, axis, angleExp, center);
        }

        private sealed class PartialTransformGeometry : IPartialGeometry
        {
            private readonly IPartialGeometry _partialGeometry;
            private readonly Axis _axis;
            private readonly IModelExpression _angle;
            private readonly Vector3 _center;

            public PartialTransformGeometry(IPartialGeometry partialGeometry, Axis axis, IModelExpression angle, Vector3 center)
            {
                _partialGeometry = partialGeometry;
                _axis = axis;
                _angle = angle;
                _center = center;
            }

            public IEnumerable<string> RequiredVariables => _partialGeometry.RequiredVariables;

            public IPartialGeometry ApplySubstitutions(IReadOnlyDictionary<string, IModelExpression> variables)
            {
                var partialGeometry = _partialGeometry.ApplySubstitutions(variables);
                return new PartialTransformGeometry(partialGeometry, _axis, _angle, _center);
            }

            public IRawGeometry Prime()
            {
                var rawGeometry = _partialGeometry.Prime();
                var angleGetter = _angle.CompileDouble();
                return new RawTransformGeometry(rawGeometry, _axis, angleGetter, _center);
            }
        }

        private sealed class RawTransformGeometry : IRawGeometry
        {
            private readonly IRawGeometry _rawGeometry;
            private readonly Axis _axis;
            private readonly Func<IReadOnlyDictionary<string, string>, double> _angleGetter;
            private readonly Vector3 _center;

            public RawTransformGeometry(
                IRawGeometry rawGeometry,
                Axis axis,
                Func<IReadOnlyDictionary<string, string>, double> angleGetter,
                Vector3 center
            )
            {
                _rawGeometry = rawGeometry;
                _axis = axis;
                _angleGetter = angleGetter;
                _center = center;
            }

            public void LoadTextures(MultiSpriteLoader loader)
            {
                _rawGeometry.LoadTextures(loader);
            }

            public IGeometry Build()
            {
                var geometry = _rawGeometry.Build();
                return new TransformGeometry(geometry, _axis, _angleGetter, _center);
            }
        }

        private sealed class TransformGeometry : IGeometry
        {
            private readonly IGeometry _geometry;
            private readonly Axis _axis;
            private readonly Func<IReadOnlyDictionary<string, string>, double> _angleGetter;
            private readonly Vector3 _center;

            public TransformGeometry(IGeometry geometry, Axis axis, Func<IReadOnlyDictionary<string, string>, double> angleGetter, Vector3 center)
            {
                _geometry = geometry;
                _axis = axis;
                _angleGetter = angleGetter;
                _center = center;
            }

            public void Add(IGeometryBuffer buffer, IReadOnlyModelData data, DirectionFlags visibleFaces)
            {
                var jsonData = data.Get<JsonModelData>();
                
                var angle = _angleGetter(jsonData?.Data ?? ImmutableDictionary<string, string>.Empty);
                if (angle != 0)
                {
                    var rad = (float)(2 * Math.PI * angle / 360);
                    var mat = _axis switch
                    {
                        Axis.X => Matrix4x4.CreateRotationX(rad, _center),
                        Axis.Y => Matrix4x4.CreateRotationY(rad, _center),
                        Axis.Z => Matrix4x4.CreateRotationZ(rad, _center),
                        _ => throw new InvalidOperationException()
                    };
                
                    buffer.Transform = mat * buffer.Transform;
                }

                _geometry.Add(buffer, data, visibleFaces);
            }
        }
    }
}