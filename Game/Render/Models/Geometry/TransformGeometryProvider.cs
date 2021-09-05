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
    /// <summary>
    /// A geometry provider that applies a 3D transform to a nested geometry.
    /// </summary>
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

                var requiredVariables = new HashSet<string>(_partialGeometry.RequiredVariables);
                foreach (var variable in angle.RequiredVariables)
                    requiredVariables.Add(variable);
                RequiredVariables = requiredVariables;
            }

            public IEnumerable<string> RequiredVariables { get; }

            public IPartialGeometry ApplySubstitutions(IReadOnlyDictionary<string, IModelExpression> variables)
            {
                var partialGeometry = _partialGeometry.ApplySubstitutions(variables);
                return new PartialTransformGeometry(partialGeometry, _axis, _angle, _center);
            }

            public IRawGeometry Prime()
            {
                var rawGeometry = _partialGeometry.Prime();
                var angleGetter = _angle.CompileDouble();

                Func<IReadOnlyDictionary<string, string>, Matrix4x4> transformGetter;
                if (_angle.RequiredVariables.Any())
                {
                    transformGetter = data => CreateRotation(data, angleGetter, _axis, _center);
                }
                else
                {
                    var mat = CreateRotation(ImmutableDictionary<string, string>.Empty, angleGetter, _axis, _center);
                    transformGetter = _ => mat;
                }

                return new RawTransformGeometry(rawGeometry, transformGetter);
            }

            private static Matrix4x4 CreateRotation(
                IReadOnlyDictionary<string, string> data,
                Func<IReadOnlyDictionary<string, string>, double> angleGetter,
                Axis axis,
                Vector3 center
            )
            {
                var angle = angleGetter(data);
                if (angle == 0)
                    return Matrix4x4.Identity;

                var rad = (float)(2 * Math.PI * angle / 360);
                return axis switch
                {
                    Axis.X => Matrix4x4.CreateRotationX(rad, center),
                    Axis.Y => Matrix4x4.CreateRotationY(rad, center),
                    Axis.Z => Matrix4x4.CreateRotationZ(rad, center),
                    _ => throw new InvalidOperationException()
                };
            }
        }

        private sealed class RawTransformGeometry : IRawGeometry
        {
            private readonly IRawGeometry _rawGeometry;
            private readonly Func<IReadOnlyDictionary<string, string>, Matrix4x4> _transformGetter;

            public RawTransformGeometry(
                IRawGeometry rawGeometry,
                Func<IReadOnlyDictionary<string, string>, Matrix4x4> transformGetter
            )
            {
                _rawGeometry = rawGeometry;
                _transformGetter = transformGetter;
            }

            public void LoadTextures(MultiSpriteLoader loader)
            {
                _rawGeometry.LoadTextures(loader);
            }

            public IGeometry Bake()
            {
                var geometry = _rawGeometry.Bake();
                return new TransformGeometry(geometry, _transformGetter);
            }
        }

        private sealed class TransformGeometry : IGeometry
        {
            private readonly IGeometry _geometry;
            private readonly Func<IReadOnlyDictionary<string, string>, Matrix4x4> _transformGetter;

            public TransformGeometry(IGeometry geometry, Func<IReadOnlyDictionary<string, string>, Matrix4x4> transformGetter)
            {
                _geometry = geometry;
                _transformGetter = transformGetter;
            }

            public void Add(IGeometryBuffer buffer, IReadOnlyModelData data, DirectionFlags visibleFaces)
            {
                var jsonData = data.Get<JsonModelData>();
                
                var mat = _transformGetter(jsonData?.Data ?? ImmutableDictionary<string, string>.Empty);

                var prevTransform = buffer.Transform;
                buffer.Transform = mat * prevTransform;

                _geometry.Add(buffer, data, visibleFaces);

                buffer.Transform = prevTransform;
            }
        }
    }
}