using SapNwRfc;

namespace Stock_Warehouse_Tracking_Project_API.Infrastructure.Sap;

/// <summary>RFC table/structure row for ZBK_STOCK (matches DDIC).</summary>
internal sealed class ZbkStockRfcRow
{
    [SapName("MATNR")]
    public string Matnr { get; set; } = string.Empty;

    [SapName("WH_ID")]
    public string WhId { get; set; } = string.Empty;

    [SapName("QUANTITY")]
    public decimal Quantity { get; set; }

    [SapName("UPDATE_AT")]
    public DateTime? UpdateAt { get; set; }
}

internal sealed class ZGetStockListInput
{
    [SapName("IV_MATNR")]
    public string IvMatnr { get; set; } = string.Empty;

    [SapName("IV_WH_ID")]
    public string IvWhId { get; set; } = string.Empty;
}

internal sealed class ZGetStockListOutput
{
    [SapName("ET_STOCK")]
    public ZbkStockRfcRow[]? EtStock { get; set; }
}

internal sealed class ZGetStockDetailInput
{
    [SapName("IV_MATNR")]
    public string IvMatnr { get; set; } = string.Empty;

    [SapName("IV_WH_ID")]
    public string IvWhId { get; set; } = string.Empty;
}

internal sealed class ZGetStockDetailOutput
{
    [SapName("ES_STOCK")]
    public ZbkStockRfcRow? EsStock { get; set; }

    [SapName("EV_FOUND")]
    public bool EvFound { get; set; }
}

internal sealed class ZCreateProductInput
{
    [SapName("IV_MATNR")]
    public string IvMatnr { get; set; } = string.Empty;

    [SapName("IV_NAME")]
    public string IvName { get; set; } = string.Empty;

    [SapName("IV_UNIT")]
    public string IvUnit { get; set; } = string.Empty;

    [SapName("IV_CATEGORY")]
    public string? IvCategory { get; set; }
}

internal sealed class ZCreateProductOutput
{
    [SapName("EV_SUCCESS")]
    public bool EvSuccess { get; set; }

    [SapName("EV_DOC_NO")]
    public string? EvDocNo { get; set; }

    [SapName("EV_ERROR")]
    public string? EvError { get; set; }
}

internal sealed class ZStockMovementInput
{
    [SapName("IV_MATNR")]
    public string IvMatnr { get; set; } = string.Empty;

    [SapName("IV_WH_ID")]
    public string IvWhId { get; set; } = string.Empty;

    [SapName("IV_QUANTITY")]
    public decimal IvQuantity { get; set; }

    [SapName("IV_REF_NO")]
    public string? IvRefNo { get; set; }
}

internal sealed class ZStockMovementOutput
{
    [SapName("EV_SUCCESS")]
    public bool EvSuccess { get; set; }

    [SapName("EV_DOC_NO")]
    public string? EvDocNo { get; set; }

    [SapName("EV_ERROR")]
    public string? EvError { get; set; }
}

internal sealed class ZTransferStockInput
{
    [SapName("IV_MATNR")]
    public string IvMatnr { get; set; } = string.Empty;

    [SapName("IV_SRC_WH")]
    public string IvSrcWh { get; set; } = string.Empty;

    [SapName("IV_DEST_WH")]
    public string IvDestWh { get; set; } = string.Empty;

    [SapName("IV_QUANTITY")]
    public decimal IvQuantity { get; set; }

    [SapName("IV_REF_NO")]
    public string? IvRefNo { get; set; }
}
