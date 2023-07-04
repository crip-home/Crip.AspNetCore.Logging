using Crip.AspNetCore.Logging.LongJsonContent.Configuration;
using Crip.AspNetCore.Logging.LongJsonContent.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Crip.AspNetCore.Logging.LongJsonContent;

/// <summary>
/// JSON content middleware. Trims property values if it exceeds max value.
/// </summary>
public class LongJsonContentMiddleware : IRequestContentLogMiddleware
{
    private const string ApplicationJson = "application/json";

    private readonly IJsonStreamModifier _jsonModifier;
    private readonly IOptions<LongJsonContentOptions> _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="LongJsonContentMiddleware"/> class.
    /// </summary>
    /// <param name="jsonModifier">JSON content modifier.</param>
    /// <param name="options">Request logging options.</param>
    public LongJsonContentMiddleware(
        IJsonStreamModifier jsonModifier,
        IOptions<LongJsonContentOptions> options)
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
    public uint MaxCharCountInField => _options.Value.MaxCharCountInField;

    /// <summary>
    /// Gets the length of content to leave in field, when trimming.
    /// </summary>
    public uint LeaveOnTrim => _options.Value.LeaveOnTrimCharCountInField;

    /// <inheritdoc/>
    public string ContentType => ApplicationJson;

    /// <inheritdoc/>
    public void Modify(Stream input, Stream output)
    {
        input.Seek(0, SeekOrigin.Begin);
        MemoryStream clone = new();
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

    private static string GetKey(object? key) =>
        key?.ToString() ?? string.Empty;

    /// <summary>
    /// We will trim Bytes|String value the MaxCharCountInField length.
    /// </summary>
    /// <param name="tokenType">Token value type.</param>
    /// <param name="value">The actual value.</param>
    /// <returns>Updated or original value.</returns>
    private object? GetValue(JsonToken tokenType, object? value)
    {
        if (value is null) return null;

        switch (tokenType)
        {
            case JsonToken.Bytes:
            case JsonToken.String:
                var text = value.ToString();
                return text?.Length <= MaxCharCountInField ? text : $"{text?.Substring(0, (int)LeaveOnTrim)}...";

            default:
                return value;
        }
    }
}