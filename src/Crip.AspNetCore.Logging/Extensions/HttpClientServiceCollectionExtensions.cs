using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;

namespace Crip.AspNetCore.Logging;

/// <summary>
/// HTTP client extensions for service collection.
/// </summary>
public static class HttpClientServiceCollectionExtensions
{
    /// <summary>
    /// Add HTTP client to the <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <typeparam name="TClient">
    /// The type of the typed client. They type specified will be registered in the service collection as
    /// a transient service.
    /// </typeparam>
    /// <returns>Updated service collection.</returns>
    public static IHttpClientBuilder AddLoggableHttpClient<TClient>(this IServiceCollection services)
        where TClient : class
    {
        return services
            .AddRequestLoggingHandler()
            .AddHttpClient<TClient>()
            .AddHttpMessageHandler<LoggingHandler<TClient>>();
    }

    /// <summary>
    /// Add HTTP client to the <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <typeparam name="TClient">
    /// The type of the typed client. They type specified will be registered in the service collection as
    /// a transient service.
    /// </typeparam>
    /// <typeparam name="TImplementation">
    /// The implementation type of the typed client. They type specified will be instantiated by the
    /// <see cref="ITypedHttpClientFactory{TClient}"/>.
    /// </typeparam>
    /// <returns>Updated service collection.</returns>
    public static IHttpClientBuilder AddLoggableHttpClient<TClient, TImplementation>(this IServiceCollection services)
        where TClient : class
        where TImplementation : class, TClient
    {
        return services
            .AddRequestLoggingHandler()
            .AddHttpClient<TClient, TImplementation>()
            .AddHttpMessageHandler<LoggingHandler<TImplementation>>();
    }

    /// <summary>
    /// Add HTTP client to the <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <typeparam name="TClient">
    /// The type of the typed client. They type specified will be registered in the service collection as
    /// a transient service.
    /// </typeparam>
    /// <returns>Updated service collection.</returns>
    public static IHttpClientBuilder AddLoggableHttpClient<TClient>(
        this IServiceCollection services,
        Action<HttpClient> configureClient)
        where TClient : class
    {
        return services
            .AddRequestLoggingHandler()
            .AddHttpClient<TClient>(configureClient)
            .AddHttpMessageHandler<LoggingHandler<TClient>>();
    }

    /// <summary>
    /// Add HTTP client to the <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <typeparam name="TClient">
    /// The type of the typed client. They type specified will be registered in the service collection as
    /// a transient service.
    /// </typeparam>
    /// <typeparam name="TImplementation">
    /// The implementation type of the typed client. They type specified will be instantiated by the
    /// <see cref="ITypedHttpClientFactory{TClient}"/>.
    /// </typeparam>
    /// <returns>Updated service collection.</returns>
    public static IHttpClientBuilder AddLoggableHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        Action<HttpClient> configureClient)
        where TClient : class
        where TImplementation : class, TClient
    {
        return services
            .AddRequestLoggingHandler()
            .AddHttpClient<TClient, TImplementation>(configureClient)
            .AddHttpMessageHandler<LoggingHandler<TImplementation>>();
    }

    /// <summary>
    /// Add HTTP client to the <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <typeparam name="TClient">
    /// The type of the typed client. They type specified will be registered in the service collection as
    /// a transient service.
    /// </typeparam>
    /// <returns>Updated service collection.</returns>
    public static IHttpClientBuilder AddLoggableHttpClient<TClient>(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient)
        where TClient : class
    {
        return services
            .AddRequestLoggingHandler()
            .AddHttpClient<TClient>(configureClient)
            .AddHttpMessageHandler<LoggingHandler<TClient>>();
    }

    /// <summary>
    /// Add HTTP client to the <paramref name="services"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configureClient">A delegate that is used to configure an <see cref="HttpClient"/>.</param>
    /// <typeparam name="TClient">
    /// The type of the typed client. They type specified will be registered in the service collection as
    /// a transient service.
    /// </typeparam>
    /// <typeparam name="TImplementation">
    /// The implementation type of the typed client. They type specified will be instantiated by the
    /// <see cref="ITypedHttpClientFactory{TClient}"/>.
    /// </typeparam>
    /// <returns>Updated service collection.</returns>
    public static IHttpClientBuilder AddLoggableHttpClient<TClient, TImplementation>(
        this IServiceCollection services,
        Action<IServiceProvider, HttpClient> configureClient)
        where TClient : class
        where TImplementation : class, TClient
    {
        return services
            .AddRequestLoggingHandler()
            .AddHttpClient<TClient, TImplementation>(configureClient)
            .AddHttpMessageHandler<LoggingHandler<TImplementation>>();
    }
}