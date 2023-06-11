using System;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LR.Standard;

public abstract class UnixTimestampBaseConverter : JsonConverter<UnixTimestamp>
{
    public override UnixTimestamp Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string date_string = ( reader.TokenType == JsonTokenType.Number ? reader.GetUInt64().ToString() : reader.GetString() )?.ToString() ?? string.Empty;
        if (date_string.Length == 0 || date_string == "False")
        {
            return UnixTimestamp.Epoch;
        }

        if (date_string.StartsWith("timestamp:") && long.TryParse(date_string.Split(':')[1], out long l1))
        {
            return UnixTimestamp.FromUnixTimeSeconds(l1);
        }

        if (date_string.EndsWith(" UTC"))
            date_string = date_string.Replace(" UTC", ""); // we always assume UTC

        if (date_string == "0000-00-00 00:00:00" || date_string == "1970-01-01 00:00:00" || date_string.StartsWith("0000-00-00") || date_string.StartsWith("1970-") ||
            date_string == "null")
        {
            return UnixTimestamp.Epoch;
        }

        if (DateTime.TryParseExact(s: date_string, format: "yyyy-MM-dd HH:mm:ss", provider: CultureInfo.InvariantCulture, style: DateTimeStyles.AssumeUniversal,
                result: out DateTime result))
        {
            result = result.ToUniversalTime();
            //LR.Log.Debug ("TryParseExact: {0} - {1}", result, result.Kind);
            return UnixTimestamp.FromDateTime(result);
        }

        if (DateTime.TryParseExact(s: date_string, format: "yyyy-MM-dd", provider: CultureInfo.InvariantCulture, style: DateTimeStyles.AssumeUniversal, result: out result))
        {
            result = result.ToUniversalTime();
            //LR.Log.Debug ("TryParseExact: {0} - {1}", result, result.Kind);
            return UnixTimestamp.FromDateTime(result);
        }

        if (long.TryParse(date_string, out long l2))
        {
            return UnixTimestamp.FromUnixTimeSeconds(l2);
        }

        return UnixTimestamp.Epoch;
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(UnixTimestamp);
    }
}

public sealed class UnixTimestampWrittenOutConverter : UnixTimestampBaseConverter
{
    public override void Write(Utf8JsonWriter writer, UnixTimestamp value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.IsNotNull() ? ( value.DateTime.ToString("yyyy-MM-dd HH:mm:ss") + " UTC" ) : null);
    }
}

public sealed class UnixTimestampLongConverter : UnixTimestampBaseConverter
{
    public override void Write(Utf8JsonWriter writer, UnixTimestamp value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.IsNotNull() ? value.UnixTimeSeconds : 0);
    }
}
