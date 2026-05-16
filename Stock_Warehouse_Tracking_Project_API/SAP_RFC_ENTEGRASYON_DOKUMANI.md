# SAP (ABAP) Tarafı — Stok ve Depo Takip Entegrasyonu Dokümanı

Bu belge, **Stock Warehouse Tracking** projesinin SAP katmanını özetler. Amaç: harici sistemlerin (ör. ASP.NET Core API) **RFC** üzerinden güvenli şekilde stok ve malzeme verilerine erişmesi ve güncellemesi.

---

## 1. Genel mimari

```
[Frontend] → [Backend REST API] → [SAP NetWeaver RFC]
                                      ↓
                            ABAP Function Modules (RFC)
                                      ↓
                         Z tablolar (ZBK_STOCK, ZBK_MATERIALS, ZBK_WAREHOUSES)
```

- SAP tarafında **doğrudan tabloya dış erişim** hedeflenmez; iş kuralları **Function Module** içinde tutulur.
- Backend, SAP .NET tarafında **SapNwRfc** kütüphanesi ile bu FM’leri çağırır (native **SAP NW RFC SDK** DLL’leri gerekir).

**Alternatif (önerilen geliştirme ortamı):** `SapClient:Provider=Http` ile ICF HTTP servisleri — native RFC SDK gerekmez. Ayrıntılar: [`SAP_HTTP_ENTEGRASYON.md`](SAP_HTTP_ENTEGRASYON.md).

---

## 1.1. Backend (ASP.NET) ön koşullar — `sapnwrfc` DLL hatası

Eğer backend çalışırken şu hatayı görürsen:

- `System.DllNotFoundException: Unable to load DLL 'sapnwrfc' ...`

bu, **SAP NW RFC SDK native kütüphanelerinin** uygulamanın çalıştığı yerde bulunmadığı (veya mimari/bağımlılık uyumsuzluğu) anlamına gelir.

Yapılacaklar (Windows):

1. SAP NetWeaver RFC SDK’yı indir (SAP resmi dağıtımı). İçinden en az şu dosyalar gerekir:
   - `sapnwrfc.dll`
   - `icudt*.dll`, `icuin*.dll`, `icuuc*.dll` (SDK sürümüne göre isimler değişebilir)
2. Bu DLL’leri backend’in çalıştığı klasöre kopyala:
   - lokal çalıştırma: `bin\Debug\net10.0\` (veya `bin\Release\net10.0\`)
   - publish sonrası: publish çıktısı klasörü
3. Uygulamanın **x64** çalıştığından emin ol:
   - Bu projede `Stock_Warehouse_Tracking_Project_API.csproj` içinde `PlatformTarget=x64` ayarlı.
4. Gerekirse **Microsoft Visual C++ Redistributable 2015-2022 (x64)** kur.

Notlar:

- DLL’leri PATH’e eklemek de mümkündür; ama en sorunsuz yöntem, DLL’leri doğrudan uygulama klasöründe bulundurmaktır.
- `SapClient:UseMock=true` iken SAP native bağımlılığı zorunlu değildir (healthcheck bunu bilerek atlar).

---

## 2. Paket ve nesne yapısı (abapGit)

Kaynak kodlar GitHub’da **abapGit** formatında tutulur (`Stock_Warehouse_Tracking_Project_SAP` deposu).

- **Function group**: `ZFG_STOCK_API`  
  Dosya kökü: `src/zfg_stockapi/`  
  - `zfg_stock_api.fugr.xml` — FM arayüz tanımları (RFC, import/export/tables)
  - `zfg_stock_api.fugr.<fm_adı>.abap` — her FM için ABAP kaynağı

- **Z tablolar** (örnek yollar):
  - `ZBK_STOCK` → `src/zbk_tblstock/zbk_stock.tabl.xml`
  - `ZBK_WAREHOUSES` → `src/zbk_tblwarehouses/zbk_warehouses.tabl.xml`
  - `ZBK_MATERIALS` → `src/tbl_materials/zbk_materials.tabl.xml`

Eski **report** programları (`ZADD_*`, `ZLIST_STOCK`) hâlâ repoda bulunabilir; bunlar RFC ile çağrılmaz. Üretim entegrasyonu için **RFC-enabled FM** kullanılır.

---

## 3. Veri modeli (Z tablolar)

### 3.1. `ZBK_STOCK` — Depo bazlı stok

| Alan        | Tip (DDIC) | Açıklama        |
|------------|------------|-----------------|
| `MATNR`    | CHAR(10)   | Malzeme kodu (PK) |
| `WH_ID`    | CHAR(5)    | Depo kodu (PK)  |
| `QUANTITY` | DEC(13,2)  | Miktar          |
| `UPDATE_AT`| DATS       | Güncelleme tarihi |

Birleşik anahtar: `(MATNR, WH_ID)`.

### 3.2. `ZBK_MATERIALS` — Malzeme ana verisi

| Alan        | Tip      | Açıklama     |
|------------|----------|--------------|
| `MATNR`    | CHAR(10) | Malzeme kodu (PK) |
| `UNIT`     | CHAR(5)  | Birim        |
| `CREATED_AT` | DATS   | Oluşturulma  |
| `MATNAME`  | CHAR(50) | Malzeme adı  |

**Not:** Tabloda “kategori” alanı yoktur. API tarafı `IV_CATEGORY` gönderebilir; FM tarafında **kayıt edilmez** (uyumluluk için opsiyonel parametre).

### 3.3. `ZBK_WAREHOUSES` — Depo master

| Alan         | Tip      | Açıklama   |
|-------------|----------|------------|
| `WH_ID`     | CHAR(5)  | Depo kodu (PK) |
| `WH_NAME`   | CHAR(50) | Depo adı   |
| `LOCATION`  | CHAR(50) | Lokasyon   |
| `CREATED_AT`| CHAR(8)  | Oluşturulma (DDIC’de CHAR; raporlarda `sy-datum` atanabilir) |

---

## 4. RFC Function Module’ler (`ZFG_STOCK_API`)

Tüm aşağıdaki FM’ler **Remote-enabled** olmalıdır (`REMOTE_CALL = R`). Dış sistemler bunları **RFC** ile çağırır.

### 4.1. `Z_GET_STOCK_LIST`

**Amaç:** Stok listesini döndürür; isteğe bağlı filtre uygular.

| Yön        | Parametre   | Tip                 | Zorunlu |
|-----------|-------------|---------------------|---------|
| IMPORTING | `IV_MATNR`  | `ZBK_STOCK-MATNR`   | Hayır (boş = filtre yok) |
| IMPORTING | `IV_WH_ID`  | `ZBK_STOCK-WH_ID`   | Hayır |
| TABLES    | `ET_STOCK`  | `ZBK_STOCK` (satır tablosu) | Evet |

**Davranış:** `ZBK_STOCK` tamamını okur; `IV_MATNR` / `IV_WH_ID` doluysa ilgili alanlara göre filtreler ve `ET_STOCK`’a yazar.

---

### 4.2. `Z_GET_STOCK_DETAIL`

**Amaç:** Tek `(MATNR, WH_ID)` için stok satırı.

| Yön        | Parametre   | Tip               |
|-----------|-------------|-------------------|
| IMPORTING | `IV_MATNR`  | `ZBK_STOCK-MATNR` |
| IMPORTING | `IV_WH_ID`  | `ZBK_STOCK-WH_ID` |
| EXPORTING | `ES_STOCK`  | `ZBK_STOCK`       |
| EXPORTING | `EV_FOUND`  | `BOOLE_D`         |

**Davranış:** `SELECT SINGLE`; bulunduysa `EV_FOUND = abap_true`, aksi halde `abap_false`.

---

### 4.3. `Z_GET_WAREHOUSE_LIST`

**Amaç:** Tüm depoları listeler.

| Yön     | Parametre       | Tip            |
|--------|-----------------|----------------|
| TABLES | `ET_WAREHOUSES` | `ZBK_WAREHOUSES` |

**Davranış:** `SELECT * FROM zbk_warehouses INTO TABLE et_warehouses.`

---

### 4.4. `Z_CREATE_PRODUCT`

**Amaç:** `ZBK_MATERIALS` tablosuna yeni malzeme ekler.

| Yön        | Parametre     | Tip                    | Not |
|-----------|---------------|------------------------|-----|
| IMPORTING | `IV_MATNR`    | `ZBK_MATERIALS-MATNR`  |     |
| IMPORTING | `IV_NAME`     | `ZBK_MATERIALS-MATNAME`|     |
| IMPORTING | `IV_UNIT`     | `ZBK_MATERIALS-UNIT`   |     |
| IMPORTING | `IV_CATEGORY` | `CHAR40` (opsiyonel)   | Tabloda yok; yok sayılır |
| EXPORTING | `EV_SUCCESS`  | `BOOLE_D`              |     |
| EXPORTING | `EV_DOC_NO`   | `CHAR20`               | Başarıda genelde `MATNR` |
| EXPORTING | `EV_ERROR`    | `CHAR255`              | Hata metni |

**Kontroller:** Boş kod/ad/birim; duplicate `MATNR`. Başarılı insert sonrası `COMMIT WORK AND WAIT`.

---

### 4.5. `Z_STOCK_IN`

**Amaç:** Stok girişi (mevcut satır varsa artırır, yoksa oluşturur).

| Yön        | Parametre    | Tip                 | Not |
|-----------|--------------|---------------------|-----|
| IMPORTING | `IV_MATNR`   | `ZBK_STOCK-MATNR`   |     |
| IMPORTING | `IV_WH_ID`   | `ZBK_STOCK-WH_ID`   |     |
| IMPORTING | `IV_QUANTITY`| `ZBK_STOCK-QUANTITY`|     |
| IMPORTING | `IV_REF_NO`  | `CHAR20` (opsiyonel)|     |
| EXPORTING | `EV_SUCCESS` | `BOOLE_D`         |     |
| EXPORTING | `EV_DOC_NO`  | `CHAR20`            | Ref veya üretilen referans |
| EXPORTING | `EV_ERROR`   | `CHAR255`           |     |

**Davranış:** `UPDATE` veya `INSERT`; `UPDATE_AT = sy-datum`; `COMMIT WORK AND WAIT`.

---

### 4.6. `Z_STOCK_OUT`

**Amaç:** Stok çıkışı; yetersiz stokta hata.

Parametre seti `Z_STOCK_IN` ile aynı (`IV_*`, `EV_*`).

**Davranış:** Kayıt yoksa veya `quantity < IV_QUANTITY` ise `EV_SUCCESS = abap_false`, `EV_ERROR` mesajı. Aksi halde azaltır ve commit.

---

### 4.7. `Z_TRANSFER_STOCK`

**Amaç:** Kaynak depodan hedef depoya transfer.

| Yön        | Parametre     | Tip                 |
|-----------|-----------------|---------------------|
| IMPORTING | `IV_MATNR`      | `ZBK_STOCK-MATNR`   |
| IMPORTING | `IV_SRC_WH`     | `ZBK_STOCK-WH_ID`   |
| IMPORTING | `IV_DEST_WH`    | `ZBK_STOCK-WH_ID`   |
| IMPORTING | `IV_QUANTITY`   | `ZBK_STOCK-QUANTITY`|
| IMPORTING | `IV_REF_NO`     | `CHAR20` (opsiyonel)|
| EXPORTING | `EV_SUCCESS`    | `BOOLE_D`           |
| EXPORTING | `EV_DOC_NO`     | `CHAR20`            |
| EXPORTING | `EV_ERROR`      | `CHAR255`           |

**Davranış:** Kaynakta yeterli miktar yoksa hata; kaynak azaltılır, hedef artırılır veya insert; `COMMIT WORK AND WAIT`. Kaynak = hedef ise hata.

---

## 5. Backend ile eşleme (özet)

Backend `ISapClient` arayüzü şu SAP FM’lere map edilir:

| Backend metodu        | SAP FM               |
|----------------------|----------------------|
| `GetStockListAsync`  | `Z_GET_STOCK_LIST`   |
| `GetStockDetailAsync`| `Z_GET_STOCK_DETAIL` |
| `CreateProductAsync` | `Z_CREATE_PRODUCT`   |
| `StockInAsync`       | `Z_STOCK_IN`         |
| `StockOutAsync`      | `Z_STOCK_OUT`        |
| `TransferStockAsync` | `Z_TRANSFER_STOCK`   |

`Z_GET_WAREHOUSE_LIST` şu an **backend `WarehouseService` içinde doğrudan kullanılmıyor** (depolar MSSQL üzerinden); ileride SAP master’a taşınmak istenirse bu FM bağlanabilir.

---

## 6. SAP sisteminde yapılacak işler (checklist)

1. abapGit ile repoyu çek veya nesneleri **SE80 / SE37** üzerinden içe aktar.
2. Tüm FM’leri **aktive et** (Function Group + include’lar).
3. **SE37** ile her FM için:
   - Başarılı senaryo
   - Hata senaryosu (ör. duplicate malzeme, yetersiz stok)
4. **SM59 gerekmez** (bu tasarım uygulama sunucusuna doğrudan RFC; ters yön yok).
5. RFC kullanıcısı için yetki: ilgili FM’lerin **yetki nesnesi** (varsa) ve tablo erişimi.

---

## 7. Test önerileri (SE37)

- **`Z_GET_STOCK_LIST`**: Boş filtre; sonra tek `MATNR` veya `WH_ID` ile.
- **`Z_GET_STOCK_DETAIL`**: Var olan ve olmayan anahtar için `EV_FOUND`.
- **`Z_CREATE_PRODUCT`**: İlk insert; aynı `MATNR` ile tekrar → hata.
- **`Z_STOCK_IN`**: Yeni `(MATNR, WH_ID)` ile insert; sonra aynı anahtarla tekrar → miktar artışı.
- **`Z_STOCK_OUT`**: Miktardan fazla çıkış → hata.
- **`Z_TRANSFER_STOCK`**: Kaynak yetersiz; kaynak=hedef; normal transfer.

---

## 8. abapGit ve sürüm kontrol

- Nesneler `ZABAP_STOCK` paket ağacı altında tutulmalı (projedeki `package.devc.xml` açıklamalarına uygun).
- Yerel `$TMP` altındaki nesneler abapGit ile taşınmaz; **paket ataması** şart.

---

## 9. Bilinen teknik notlar

- **`Z_GET_STOCK_LIST2` kaldırıldı**; yerine **`Z_GET_STOCK_LIST`** kullanılmalı (isim ve filtre uyumu).
- **`BOOLE_D` / `abap_true`**: C# tarafında genelde `bool` olarak map edilir.
- **`COMMIT WORK`**: Bu FM’lerde işlem sonunda commit vardır; BAPI tarzı iki aşamalı commit kullanılmıyorsa dikkat: uzun transaction zincirlerinde performans/lock açısından değerlendirilmeli.

---

## 10. GPT’ye verirken bağlam cümlesi (kopyala-yapıştır)

> Bu proje SAP NetWeaver üzerinde `ZBK_STOCK`, `ZBK_MATERIALS`, `ZBK_WAREHOUSES` tablolarını kullanıyor. Dış entegrasyon `ZFG_STOCK_API` function group altındaki RFC-enabled FM’ler üzerinden yapılıyor: `Z_GET_STOCK_LIST`, `Z_GET_STOCK_DETAIL`, `Z_GET_WAREHOUSE_LIST`, `Z_CREATE_PRODUCT`, `Z_STOCK_IN`, `Z_STOCK_OUT`, `Z_TRANSFER_STOCK`. Backend bu FM’leri SapNwRfc ile çağırıyor; SAP tarafında SE37 testleri ve aktivasyon tamamlanmalı.

---

*Dosya yolu (repo içi):* `SAP_RFC_ENTEGRASYON_DOKUMANI.md`
