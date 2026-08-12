using System;
using Microsoft.Extensions.DependencyInjection;
using Sufficit.Blazor.UI.Services;
using Sufficit.Blazor.UI.Themes;

namespace Sufficit.Blazor.UI;

/// <summary>
/// DI registration entry point for the SUI library.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the SUI theme, snackbar and dialog services. Pass a
    /// configuration callback to supply the consuming application's
    /// <see cref="ISUITheme"/>; without one, <see cref="DefaultSUITheme"/>
    /// (light, blue) is used.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddSufficitUI(opts =&gt; opts.Theme = new IdentitySUITheme());
    /// </code>
    /// </example>
    public static IServiceCollection AddSufficitUI(
        this IServiceCollection services,
        Action<SUIThemeOptions>? configure = null)
    {
        var options = new SUIThemeOptions();
        configure?.Invoke(options);

        var theme = options.Theme ?? DefaultSUITheme.Instance;
        services.AddScoped(_ => theme);
        services.AddScoped<ISUISnackbar, SUISnackbarService>();
        services.AddScoped<ISUIDialogService, SUIDialogService>();
        return services;
    }
}
