namespace Crip.AspNetCore.Logging.LongJsonContent.Configuration;

/// <summary>
/// Long JSON content middleware options.
/// </summary>
public record LongJsonContentOptions
{
    /// <summary>
    /// Gets or sets the maximum allowed characters in a field.
    /// </summary>
    public uint MaxCharCountInField { get; set; } = 500;

    /// <summary>
    /// Gets or sets character count to leave in a field after it is trimmed.
    /// </summary>
    public uint LeaveOnTrimCharCountInField { get; set; } = 10;
}