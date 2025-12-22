using System.Text.Json;
using System.Text.Json.Serialization;
using UnitsNet;

namespace Generic;

public class AngleJsonConverter : JsonConverter<Angle>
{
    public override Angle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        Angle.FromRevolutions(reader.GetDouble());

    public override void Write(Utf8JsonWriter writer, Angle value, JsonSerializerOptions options) =>
        writer.WriteNumberValue(value.Revolutions);
}