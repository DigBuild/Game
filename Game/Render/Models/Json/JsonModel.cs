﻿using System;
using System.Collections.Generic;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;
using DigBuild.Render.Models.Geometry;

namespace DigBuild.Render.Models.Json
{
    public sealed class JsonModel : IBlockModel, IItemModel
    {
        private readonly IEnumerable<(JsonModelRule Rule, IGeometry Geometry)> _geometry;

        public JsonModel(IEnumerable<(JsonModelRule Rule, IGeometry Geometry)> geometry)
        {
            _geometry = geometry;
        }

        public void AddGeometry(IGeometryBuffer buffer, IReadOnlyModelData data, DirectionFlags faces)
        {
            var jsonData = data.Get<JsonModelData>() ?? JsonModelData.Empty;
            foreach (var (rule, geometry) in _geometry)
            {
                if (!rule.Test(jsonData))
                    continue;
                geometry.Add(buffer, data, faces);
                return;
            }
        }

        public bool HasDynamicGeometry => false;

        public void AddDynamicGeometry(IGeometryBuffer buffer, IReadOnlyModelData data, DirectionFlags visibleFaces, float partialTick)
        {
            throw new InvalidOperationException();
        }

        public void AddGeometry(IGeometryBuffer buffer, IReadOnlyModelData data, ItemModelTransform transform, float partialTick)
        {
            buffer.Transform = transform.GetMatrix() * buffer.Transform;

            var jsonData = data.Get<JsonModelData>() ?? JsonModelData.Empty;
            foreach (var (rule, geometry) in _geometry)
            {
                if (!rule.Test(jsonData))
                    continue;
                geometry.Add(buffer, data, DirectionFlags.All);
                return;
            }
        }
    }
}