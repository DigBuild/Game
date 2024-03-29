﻿using System;
using System.Numerics;
using DigBuild.Engine.Math;
using DigBuild.Engine.Render;
using DigBuild.Engine.Render.Models;
using DigBuild.Render;
using DigBuild.Render.Worlds;

namespace DigBuild.Content.Models.Blocks
{
    public sealed class SpinnyTriangleModel : IBlockModel
    {
        private readonly MultiSprite _sprite;
        
        public SpinnyTriangleModel(MultiSprite sprite)
        {
            _sprite = sprite;
        }

        public void AddGeometry(IGeometryBuffer buffer, IReadOnlyModelData data, DirectionFlags visibleFaces)
        {
        }
        
        public bool HasDynamicGeometry => true;

        public void AddDynamicGeometry(IGeometryBuffer buffers, IReadOnlyModelData data, DirectionFlags visibleFaces, float partialTick)
        {
            var angle = (DateTime.Now.Ticks % TimeSpan.TicksPerSecond) / (double) TimeSpan.TicksPerSecond;
            var matrix = Matrix4x4.CreateRotationY((float) (angle * 2 * Math.PI), Vector3.One / 2);

            var normal = Vector3.TransformNormal(new Vector3(0, 0, 1), matrix);
            
            var v1 = new WorldVertex(new Vector3(0.5f, 1f, 0.5f), normal, _sprite, 0.5f, 1, 1);
            var v2 = new WorldVertex(Vector3.Transform(new Vector3(0.5f, 0, 0), matrix), normal, _sprite, 0, 0, 1);
            var v3 = new WorldVertex(Vector3.Transform(new Vector3(0.5f, 0, 1), matrix), normal, _sprite, 1, 0, 1);

            var buf = buffers.Get(WorldRenderLayers.Opaque);
            buf.Accept(v1, v2, v3);
            buf.Accept(v1, v3, v2);
        }
    }
}