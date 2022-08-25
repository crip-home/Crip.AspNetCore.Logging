using System;
using System.IO;
using Newtonsoft.Json;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// JSON stream modifier contract.
/// </summary>
public interface IJsonStreamModifier
{
    /// <summary>
    /// Modify <paramref name="input"/> stream with key/value factories
    /// and write results to the <paramref name="output"/>.
    /// </summary>
    /// <param name="input">Original JSON stream.</param>
    /// <param name="output">Modified JSON stream.</param>
    /// <param name="propertyKeyFactory">Property key factory.</param>
    /// <param name="propertyValueFactory">Property value factory.</param>
    void Modify(
        Stream input,
        Stream output,
        Func<object?, string?>? propertyKeyFactory = null,
        Func<JsonToken, object?, object?>? propertyValueFactory = null);
}