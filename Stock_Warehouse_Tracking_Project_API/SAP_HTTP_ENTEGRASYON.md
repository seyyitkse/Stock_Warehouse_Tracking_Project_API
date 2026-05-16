# SAP HTTP (ICF) entegrasyonu

ASP.NET API, SAP NetWeaver üzerindeki ICF HTTP servislerine **HttpClient** ile bağlanır. `sapnwrfc.dll` ve SapNwRfc yalnızca `SapClient:Provider=Rfc` modunda gerekir.

## Mimari

```
React → ASP.NET API (JWT) → HttpSapClient → SAP ICF (/sap/bc/zstock/...) → ZBK_* tablolar
```

## Provider seçimi (`appsettings.json`)

| `SapClient:Provider` | Açıklama |
|---------------------|----------|
| `Mock` | Bellek içi test verisi (`MockSapClient`) |
| `Http` | SAP ICF HTTP servisleri (`HttpSapClient`) |
| `Rfc` | SAP NW RFC SDK + Function Module (`RfcSapClient`) |

Geriye dönük uyumluluk: `SapClient:UseMock=true` ise provider otomatik `Mock` olur.

HTTP moduna geçmek için:

```json
"SapClient": {
  "Provider": "Http",
  "UseMock": false
}
```

## `SapHttp` ayarları

| Alan | Açıklama |
|------|----------|
| `BaseUrl` | Örn. `http://sap-host:8000` (sonunda `/` yok) |
| `Username` / `Password` | SAP Basic Auth |
| `Client` | Mandant → header `sap-client` |
| `Language` | Header `sap-language` |
| `TimeoutSeconds` | HttpClient zaman aşımı |
| `StockListPath` | Varsayılan: `sap/bc/zstock/stock` |
| `StockDetailPath` | `sap/bc/zstock/stock/{matnr}/{whId}` |
| `StockInPath` | `sap/bc/zstock/stock/in` |
| `StockOutPath` | `sap/bc/zstock/stock/out` |
| `TransferPath` | `sap/bc/zstock/stock/transfer` |
| `ProductsPath` | `sap/bc/zstock/products` |

Production’da şifreyi **User Secrets** veya ortam değişkeni ile verin: `SapHttp__Password`.

## SAP tarafı checklist

1. **SICF**: `/default_host/sap/bc/zstock/...` servisleri **Active**.
2. Handler class (ör. `ZCL_STOCK_HTTP_HANDLER`) atanmış.
3. ICM HTTP portu açık (genelde `8000`).
4. Teknik kullanıcının `ZBK_STOCK` / ilgili FM yetkileri var.

## Endpoint ↔ backend eşlemesi

| HTTP | `ISapClient` metodu |
|------|---------------------|
| `GET .../stock?matnr=&whId=` | `GetStockListAsync` |
| `GET .../stock/{matnr}/{whId}` | `GetStockDetailAsync` |
| `POST .../products` | `CreateProductAsync` |
| `POST .../stock/in` | `StockInAsync` |
| `POST .../stock/out` | `StockOutAsync` |
| `POST .../stock/transfer` | `TransferStockAsync` |

### JSON sözleşmesi (stok satırı)

```json
[
  {
    "matnr": "M001",
    "whId": "D001",
    "quantity": 50,
    "updatedAt": "2026-05-16"
  }
]
```

### Hareket yanıtı (POST)

```json
{
  "success": true,
  "sapDocNo": "DOC123",
  "errorMessage": null
}
```

## SAP’yi doğrudan test (PowerShell)

```powershell
$user = "DEVELOPER"
$pass = "YOUR_PASSWORD"
$pair = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${user}:${pass}"))
Invoke-WebRequest -Uri "http://127.0.0.1:8000/sap/bc/zstock/stock" `
  -Headers @{ Authorization = "Basic $pair"; "sap-client" = "001" } `
  -UseBasicParsing
```

Beklenen: HTTP 200, `Content-Type: application/json`.

## ASP.NET health check

- `GET /health/sap` — provider’a göre mock / HTTP GET stock / RFC ping.

## Geliştirme notları

- Controller ve servis katmanı değişmez; yalnızca `ISapClient` implementasyonu değişir.
- React **doğrudan SAP’ye bağlanmamalı**; JWT ile `api/stocks` kullanılır.
- RFC detayları: [`SAP_RFC_ENTEGRASYON_DOKUMANI.md`](SAP_RFC_ENTEGRASYON_DOKUMANI.md).
