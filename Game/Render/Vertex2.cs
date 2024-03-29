﻿using System.Numerics;
using DigBuild.Engine.Render;

namespace DigBuild.Render
{
    /// <summary>
    /// A basic 2D vertex with a position.
    /// </summary>
    public readonly struct Vertex2
    {
        /// <summary>
        /// The position.
        /// </summary>
        public readonly Vector2 Pos;

        public Vertex2(Vector2 pos)
        {
            Pos = pos;
        }

        public Vertex2(float x, float y)
        {
            Pos = new Vector2(x, y);
        }

        public override string ToString()
        {
            return $"Vertex({Pos})";
        }
    }
}