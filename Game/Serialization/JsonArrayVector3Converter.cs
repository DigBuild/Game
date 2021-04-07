using System;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DigBuild.Serialization
{
    public class JsonArrayVector3Converter : JsonConverter<Vector3>
    {
        public override Vector3 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException();
            
            reader.Read();
            var x = (float) reader.GetDouble();
            reader.Read();
            var y = (float) reader.GetDouble();
            reader.Read();
            var z = (float) reader.GetDouble();
            
            reader.Read();
            if (reader.TokenType != JsonTokenType.EndArray)
                throw new JsonException();
            
            return new Vector3(x, y, z);
        }

        public override void Write(Utf8JsonWriter writer, Vector3 value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();
            writer.WriteNumberValue(value.X);
            writer.WriteNumberValue(value.Y);
            writer.WriteNumberValue(value.Z);
            writer.WriteEndArray();
        }
    }
}