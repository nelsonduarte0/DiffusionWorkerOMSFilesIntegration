using DiffusionWorkerOMSFIlesIntegration;
using DiffusionWorkerOMSFIlesIntegration.Application.Configuration;
using DiffusionWorkerOMSFIlesIntegration.Application.Services.Interfaces;
using DiffusionWorkerOMSFIlesIntegration.Infrastructure;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostCtx, services) =>
    {
        IConfiguration configuration = hostCtx.Configuration;
        var applicationSettings = configuration.GetSection(nameof(ApplicationSettings)).Get<ApplicationSettings>();
        services.AddSingleton<IApplicationSettings>(applicationSettings);
        services.AddHostedService<Worker>();
        services.AddSingleton<IProcessHandler<string>, OMSFilesIntegration>();
    })
    .ConfigureAppConfiguration((hostCtx, builder) =>
    {
        var environmentName = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");
        var path = Environment.CurrentDirectory;
        builder.SetBasePath(Environment.CurrentDirectory)
               .AddJsonFile($"appsettings.CrossReferences.json", optional: true, reloadOnChange: true)
               .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
               .AddJsonFile($"appsettings.{environmentName ?? "production"}.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables();

        if (true)
        {
            builder.AddUserSecrets<Program>();
        }
        //else
        //{
        //    var keyVaultEndpoint = Environment.GetEnvironmentVariable("KEYVAULT_ENDPOINT");

        //    if (string.IsNullOrEmpty(keyVaultEndpoint))
        //        throw new InvalidOperationException("Key vault not configured");


        //    var keyVaultClient = new SecretClient(
        //                new Uri(keyVaultEndpoint),
        //                new DefaultAzureCredential()
        //            );

        //}
    })
    //.UseSerilog((hostingContext, services, loggerConfiguration) => loggerConfiguration
    //                    .ReadFrom.Configuration(hostingContext.Configuration.GetSection("Logging"))
    //                    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning))
    .Build();
await host.RunAsync();
