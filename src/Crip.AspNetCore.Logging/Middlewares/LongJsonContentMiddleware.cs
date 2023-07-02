using System;
using System.IO;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// JSON content middleware. Trims property values if it exceeds max value.
/// </summary>
public class LongJsonContentMiddleware : IRequestContentLogMiddleware
{
    private const string MaxCharCountInFieldSectionKey = "Logging:Request:MaxCharCountInField";
    private const string LeaveOnTrimSectionKey = "Logging:Request:LeaveOnTrimCharCountInField";
    private readonly IJsonStreamModifier _jsonModifier;
    private readonly IOptions<RequestLoggingOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="LongJsonContentMiddleware"/> class.
    /// </summary>
    /// <param name="jsonModifier">JSON content modifier.</param>
    /// <param name="options">Request logging options.</param>
    public LongJsonContentMiddleware(
        IJsonStreamModifier jsonModifier,
        IOptions<RequestLoggingOptions> options)
    {
        _jsonModifier = jsonModifier;
        _options = options;

        if (MaxCharCountInField <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(LongJsonContentOptions.MaxCharCountInField));
        }
    }

    /// <summary>
    /// Gets the maximum character count in single field.
    /// </summary>
    public uint MaxCharCountInField => _options.Value.LongJsonContent.MaxCharCountInField;

    /// <summary>
    /// Gets the length of content to leave in field, when trimming.
    /// </summary>
    public uint LeaveOnTrim => _options.Value.LongJsonContent.LeaveOnTrimCharCountInField;

    /// <inheritdoc/>
    public string ContentType => "application/json";

    /// <inheritdoc/>
    public void Modify(Stream input, Stream output)
    {
        input.Seek(0, SeekOrigin.Begin);
        var clone = new MemoryStream();
        input.CopyTo(clone);

        try
        {
            input.Seek(0, SeekOrigin.Begin);
            _jsonModifier.Modify(input, output, GetKey, GetValue);
        }
        catch (Exception)
        {
            // Ignore modifications if we could not read it.
            clone.Seek(0, SeekOrigin.Begin);
            output.Seek(0, SeekOrigin.Begin);

            clone.CopyTo(output);
            output.Seek(0, SeekOrigin.Begin);
        }
    }

    private string? GetKey(object? key) =>
        key?.ToString();

    /// <summary>
    /// We will trim Bytes|String value the MaxCharCountInField length.
    /// </summary>
    /// <param name="tokenType">Token value type.</param>
    /// <param name="value">The actual value.</param>
    /// <returns>Updated or original value.</returns>
    private object? GetValue(JsonToken tokenType, object? value)
    {
        if (value is null)
        {
            return null;
        }

        switch (tokenType)
        {
            case JsonToken.Bytes:
            case JsonToken.String:
                var val = value.ToString();
                return val?.Length <= MaxCharCountInField ? val : $"{val?.Substring(0, (int)LeaveOnTrim)}...";

            default:
                return value;
        }
    }
}