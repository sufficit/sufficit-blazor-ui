using System;
using Microsoft.Extensions.DependencyInjection;
using Sufficit.Blazor.UI.Themes;

namespace Sufficit.Blazor.UI;

/// <summary>
/// DI registration entry point for the SUI library.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SUI theme services. Pass a configuration callback to
    /// supply the consuming application's <see cref="ISuiTheme"/>; without one,
    /// <see cref="DefaultSuiTheme"/> (light, blue) is used.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddSufficitUI(opts =&gt; opts.Theme = new IdentitySuiTheme());
    /// </code>
    /// </example>
    public static IServiceCollection AddSufficitUI(
        this IServiceCollection services,
        Action<SuiThemeOptions>? configure = null)
    {
        var options = new SuiThemeOptions();
        configure?.Invoke(options);

        var theme = options.Theme ?? DefaultSuiTheme.Instance;
        services.AddScoped(_ => theme);
        return services;
    }
}
