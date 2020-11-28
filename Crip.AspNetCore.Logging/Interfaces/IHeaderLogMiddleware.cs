namespace Crip.AspNetCore.Logging
{
    /// <summary>
    /// Logging request header value parser contract.
    /// </summary>
    public interface IHeaderLogMiddleware
    {
        /// <summary>
        /// Allows to modify header value before write it to log file.
        /// </summary>
        /// <param name="key">Request header key.</param>
        /// <param name="value">Request header value concatenated into one comma separated.</param>
        /// <returns>Original or modified header value.</returns>
        string Modify(string key, string value);
    }
}
