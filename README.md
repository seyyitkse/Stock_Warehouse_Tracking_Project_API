# 📦 Stock Warehouse Tracking Project API

.NET 8 ile geliştirilmiş, **stok ve depo yönetimi** için RESTful bir Web API projesidir. JWT tabanlı kimlik doğrulama, SAP entegrasyonu (Mock / RFC), katmanlı mimari ve kapsamlı loglama altyapısına sahiptir.

---

## 🚀 Özellikler

- **JWT Bearer Authentication** – Güvenli token tabanlı kimlik doğrulama
- **Rol Tabanlı Yetkilendirme** – `Admin`, `WarehouseManager`, `Manager` rolleri
- **SAP Entegrasyonu** – Yapılandırma ile Mock veya gerçek RFC client seçimi
- **Entity Framework Core** – SQL Server üzerinde Code-First yaklaşım
- **Serilog** – Yapılandırılabilir, yapısal loglama
- **FluentValidation** – İstek doğrulama (request validation)
- **AutoMapper** – Entity ↔ DTO dönüşümleri
- **Swagger / OpenAPI** – JWT destekli interaktif API dokümantasyonu
- **Global Exception & Request/Response Middleware** – Merkezi hata yönetimi ve istek loglama
- **Soft Delete** – `BaseEntity` üzerinden silinmeden işaretleme
- **Health Checks** – Uygulama sağlık kontrolü

---

## 🏗️ Proje Mimarisi

```
Stock_Warehouse_Tracking_Project_API/
├── API/
│   ├── Controllers/          # HTTP endpoint'leri
│   └── Middleware/           # Global exception & request/response logging
├── Application/
│   ├── DTOs/                 # Veri transfer nesneleri
│   ├── Mappings/             # AutoMapper profilleri
│   ├── Services/             # Servis arayüzleri ve implementasyonları
│   ├── Validators/           # FluentValidation kuralları
│   └── Common/               # Paylaşılan yardımcı sınıflar (PagedResult vb.)
├── Domain/
│   ├── Entities/             # Veritabanı entity'leri
│   ├── Enums/                # Enum tanımları
│   └── Interfaces/           # Domain arayüzleri
├── Infrastructure/
│   ├── Logging/              # Serilog yapılandırması
│   ├── Persistence/          # AppDbContext ve EF yapılandırmaları
│   └── Sap/                  # SAP client implementasyonları (Mock & RFC)
└── Migrations/               # EF Core migration'ları
```

---

## 🔌 API Endpoint'leri

### 🔐 Auth — `/api/auth`
| Metot | Endpoint | Açıklama | Yetki |
|-------|----------|----------|-------|
| POST | `/api/auth/login` | Kullanıcı girişi, JWT token döner | Herkese açık |
| POST | `/api/auth/register` | Yeni kullanıcı kaydı | Herkese açık |

### 📦 Products — `/api/products`
| Metot | Endpoint | Açıklama | Yetki |
|-------|----------|----------|-------|
| GET | `/api/products` | Tüm ürünleri listele | Giriş yapılmış |
| GET | `/api/products/{id}` | ID'ye göre ürün getir | Giriş yapılmış |
| POST | `/api/products` | Yeni ürün oluştur | Admin, WarehouseManager |
| PUT | `/api/products/{id}` | Ürün güncelle | Admin, WarehouseManager |
| DELETE | `/api/products/{id}` | Ürün sil | Admin |

### 🏭 Warehouses — `/api/warehouses`
| Metot | Endpoint | Açıklama | Yetki |
|-------|----------|----------|-------|
| GET | `/api/warehouses` | Tüm depoları listele | Giriş yapılmış |
| GET | `/api/warehouses/{id}` | ID'ye göre depo getir | Giriş yapılmış |
| POST | `/api/warehouses` | Yeni depo oluştur | Admin |
| PUT | `/api/warehouses/{id}` | Depo güncelle | Admin |
| DELETE | `/api/warehouses/{id}` | Depo sil | Admin |

### 📊 Stocks — `/api/stocks`
| Metot | Endpoint | Açıklama | Yetki |
|-------|----------|----------|-------|
| GET | `/api/stocks` | Stokları listele (matnr, whId filtresi) | Giriş yapılmış |
| GET | `/api/stocks/{matnr}/{whId}` | Stok detayı getir | Giriş yapılmış |
| POST | `/api/stocks/in` | Stok girişi | Admin, WarehouseManager |
| POST | `/api/stocks/out` | Stok çıkışı | Admin, WarehouseManager |
| POST | `/api/stocks/transfer` | Depolar arası transfer | Admin, WarehouseManager |

### 🔄 Movements — `/api/movements`
| Metot | Endpoint | Açıklama | Yetki |
|-------|----------|----------|-------|
| GET | `/api/movements` | Hareketleri filtreli ve sayfalı listele | Giriş yapılmış |

### 📋 Logs — `/api/logs`
| Metot | Endpoint | Açıklama | Yetki |
|-------|----------|----------|-------|
| GET | `/api/logs` | Operasyon loglarını listele | Giriş yapılmış |

---

## ⚙️ Yapılandırma

### `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=StockWarehouseDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "EN_AZ_32_KARAKTER_UZUN_GIZLI_ANAHTAR",
    "Issuer": "StockWarehouseAPI",
    "Audience": "StockWarehouseClient",
    "ExpiresInMinutes": 480
  },
  "SapClient": {
    "UseMock": true
  }
}
```

| Alan | Açıklama |
|------|----------|
| `ConnectionStrings:DefaultConnection` | SQL Server bağlantı dizesi |
| `Jwt:Key` | JWT imzalama anahtarı (min. 32 karakter) |
| `Jwt:ExpiresInMinutes` | Token geçerlilik süresi (dakika) |
| `SapClient:UseMock` | `true` → MockSapClient, `false` → RfcSapClient |

---

## 🛠️ Kurulum & Çalıştırma

### Gereksinimler

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (2019+ önerilir)

### Adımlar

1. **Repoyu klonlayın:**
   ```bash
   git clone https://github.com/seyyitkse/Stock_Warehouse_Tracking_Project_API.git
   cd Stock_Warehouse_Tracking_Project_API
   ```

2. **Bağlantı dizesini güncelleyin:**  
   `appsettings.json` veya `appsettings.Development.json` dosyasındaki `ConnectionStrings:DefaultConnection` değerini kendi SQL Server örneğinize göre ayarlayın.

3. **JWT anahtarını güncelleyin:**  
   `Jwt:Key` alanına en az 32 karakterlik güçlü bir anahtar girin.

4. **Veritabanını oluşturun:**
   ```bash
   dotnet ef database update
   ```

5. **Uygulamayı çalıştırın:**
   ```bash
   dotnet run --project Stock_Warehouse_Tracking_Project_API
   ```

6. **Swagger UI'ya erişin:**  
   `https://localhost:{port}/swagger`

---

## 🔑 Kimlik Doğrulama

1. `/api/auth/login` endpoint'ine kullanıcı adı ve şifre ile `POST` isteği gönderin.
2. Dönen `token` değerini Swagger UI'da **Authorize** butonuna veya istek header'ına ekleyin:
   ```
   Authorization: Bearer <token>
   ```

---

## 👥 Roller

| Rol | Yetki |
|-----|-------|
| `Admin` | Tüm işlemler (CRUD, stok, transfer, silme) |
| `WarehouseManager` | Ürün/stok oluşturma, güncelleme, stok hareketleri |
| `Manager` | Yalnızca okuma işlemleri |

---

## 🧰 Kullanılan Teknolojiler

| Teknoloji | Kullanım Amacı |
|-----------|----------------|
| .NET 8 | Web API framework |
| Entity Framework Core | ORM, Code-First |
| SQL Server | Veritabanı |
| Serilog | Yapısal loglama |
| AutoMapper | DTO dönüşümleri |
| FluentValidation | İstek doğrulama |
| JWT Bearer | Kimlik doğrulama |
| Swagger / Swashbuckle | API dokümantasyonu |

---

## 📄 Lisans

Bu proje [MIT Lisansı](LICENSE) ile lisanslanmıştır.

👨‍💻 Geliştiriciler
Ahmet Seyyit Köse
Nursena Çamkömürü
