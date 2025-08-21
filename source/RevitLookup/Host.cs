using System.IO;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RevitLookup.Abstractions.Services.Appearance;
using RevitLookup.Abstractions.Services.Application;
using RevitLookup.Abstractions.Services.Decomposition;
using RevitLookup.Abstractions.Services.Presentation;
using RevitLookup.Abstractions.Services.Settings;
using RevitLookup.Config.Http;
using RevitLookup.Config.Logging;
using RevitLookup.Config.Options;
using RevitLookup.Services;
using RevitLookup.Services.Appearance;
using RevitLookup.Services.Application;
using RevitLookup.Services.Decomposition;
using RevitLookup.Services.Settings;
using RevitLookup.Services.Export;
using RevitLookup.UI.Framework.Services;
using RevitLookup.UI.Framework.Services.Presentation;
using Wpf.Ui;
using Wpf.Ui.Abstractions;
using SoftwareUpdateService = RevitLookup.Services.Settings.SoftwareUpdateService;

namespace RevitLookup;

/// <summary>
///     Provides a host for the application's services and manages their lifetimes
/// </summary>
public static class Host
{
    private static IHost? _host;

    /// <summary>
    ///     Starts the host and configures the application's services
    /// </summary>
    public static void Start()
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            ContentRootPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            DisableDefaults = true,
#if RELEASE
            EnvironmentName = Environments.Development
#else
            EnvironmentName = Environments.Development
#endif
        });

        //Logging
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilogConfiguration();

        //Configuration
        builder.Services.AddApplicationOptions();
        builder.Services.AddResourceLocationsOptions();
        builder.Services.AddSerializerOptions();

        //Frontend services
        builder.Services.AddScoped<INavigationViewPageProvider, DependencyInjectionNavigationViewPageProvider>();
        builder.Services.AddScoped<INavigationService, NavigationService>();
        builder.Services.AddScoped<IContentDialogService, ContentDialogService>();
        builder.Services.AddScoped<ISnackbarService, SnackbarService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<IWindowIntercomService, WindowIntercomService>();

        //MVVM services
        builder.Services.RegisterViews();
        builder.Services.RegisterViewModels();

        //Application services
        builder.Services.AddHttpApiClients();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<ISoftwareUpdateService, SoftwareUpdateService>();
        builder.Services.AddSingleton<IThemeWatcherService, ThemeWatcherService>();
        builder.Services.AddSingleton<RevitRibbonService>();
        builder.Services.AddHostedService<HostBackgroundService>();
        builder.Services.AddScoped<RevitLookup.Abstractions.Services.Export.ICsvExportService, CsvExportService>();

        //Composer services
        builder.Services.AddScoped<IDecompositionService, DecompositionService>();
        builder.Services.AddScoped<IVisualDecompositionService, VisualDecompositionService>();
        builder.Services.AddScoped<IDecompositionSearchService, DecompositionSearchService>();
        builder.Services.AddTransient<IUiOrchestratorService, UiOrchestratorService>();

        //Revit tools services
        builder.Services.AddTransient<EventsMonitoringService>();

        _host = builder.Build();
        _host.Start();
    }

    /// <summary>
    ///     Stops the host and handle <see cref="IHostedService"/> services
    /// </summary>
    public static void Stop()
    {
        _host!.StopAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Get service of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The type of service object to get</typeparam>
    /// <exception cref="System.InvalidOperationException">There is no service of type <typeparamref name="T"/></exception>
    public static T GetService<T>() where T : class
    {
        return _host!.Services.GetRequiredService<T>();
    }
}