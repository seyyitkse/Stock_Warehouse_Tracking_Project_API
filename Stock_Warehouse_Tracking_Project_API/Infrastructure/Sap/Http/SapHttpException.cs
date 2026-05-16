namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap.Http;

public sealed class SapHttpException : Exception
{
    public SapHttpException(string message) : base(message)
    {
    }

    public SapHttpException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public int? StatusCode { get; init; }

    public string? ResponseBody { get; init; }
}
