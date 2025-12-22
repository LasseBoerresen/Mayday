using System.Text.Json;
using System.Text.Json.Serialization;
using UnitsNet;

namespace Generic;

public class LengthJsonConverter : JsonConverter<Length>
{
    public override Length Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Length.FromMeters(reader.GetDouble());

    public override void Write(Utf8JsonWriter writer, Length value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Meters);
}