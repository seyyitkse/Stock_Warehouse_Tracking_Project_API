namespace Stock_Warehouse_Tracking_Project_API.Configuration;

public static class SapClientConfiguration
{
    public const string HttpClientName = "SapHttp";

    public static SapClientProvider GetProvider(IConfiguration configuration)
    {
        var providerValue = configuration["SapClient:Provider"];
        if (!string.IsNullOrWhiteSpace(providerValue) &&
            Enum.TryParse(providerValue, ignoreCase: true, out SapClientProvider provider))
        {
            return provider;
        }

        if (configuration.GetValue<bool>("SapClient:UseMock"))
            return SapClientProvider.Mock;

        return SapClientProvider.Rfc;
    }
}
