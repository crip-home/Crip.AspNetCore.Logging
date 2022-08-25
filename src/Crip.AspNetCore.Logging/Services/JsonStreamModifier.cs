using System;
using System.IO;
using Newtonsoft.Json;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// JSON stream modifier.
/// </summary>
public class JsonStreamModifier : IJsonStreamModifier
{
    /// <inheritdoc />
    public void Modify(
        Stream input,
        Stream output,
        Func<object?, string?>? propertyKeyFactory = null,
        Func<JsonToken, object?, object?>? propertyValueFactory = null)
    {
        var getKey = propertyKeyFactory ?? (k => k?.ToString());
        var getValue = propertyValueFactory ?? ((t, v) => v);

        using var streamReader = new StreamReader(input);
        using var reader = new JsonTextReader(streamReader);
        var streamWriter = new StreamWriter(output);
        var writer = new JsonTextWriter(streamWriter);
        while (reader.Read())
        {
            AddElement(writer, reader.TokenType, reader.Value, getKey, getValue);
        }

        streamWriter.Flush();
        output.Seek(0, SeekOrigin.Begin);
    }

    private void AddElement(
        JsonWriter writer,
        JsonToken tokenType,
        object? value,
        Func<object?, string?> getKey,
        Func<JsonToken, object?, object?> getValue)
    {
        switch (tokenType)
        {
            case JsonToken.StartObject:
                writer.WriteStartObject();
                break;

            case JsonToken.EndObject:
                writer.WriteEndObject();
                break;

            case JsonToken.StartArray:
                writer.WriteStartArray();
                break;

            case JsonToken.EndArray:
                writer.WriteEndArray();
                break;

            default:
                AddToken(writer, tokenType, value, getKey, getValue);
                break;
        }
    }

    private void AddToken(
        JsonWriter writer,
        JsonToken tokenType,
        object? value,
        Func<object?, string?> getKey,
        Func<JsonToken, object?, object?> getValue)
    {
        if (tokenType == JsonToken.PropertyName)
        {
            var key = getKey(value);
            writer.WritePropertyName(key, true);
        }
        else
        {
            var val = getValue(tokenType, value);
            writer.WriteValue(val);
        }
    }
}