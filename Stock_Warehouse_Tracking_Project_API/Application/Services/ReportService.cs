using System.Text;
using Microsoft.EntityFrameworkCore;
using Stock_Warehouse_Tracking_Project_API.Application.DTOs.Report;
using Stock_Warehouse_Tracking_Project_API.Domain.Enums;
using Stock_Warehouse_Tracking_Project_API.Domain.Interfaces;
using Stock_Warehouse_Tracking_Project_API.Infrastructure.Persistence;

namespace Stock_Warehouse_Tracking_Project_API.Application.Services;

public interface IReportService
{
    Task<StockSummaryReportDto> GetStockSummaryAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MovementTrendPointDto>> GetMovementTrendAsync(
        string granularity = "daily",
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken ct = default);
    Task<IReadOnlyList<WarehouseComparisonDto>> GetWarehouseComparisonAsync(CancellationToken ct = default);
    Task<byte[]> ExportMovementsCsvAsync(CancellationToken ct = default);
    Task<EmailReportResultDto> EmailReportAsync(EmailReportRequest request, int? requestingUserId, CancellationToken ct = default);
}

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    private readonly INewStockService _stockService;
    private readonly IProductService _productService;
    private readonly IWarehouseService _warehouseService;
    private readonly IStockThresholdService _thresholdService;
    private readonly IEnumerable<INotificationProvider> _notificationProviders;
    private readonly IOperationLogService _opLog;
    private readonly IConfiguration _configuration;

    public ReportService(
        AppDbContext db,
        INewStockService stockService,
        IProductService productService,
        IWarehouseService warehouseService,
        IStockThresholdService thresholdService,
        IEnumerable<INotificationProvider> notificationProviders,
        IOperationLogService opLog,
        IConfiguration configuration)
    {
        _db = db;
        _stockService = stockService;
        _productService = productService;
        _warehouseService = warehouseService;
        _thresholdService = thresholdService;
        _notificationProviders = notificationProviders;
        _opLog = opLog;
        _configuration = configuration;
    }

    public async Task<StockSummaryReportDto> GetStockSummaryAsync(CancellationToken ct = default)
    {
        var stocks = await _stockService.GetStocksAsync(ct: ct);
        var products = await _productService.GetAllAsync(ct);
        var warehouses = await _warehouseService.GetAllAsync(ct);
        var lowStock = await _thresholdService.GetLowStockCountAsync(ct);

        return new StockSummaryReportDto
        {
            TotalQuantity = stocks.Sum(s => s.Quantity),
            ProductCount = products.Count,
            WarehouseCount = warehouses.Count,
            LowStockCount = lowStock,
            EmptyStockLines = stocks.Count(s => s.Quantity <= 0)
        };
    }

    public async Task<IReadOnlyList<MovementTrendPointDto>> GetMovementTrendAsync(
        string granularity = "daily",
        DateTime? dateFrom = null,
        DateTime? dateTo = null,
        CancellationToken ct = default)
    {
        var to = dateTo?.ToUniversalTime() ?? DateTime.UtcNow;
        var from = dateFrom?.ToUniversalTime()
            ?? (granularity == "weekly" ? to.AddDays(-84) : to.AddDays(-30));

        var movements = await _db.StockMovements
            .AsNoTracking()
            .Where(m => m.Date >= from && m.Date <= to)
            .ToListAsync(ct);

        IEnumerable<IGrouping<DateTime, Domain.Entities.StockMovement>> grouped = granularity == "weekly"
            ? movements.GroupBy(m => StartOfWeek(m.Date))
            : movements.GroupBy(m => m.Date.Date);

        return grouped
            .OrderBy(g => g.Key)
            .Select(g => new MovementTrendPointDto
            {
                Label = granularity == "weekly"
                    ? $"Hafta {g.Key:dd.MM.yyyy}"
                    : g.Key.ToString("dd.MM.yyyy"),
                InCount = g.Count(x => x.Type == MovementType.In),
                OutCount = g.Count(x => x.Type == MovementType.Out),
                TransferCount = g.Count(x => x.Type == MovementType.Transfer)
            })
            .ToList();
    }

    public async Task<IReadOnlyList<WarehouseComparisonDto>> GetWarehouseComparisonAsync(CancellationToken ct = default)
    {
        var stocks = await _stockService.GetStocksAsync(ct: ct);
        var warehouses = await _warehouseService.GetAllAsync(ct);
        var warehouseByCode = warehouses.ToDictionary(w => w.Code, StringComparer.OrdinalIgnoreCase);

        return stocks
            .GroupBy(s => s.WarehouseId)
            .Select(g =>
            {
                warehouseByCode.TryGetValue(g.Key, out var wh);
                return new WarehouseComparisonDto
                {
                    WarehouseCode = g.Key,
                    WarehouseName = wh?.Name ?? g.Key,
                    TotalQuantity = g.Sum(x => x.Quantity),
                    LineCount = g.Count()
                };
            })
            .OrderByDescending(x => x.TotalQuantity)
            .ToList();
    }

    public async Task<byte[]> ExportMovementsCsvAsync(CancellationToken ct = default)
    {
        var movements = await _db.StockMovements
            .AsNoTracking()
            .Include(m => m.Product)
            .Include(m => m.SourceWarehouse)
            .Include(m => m.DestWarehouse)
            .Include(m => m.User)
            .OrderByDescending(m => m.Date)
            .Take(5000)
            .ToListAsync(ct);

        var sb = new StringBuilder();
        sb.AppendLine("Tarih,Islem,Malzeme,Miktar,KaynakDepo,HedefDepo,Kullanici,RefNo");
        foreach (var m in movements)
        {
            sb.AppendLine(string.Join(",",
                m.Date.ToString("O"),
                m.Type,
                Escape(m.Product.Code),
                m.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture),
                Escape(m.SourceWarehouse?.Code),
                Escape(m.DestWarehouse?.Code),
                Escape(m.User?.Name),
                Escape(m.RefNo)));
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<EmailReportResultDto> EmailReportAsync(
        EmailReportRequest request,
        int? requestingUserId,
        CancellationToken ct = default)
    {
        var sendGrid = _notificationProviders.FirstOrDefault(p => p.Name == "SendGrid");
        if (sendGrid is null)
        {
            return new EmailReportResultDto { Sent = false, Message = "SendGrid provider bulunamadı." };
        }

        var to = request.To;
        if (string.IsNullOrWhiteSpace(to) && requestingUserId.HasValue)
        {
            var pref = await _db.UserNotificationPreferences.AsNoTracking()
                .FirstOrDefaultAsync(p => p.UserId == requestingUserId.Value, ct);
            to = pref?.AlertEmail;
        }
        if (string.IsNullOrWhiteSpace(to))
            to = _configuration["Integrations:SendGrid:AlertEmail"];

        if (string.IsNullOrWhiteSpace(to))
        {
            return new EmailReportResultDto
            {
                Sent = false,
                Message = "Alıcı e-posta adresi bulunamadı. Tercihler veya SendGrid AlertEmail ayarlayın."
            };
        }

        var summary = await GetStockSummaryAsync(ct);
        var warehouses = await GetWarehouseComparisonAsync(ct);
        var periodDays = request.PeriodDays <= 0 ? 7 : request.PeriodDays;

        var text = new StringBuilder();
        text.AppendLine("Stok Depo Takip — Dönemsel Rapor");
        text.AppendLine($"Dönem: son {periodDays} gün");
        text.AppendLine($"Toplam stok: {summary.TotalQuantity}");
        text.AppendLine($"Ürün: {summary.ProductCount}, Depo: {summary.WarehouseCount}");
        text.AppendLine($"Kritik stok: {summary.LowStockCount}, Boş satır: {summary.EmptyStockLines}");
        text.AppendLine();
        text.AppendLine("Depo dağılımı (üst 5):");
        foreach (var w in warehouses.Take(5))
            text.AppendLine($"- {w.WarehouseName} ({w.WarehouseCode}): {w.TotalQuantity}");

        var html = $@"
<html><body style='font-family:Segoe UI,Arial,sans-serif;color:#0f172a'>
  <h2>Stok Depo Takip — Dönemsel Rapor</h2>
  <p>Dönem: son <strong>{periodDays}</strong> gün</p>
  <ul>
    <li>Toplam stok: <strong>{summary.TotalQuantity}</strong></li>
    <li>Ürün / Depo: <strong>{summary.ProductCount}</strong> / <strong>{summary.WarehouseCount}</strong></li>
    <li>Kritik stok: <strong>{summary.LowStockCount}</strong></li>
    <li>Boş satır: <strong>{summary.EmptyStockLines}</strong></li>
  </ul>
  <h3>Depo dağılımı</h3>
  <ol>
    {string.Join("", warehouses.Take(5).Select(w => $"<li>{System.Net.WebUtility.HtmlEncode(w.WarehouseName)}: {w.TotalQuantity}</li>"))}
  </ol>
</body></html>";

        byte[]? csv = null;
        if (request.IncludeCsv)
            csv = await ExportMovementsCsvAsync(ct);

        var sent = await sendGrid.SendEmailAsync(new EmailMessage
        {
            To = to,
            Subject = $"Stok Raporu — son {periodDays} gün",
            Body = text.ToString(),
            HtmlBody = html,
            AttachmentBytes = csv,
            AttachmentFileName = csv is null ? null : "hareket-raporu.csv",
            AttachmentContentType = "text/csv"
        }, ct);

        await _opLog.LogAsync(
            requestingUserId,
            "ReportEmailed",
            "Report",
            sent,
            details: $"To={to}, PeriodDays={periodDays}, IncludeCsv={request.IncludeCsv}",
            errorMessage: sent ? null : "Rapor e-postası gönderilemedi.",
            source: EventLogSource.System,
            severity: sent ? EventLogSeverity.Info : EventLogSeverity.Error,
            actorUserId: requestingUserId,
            ct: ct);

        return new EmailReportResultDto
        {
            Sent = sent,
            To = to,
            Message = sent ? "Rapor e-postası gönderildi." : "Rapor e-postası gönderilemedi."
        };
    }

    private static DateTime StartOfWeek(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.Date.AddDays(-diff);
    }

    private static string Escape(string? value)
    {
        var text = value ?? "";
        return text.Contains(',') ? $"\"{text.Replace("\"", "\"\"")}\"" : text;
    }
}
