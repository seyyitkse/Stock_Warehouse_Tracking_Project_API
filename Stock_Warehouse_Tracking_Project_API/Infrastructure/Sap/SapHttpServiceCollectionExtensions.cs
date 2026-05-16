using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Options;
using Stock_Warehouse_Tracking_Project_API.Configuration;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap;

public static class SapHttpServiceCollectionExtensions
{
    public static IServiceCollection AddSapHttpClient(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SapHttpOptions>(configuration.GetSection(SapHttpOptions.SectionName));

        services.AddHttpClient<HttpSapClient>(SapClientConfiguration.HttpClientName, (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<SapHttpOptions>>().Value;

            if (string.IsNullOrWhiteSpace(options.BaseUrl))
                throw new InvalidOperationException("SapHttp:BaseUrl is required when SapClient:Provider is Http.");

            client.BaseAddress = new Uri(options.BaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(Math.Max(5, options.TimeoutSeconds));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (!string.IsNullOrWhiteSpace(options.Username))
            {
                var credentials = Convert.ToBase64String(
                    Encoding.ASCII.GetBytes($"{options.Username}:{options.Password}"));
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", credentials);
            }

            if (!string.IsNullOrWhiteSpace(options.Client))
                client.DefaultRequestHeaders.TryAddWithoutValidation("sap-client", options.Client);

            if (!string.IsNullOrWhiteSpace(options.Language))
                client.DefaultRequestHeaders.TryAddWithoutValidation("sap-language", options.Language);
        });

        services.AddScoped<ISapClient>(sp => sp.GetRequiredService<HttpSapClient>());

        return services;
    }
}
