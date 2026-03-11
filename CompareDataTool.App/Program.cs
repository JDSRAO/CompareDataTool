using CompareDataTool.Domain.Interfaces;
using CompareDataTool.Domain.Models;
using CompareDataTool.Domain.Services;
using CompareDataTool.Infrastructure.Data.Sqlite;
using CompareDataTool.Infrastructure.DataSources;
using CompareDataTool.Infrastructure.DataSources.Dataverse;
using CompareDataTool.Infrastructure.DataSources.SQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace CompareDataTool.App
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHost();
            await host.StartAsync();
            var logger = host.Services.GetService<ILogger<Program>>();

            try
            {
                var orchestrator = host.Services.GetService<Orchestrator>();
                await orchestrator.RunAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
            }

            logger.LogInformation("The program has completed execution please press Ctrl+C to exit");
            await host.WaitForShutdownAsync();
        }

        private static IHost CreateHost()
        {
            var host = new HostBuilder();

            host.ConfigureAppConfiguration(c => 
            {
                c.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appSettings.json", false, true);

                SetSerilogConfiguration(c.Build());
            });

            host.UseSerilog();

            host.ConfigureServices((context, services) => 
            {
                var config = new AppConfiguration();
                context.Configuration.Bind(config);

                services.AddSingleton<Orchestrator>();
                services.AddSingleton<AppConfiguration>(config);
                services.AddSingleton<IDataSourceRepositoryFactory, DataSourceFactory>();

                services.AddScoped<IAppDataRepository, SqliteAppDataRepository>();
                services.AddScoped<DataverseDataSource>();
                services.AddScoped<SqlDataSource>();
                services.AddScoped<DataCompareService>();
                services.AddScoped<DataCompareService>();
            });

            return host.Build();
        }

        private static void SetSerilogConfiguration(IConfiguration configuration)
        {
            var fileName = "compare-data-";
            var basedir = Path.Combine(Directory.GetCurrentDirectory(), "logs", DateTime.Now.ToString("yyyy-MM-dd"));
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .ReadFrom.Configuration(configuration)
                .WriteTo.Console()
                .WriteTo.File(path: $@"{basedir}\{fileName}-.log",
                  rollingInterval: RollingInterval.Day,
                  restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Verbose,
                  rollOnFileSizeLimit: true)
                .WriteTo.File(path: $@"{basedir}\{fileName}-errors-.log",
                  rollingInterval: RollingInterval.Day,
                  restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
                  rollOnFileSizeLimit: true)
                .WriteTo.File(path: $@"{basedir}\{fileName}-warnings-.log",
                  rollingInterval: RollingInterval.Day,
                  restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning,
                  rollOnFileSizeLimit: true)
                 //.WriteTo.MSSqlServer
                 //(
                 //    connectionString: "Server=IN00478\\SQLEXPRESS;Database=RestaurantDb;Integrated Security=True;TrustServerCertificate=True",
                 //    sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
                 //    {
                 //        AutoCreateSqlTable = true,
                 //        TableName = "Logs"
                 //    }
                 //)
                 .CreateLogger();
        }
    }
}