using System.Collections.Generic;
using System.Numerics;
using DigBuild.Engine.Render;
using DigBuild.Engine.Ui;

namespace DigBuild.Ui
{
    /// <summary>
    /// A basic user interface text renderer.
    /// </summary>
    public sealed class TextRenderer : ITextRenderer
    {
        private readonly IRenderLayer<UiVertex> _layer;
        private readonly Dictionary<char, CharacterInfo> _characters = new();

        public TextRenderer(IRenderLayer<UiVertex> layer)
        {
            _layer = layer;
            for (var c = 'a'; c <= 'z'; c++)
                _characters[c] = new CharacterInfo(c);
            for (var c = 'A'; c <= 'Z'; c++)
                _characters[c] = new CharacterInfo(c);
            for (var c = '0'; c <= '9'; c++)
                _characters[c] = new CharacterInfo(c);
            
            _characters[':'] = new CharacterInfo(':');
            _characters['<'] = new CharacterInfo('<');
            _characters['>'] = new CharacterInfo('>');
            _characters[','] = new CharacterInfo(',');
            _characters['.'] = new CharacterInfo('.');
            _characters['('] = new CharacterInfo('(');
            _characters[')'] = new CharacterInfo(')');
            _characters['-'] = new CharacterInfo('-');
            _characters['+'] = new CharacterInfo('+');
        }

        public uint GetWidth(char c)
        {
            return CharacterInfo.CharWidth;
        }

        public uint DrawLine(IGeometryBuffer buffer, string text, uint scale = 1, bool yellow = false)
        {
            var length = 0u;

            var transform = buffer.Transform;

            foreach (var c in text)
            {
                if (_characters.TryGetValue(c, out var info))
                {
                    buffer.Transform = Matrix4x4.CreateTranslation(length, 0, 0) *
                                        Matrix4x4.CreateScale(scale) *
                                        transform;
                    buffer.Get(_layer).Accept(yellow ? info.VerticesYellow : info.Vertices);
                }

                length += GetWidth(c);
            }

            return length * scale;
        }

        private readonly struct CharacterInfo
        {
            public const uint CharWidth = 5;
            private const uint CharHeight = 8;
            private const uint CharBaseline = 0;
            private const uint TextureSize = 128;
            
            internal readonly UiVertex[] Vertices;
            internal readonly UiVertex[] VerticesYellow;

            public CharacterInfo(char c)
            {
                Vertices = GenerateVertices(c, Vector4.One);
                VerticesYellow = GenerateVertices(c, new Vector4(1, 1, 0, 1));
            }

            private static UiVertex[] GenerateVertices(char c, Vector4 color)
            {
                var (posX, posY) = GetCharacterPosition(c);
                
                var v1 = new UiVertex(
                    new Vector2(0, -CharBaseline),
                    new Vector2(posX * CharWidth, posY * CharHeight) / TextureSize,
                    color
                );
                var v2 = new UiVertex(
                    new Vector2(CharWidth, -CharBaseline),
                    new Vector2((posX + 1) * CharWidth, posY * CharHeight) / TextureSize,
                    color
                );
                var v3 = new UiVertex(
                    new Vector2(CharWidth, -CharBaseline + CharHeight),
                    new Vector2((posX + 1) * CharWidth, (posY + 1) * CharHeight) / TextureSize,
                    color
                );
                var v4 = new UiVertex(
                    new Vector2(0, -CharBaseline + CharHeight),
                    new Vector2(posX * CharWidth, (posY + 1) * CharHeight) / TextureSize,
                    color
                );

                return new[] { v1, v2, v3, v3, v4, v1 };
            }

            private static (uint, uint) GetCharacterPosition(char c)
            {
                if (c is >= 'a' and <= 'z')
                    c = (char) (c - 'a' + 'A');

                if (c is >= 'A' and <= 'Z')
                    return ((uint) ((c - 'A') % 25), (uint) ((c - 'A') / 25));

                if (c is >= '0' and <= '9')
                    return ((uint) (c - '0' + 1), 1u);
                
                if (c == ':') return (11u, 1u);
                if (c == '<') return (12u, 1u);
                if (c == '>') return (13u, 1u);
                if (c == ',') return (14u, 1u);
                if (c == '.') return (15u, 1u);
                if (c == '(') return (16u, 1u);
                if (c == ')') return (17u, 1u);
                if (c == '-') return (18u, 1u);
                if (c == '+') return (19u, 1u);

                return (0u, 3u);
            }
        }
    }

    public readonly struct TextVertex
    {
        public readonly Vector2 Position;
        public readonly Vector2 Uv;

        public TextVertex(Vector2 position, Vector2 uv)
        {
            Position = position;
            Uv = uv;
        }

        public static VertexTransformer<TextVertex> CreateTransformer(IVertexConsumer<TextVertex> next, Matrix4x4 transform)
        {
            return new(next, v => new TextVertex(
                Vector2.Transform(v.Position, transform),
                v.Uv
            ));
        }
    }
}