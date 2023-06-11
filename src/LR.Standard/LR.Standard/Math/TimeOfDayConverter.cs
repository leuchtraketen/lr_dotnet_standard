using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LR.Standard;

public sealed class TimeOfDayConverter : JsonConverter<TimeOfDay>
{
    public override TimeOfDay Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return TimeOfDay.FromTimeSpan(TimeSpan.FromSeconds(reader.GetInt64()));
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(TimeOfDay);
    }

    public override void Write(Utf8JsonWriter writer, TimeOfDay value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue((int)value.TimeSpan.TotalSeconds);
    }
}

