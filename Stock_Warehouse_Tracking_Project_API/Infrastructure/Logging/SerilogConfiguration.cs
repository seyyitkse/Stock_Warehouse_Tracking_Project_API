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

        Log.Logger = new LoggerConfiguration()
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
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.MSSqlServer(
                connectionString: connectionString,
                sinkOptions: new MSSqlServerSinkOptions
                {
                    TableName = "SerilogLogs",
                    AutoCreateSqlTable = true
                })
            .CreateLogger();

        builder.Host.UseSerilog();
        return builder;
    }
}
