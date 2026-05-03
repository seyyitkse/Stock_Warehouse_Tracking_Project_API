using MSSqlServerSinkOptions = Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Logging;

public static class SerilogConfiguration
{
    public static WebApplicationBuilder AddSerilogConfiguration(this WebApplicationBuilder builder)
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        var enableMsSqlSink = builder.Configuration.GetValue<bool?>("Serilog:EnableMSSqlSink") ?? false;

        var cfg = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                path: "Logs/app-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");

        if (enableMsSqlSink && !string.IsNullOrWhiteSpace(connectionString))
        {
            cfg = cfg.WriteTo.MSSqlServer(
                connectionString: connectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "SerilogLogs",
                    AutoCreateSqlTable = true
                });
        }

        Log.Logger = cfg.CreateLogger();

        builder.Host.UseSerilog();
        return builder;
    }
}
