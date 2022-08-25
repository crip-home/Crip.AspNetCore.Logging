using System;
using System.IO;
using Microsoft.Extensions.Configuration;
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

    /// <summary>
    /// Initializes a new instance of the <see cref="LongJsonContentMiddleware"/> class.
    /// </summary>
    /// <param name="jsonModifier">JSON content modifier.</param>
    /// <param name="configuration">The key/value application configuration properties.</param>
    public LongJsonContentMiddleware(IJsonStreamModifier jsonModifier, IConfiguration? configuration)
    {
        _jsonModifier = jsonModifier;
        if (configuration is not null)
        {
            MaxCharCountInField = configuration.GetValue(MaxCharCountInFieldSectionKey, 500);
            LeaveOnTrim = configuration.GetValue(LeaveOnTrimSectionKey, 10);
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LongJsonContentMiddleware"/> class.
    /// </summary>
    /// <param name="jsonModifier">JSON content modifier.</param>
    /// <param name="maxCharCountInField">The maximum character count in field.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If <paramref name="maxCharCountInField"/> is less than 1.
    /// </exception>
    public LongJsonContentMiddleware(IJsonStreamModifier jsonModifier, int maxCharCountInField)
        : this(jsonModifier, null)
    {
        if (maxCharCountInField < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCharCountInField));
        }

        MaxCharCountInField = maxCharCountInField;
    }

    /// <summary>
    /// Gets the maximum character count in single field.
    /// </summary>
    public int MaxCharCountInField { get; } = 500;

    /// <summary>
    /// Gets or sets the length of content to leave in field, when trimming.
    /// </summary>
    public int LeaveOnTrim { get; set; } = 10;

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
                return val?.Length <= MaxCharCountInField ? val : $"{val?.Substring(0, LeaveOnTrim)}...";

            default:
                return value;
        }
    }
}