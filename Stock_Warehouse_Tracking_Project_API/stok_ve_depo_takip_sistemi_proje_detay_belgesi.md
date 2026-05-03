# Stok ve Depo Takip Sistemi Projesi

## 1. Proje Adı

**Stok ve Depo Takip Sistemi**

Bu proje; SAP/ABAP tarafında oluşturulan stok ve depo verilerinin, modern bir backend API ve frontend arayüzü ile entegre edilmesini amaçlayan örnek bir kurumsal yazılım projesidir.

Proje genel olarak üç ana bölümden oluşur:

1. **SAP / ABAP Katmanı**
2. **Backend API Katmanı**
3. **Frontend Web Arayüzü**

---

## 2. Projenin Genel Amacı

Bu projenin amacı, bir işletmenin ürün, stok ve depo bilgilerini merkezi bir sistem üzerinden takip edebilmesini sağlamaktır.

Sistem sayesinde:

- Ürünler sisteme kaydedilebilir.
- Depolar tanımlanabilir.
- Ürünlerin hangi depoda ne kadar stokta olduğu takip edilebilir.
- Stok listesi görüntülenebilir.
- Yeni malzeme eklenebilir.
- Stok giriş ve çıkış işlemleri yapılabilir.
- Backend üzerinden SAP verileri dış sistemlere açılabilir.
- Frontend tarafında kullanıcı dostu bir panel üzerinden stok/depo yönetimi yapılabilir.

Bu proje özellikle SAP öğrenen biri ile backend/frontend geliştiren birinin birlikte çalışabileceği güzel bir entegrasyon projesidir.

---

## 3. Projenin Kapsamı

Proje küçük ve orta ölçekli bir stok takip sisteminin temel ihtiyaçlarını karşılayacak şekilde planlanmıştır.

### 3.1. Kapsama Dahil Olan İşlemler

- SAP tarafında özel tablolar oluşturma
- ABAP Function Module yazma
- SAP verilerini dış dünyaya açma
- Backend API ile SAP tarafındaki verileri okuma/yazma
- Frontend arayüz ile kullanıcıya görsel panel sunma
- GitHub üzerinde kodları düzenli şekilde saklama
- abapGit ile ABAP kodlarını versiyonlama
- Proje yapısını modüler hale getirme

### 3.2. Kapsama Dahil Olabilecek İleri Aşamalar

- Kullanıcı giriş sistemi
- Rol bazlı yetkilendirme
- Kritik stok uyarıları
- Depolar arası transfer
- Raporlama ekranları
- Stok hareket geçmişi
- Dashboard ve grafikler
- Loglama sistemi
- Docker ile yayınlama
- Canlı SAP sistemi veya SAP trial sistemi ile entegrasyon

---

## 4. Projenin Mimari Yapısı

Proje üç katmanlı bir yapı üzerine kuruludur.

```text
Frontend React Arayüzü
        |
        | HTTP Request / REST API
        v
ASP.NET Core Backend API
        |
        | SAP .NET Connector / RFC / REST Entegrasyonu
        v
SAP ABAP Sistemi
        |
        | ABAP Function Module + Z Tablolar
        v
SAP Veritabanı
```

### 4.1. SAP / ABAP Katmanı

SAP tarafında stok ve depo verilerinin tutulduğu ana katmandır.

Bu katmanda:

- Z tablolar oluşturulur.
- Function Module yazılır.
- Paket yapısı hazırlanır.
- Kodlar abapGit ile GitHub’a aktarılır.

### 4.2. Backend API Katmanı

Backend tarafı, frontend ile SAP arasında köprü görevi görür.

Bu katmanda:

- SAP’ye bağlantı kurulur.
- SAP Function Module çağrılır.
- Frontend’e JSON veri döndürülür.
- Kullanıcıdan gelen istekler SAP’ye iletilir.

### 4.3. Frontend Katmanı

Frontend, son kullanıcının sistemi kullandığı web arayüzüdür.

Bu katmanda:

- Login ekranı
- Dashboard ekranı
- Stok listesi
- Depo listesi
- Ürün ekleme formu
- Stok giriş/çıkış ekranı
- Raporlama ekranı

gibi sayfalar bulunabilir.

---

## 5. Kullanılan Teknolojiler

### 5.1. SAP / ABAP Tarafı

- SAP NetWeaver 7.52 Developer Edition
- ABAP Programlama Dili
- SE11 – Data Dictionary
- SE37 – Function Module
- SE80 – Object Navigator
- SE16 / SE16N – Tablo görüntüleme
- SE38 – ABAP programları
- abapGit – ABAP kodlarını GitHub’a aktarma

### 5.2. Backend Tarafı

Planlanan backend teknolojisi:

- ASP.NET Core Web API
- C#
- SAP .NET Connector, yani NCo
- JWT Authentication
- REST API mimarisi

Alternatif olarak Node.js veya başka bir backend de kullanılabilir; fakat bu proje için ASP.NET Core daha uygun görünmektedir.

### 5.3. Frontend Tarafı

Planlanan frontend teknolojisi:

- React
- Vite
- JavaScript veya TypeScript
- Axios
- CSS / Tailwind CSS
- Dashboard bileşenleri

### 5.4. Versiyon Kontrol

- Git
- GitHub
- abapGit

---

## 6. SAP Tarafında Şu Ana Kadar Yapılanlar

Bu projede SAP tarafında temel stok ve depo yapısı oluşturulmaya başlanmıştır.

Şu ana kadar öne çıkan nesneler şunlardır:

| Nesne | Türü | Açıklama |
|---|---|---|
| `ZABAP_STOCK` | Package | Stok ve depo takip projesi için ana ABAP paketi |
| `ZBK_ADD_MATERIAL` | Subpackage | Malzeme ekleme işlemleri için oluşturulan alt paket |
| `ZBK_STOCK` | Z Tablo | Ürünlerin stok bilgilerinin tutulduğu özel SAP tablosu |
| `ZBK_WAREHOUSES` | Z Tablo | Depo bilgilerinin tutulduğu özel SAP tablosu |
| `Z_GET_STOCK_LIST` | Function Module | Stok listesini dış sisteme döndürmek için kullanılan fonksiyon modülü |
| `ZADD_MATERIAL` | Function Module | Yeni malzeme/ürün eklemek için planlanan veya geliştirilen fonksiyon modülü |

---

## 7. SAP Package Yapısı

SAP tarafında düzenli geliştirme yapabilmek için package yapısı önemlidir.

### 7.1. Ana Package

```text
ZABAP_STOCK
```

Bu package, projenin SAP tarafındaki ana klasörü gibi düşünülebilir.

İçerisinde:

- Tablolar
- Function Module’ler
- Function Group’lar
- Data Element’ler
- Domain’ler
- Programlar
- Class’lar

bulunabilir.

### 7.2. Subpackage

```text
ZBK_ADD_MATERIAL
```

Bu subpackage, özellikle malzeme ekleme işlemleriyle ilgili geliştirmeleri ayrı tutmak için oluşturulmuştur.

Bu yapı sayesinde proje daha düzenli olur.

Örneğin:

```text
ZABAP_STOCK
│
├── ZBK_ADD_MATERIAL
│   ├── ZADD_MATERIAL
│   └── Malzeme ekleme ile ilgili nesneler
│
├── ZBK_STOCK
├── ZBK_WAREHOUSES
└── Z_GET_STOCK_LIST
```

---

## 8. SAP Tabloları

## 8.1. `ZBK_STOCK` Tablosu

### Amacı

`ZBK_STOCK` tablosu, ürünlerin stok bilgilerinin tutulduğu ana tablodur.

Bu tablo sayesinde sistemde hangi ürünün hangi miktarda bulunduğu takip edilir.

### Kullanım Amacı

Bu tablo şu sorulara cevap verir:

- Sistemde hangi ürünler var?
- Ürünün stok miktarı ne kadar?
- Ürün hangi depoda bulunuyor?
- Ürünün birimi nedir?
- Stok kritik seviyenin altında mı?

### Örnek Alanlar

Projede kullanılan gerçek alanlar SAP tarafında nasıl oluşturulduysa ona göre değişebilir. Genel olarak bu tablo için şu alanlar mantıklıdır:

| Alan Adı | Açıklama |
|---|---|
| `MANDT` | SAP client alanı |
| `MATERIAL_ID` | Malzeme/ürün kodu |
| `MATERIAL_NAME` | Malzeme/ürün adı |
| `WAREHOUSE_ID` | Depo kodu |
| `QUANTITY` | Stok miktarı |
| `UNIT` | Ölçü birimi |
| `MIN_STOCK` | Minimum stok seviyesi |
| `CREATED_DATE` | Oluşturulma tarihi |
| `UPDATED_DATE` | Güncellenme tarihi |

### Projedeki Rolü

Bu tablo projenin merkezindeki tablodur. Backend veya frontend stok listesini görmek istediğinde aslında bu tablodaki veriler kullanılır.

---

## 8.2. `ZBK_WAREHOUSES` Tablosu

### Amacı

`ZBK_WAREHOUSES` tablosu, sistemdeki depo bilgilerinin tutulduğu tablodur.

Bu tablo sayesinde ürünlerin hangi depoda bulunduğu anlamlı hale gelir.

### Kullanım Amacı

Bu tablo şu sorulara cevap verir:

- Sistemde kaç depo var?
- Depo kodu nedir?
- Depo adı nedir?
- Depo hangi lokasyondadır?
- Depo aktif mi pasif mi?

### Örnek Alanlar

| Alan Adı | Açıklama |
|---|---|
| `MANDT` | SAP client alanı |
| `WAREHOUSE_ID` | Depo kodu |
| `WAREHOUSE_NAME` | Depo adı |
| `LOCATION` | Depo lokasyonu |
| `IS_ACTIVE` | Depo aktiflik bilgisi |
| `CREATED_DATE` | Oluşturulma tarihi |

### Projedeki Rolü

`ZBK_WAREHOUSES` tablosu olmadan stok bilgisinin hangi depoya ait olduğu tam olarak anlaşılamaz. Bu nedenle stok tablosu ile ilişkili çalışır.

Örneğin:

```text
ZBK_STOCK.WAREHOUSE_ID = ZBK_WAREHOUSES.WAREHOUSE_ID
```

---

## 9. Function Module Yapısı

SAP tarafında dış sistemlerin veriye erişebilmesi için Function Module kullanılır.

Backend tarafı SAP’ye bağlandığında doğrudan tabloya erişmek yerine Function Module çağırır. Bu daha kontrollü ve güvenli bir yöntemdir.

---

## 9.1. `Z_GET_STOCK_LIST`

### Amacı

`Z_GET_STOCK_LIST`, SAP tarafındaki stok listesini dış sisteme aktarmak için kullanılan Function Module’dür.

Backend bu fonksiyonu çağırarak stok listesini alır.

### Ne İşe Yarar?

Bu fonksiyon:

- `ZBK_STOCK` tablosundaki stok kayıtlarını okur.
- Gerekirse depo tablosu ile ilişkilendirir.
- Sonucu backend tarafına tablo/list formatında döndürür.

### Örnek Kullanım Akışı

```text
Frontend kullanıcı stok listesini açar
        |
        v
Backend /api/stocks endpointine istek gelir
        |
        v
Backend SAP’ye bağlanır
        |
        v
Z_GET_STOCK_LIST çağrılır
        |
        v
SAP stok listesini döndürür
        |
        v
Backend JSON formatında frontend’e gönderir
```

### Projedeki Önemi

Bu fonksiyon, SAP ile dış dünya arasındaki ilk önemli veri okuma noktasıdır.

---

## 9.2. `ZADD_MATERIAL`

### Amacı

`ZADD_MATERIAL`, sisteme yeni ürün veya malzeme eklemek için kullanılan Function Module’dür.

### Ne İşe Yarar?

Bu fonksiyon:

- Backend’den gelen ürün bilgilerini alır.
- Gerekli kontrolleri yapar.
- `ZBK_STOCK` tablosuna yeni kayıt ekler.
- İşlem sonucunu başarılı veya hatalı olarak döndürür.

### Yapması Gereken Kontroller

Fonksiyon içinde şu kontroller yapılabilir:

- Malzeme kodu boş mu?
- Malzeme adı boş mu?
- Depo kodu geçerli mi?
- Aynı malzeme daha önce eklenmiş mi?
- Stok miktarı negatif mi?
- Ölçü birimi doğru mu?

### Örnek Kullanım Akışı

```text
Kullanıcı frontend üzerinden ürün ekleme formunu doldurur
        |
        v
Frontend backend API’ye POST isteği atar
        |
        v
Backend SAP’ye bağlanır
        |
        v
ZADD_MATERIAL fonksiyonu çağrılır
        |
        v
SAP ürünü ZBK_STOCK tablosuna ekler
        |
        v
Sonuç frontend’e bildirilir
```

### Projedeki Önemi

Bu fonksiyon, projenin sadece veri okuyan değil, aynı zamanda SAP’ye veri yazabilen bir sisteme dönüşmesini sağlar.

---

## 10. Function Group Mantığı

SAP’de Function Module’ler genellikle Function Group altında tutulur.

Function Group, birbiriyle ilişkili fonksiyonları bir arada tutan yapıdır.

Örneğin stok projesi için şöyle bir yapı olabilir:

```text
Function Group: ZFG_STOCK_API

Function Modules:
- Z_GET_STOCK_LIST
- ZADD_MATERIAL
- Z_UPDATE_STOCK
- Z_DELETE_MATERIAL
- Z_GET_WAREHOUSE_LIST
```

### Neden Function Group Kullanılır?

- Fonksiyonlar düzenli durur.
- Ortak değişkenler kullanılabilir.
- SAP geliştirme yapısı daha okunabilir olur.
- abapGit ile GitHub’a aktarım daha düzenli olur.

---

## 11. Backend API Tasarımı

Backend, SAP ile frontend arasında köprü görevi görür.

Frontend doğrudan SAP’ye bağlanmaz. Bunun yerine backend API’ye istek atar.

Backend de SAP Function Module çağırarak işlemi gerçekleştirir.

---

## 11.1. Backend’in Görevleri

Backend’in temel görevleri şunlardır:

- SAP bağlantısını yönetmek
- SAP Function Module çağırmak
- Gelen verileri doğrulamak
- SAP’den gelen sonucu JSON’a çevirmek
- Kullanıcı kimlik doğrulaması yapmak
- Hataları yönetmek
- Loglama yapmak
- Frontend’e temiz veri sunmak

---

## 11.2. Önerilen Backend Endpointleri

| HTTP Metodu | Endpoint | Açıklama |
|---|---|---|
| `GET` | `/api/stocks` | Stok listesini getirir |
| `GET` | `/api/stocks/{id}` | Belirli bir ürünün stok detayını getirir |
| `POST` | `/api/materials` | Yeni malzeme ekler |
| `PUT` | `/api/stocks/{id}` | Stok bilgisini günceller |
| `DELETE` | `/api/materials/{id}` | Malzemeyi pasife alır veya siler |
| `GET` | `/api/warehouses` | Depo listesini getirir |
| `POST` | `/api/warehouses` | Yeni depo ekler |
| `POST` | `/api/stock-movements/in` | Stok girişi yapar |
| `POST` | `/api/stock-movements/out` | Stok çıkışı yapar |
| `POST` | `/api/auth/login` | Kullanıcı girişi yapar |

---

## 11.3. Backend Klasör Yapısı

Örnek ASP.NET Core proje yapısı:

```text
StockWarehouseTracking.API
│
├── Controllers
│   ├── AuthController.cs
│   ├── StockController.cs
│   ├── MaterialController.cs
│   └── WarehouseController.cs
│
├── Services
│   ├── SapService.cs
│   ├── StockService.cs
│   ├── MaterialService.cs
│   └── WarehouseService.cs
│
├── Models
│   ├── StockDto.cs
│   ├── MaterialCreateDto.cs
│   └── WarehouseDto.cs
│
├── Configurations
│   └── SapSettings.cs
│
├── Program.cs
└── appsettings.json
```

---

## 11.4. SAP .NET Connector Kullanımı

SAP .NET Connector, C# uygulamasının SAP sistemine bağlanmasını sağlar.

Bu proje için backend tarafı SAP’ye şu bilgilerle bağlanabilir:

- Application Server Host
- System Number
- Client
- Username
- Password
- Language

Backend SAP’ye bağlandıktan sonra Function Module çağırır.

Örnek mantık:

```text
C# Backend
   -> SAP NCo bağlantısı açar
   -> Z_GET_STOCK_LIST fonksiyonunu çağırır
   -> SAP’den dönen tabloyu okur
   -> JSON olarak frontend’e döndürür
```

---

## 12. Frontend Tasarımı

Frontend, kullanıcının stok ve depo işlemlerini görsel olarak yapacağı web panelidir.

---

## 12.1. Frontend Sayfaları

Önerilen sayfalar:

| Sayfa | Açıklama |
|---|---|
| Login | Kullanıcı girişi |
| Dashboard | Genel özet ekranı |
| Stok Listesi | Tüm stokların listelendiği ekran |
| Ürün Ekle | Yeni ürün/malzeme ekleme ekranı |
| Depo Listesi | Depoların görüntülendiği ekran |
| Depo Ekle | Yeni depo oluşturma ekranı |
| Stok Giriş | Ürün stoğunu artırma ekranı |
| Stok Çıkış | Ürün stoğunu azaltma ekranı |
| Raporlar | Stok ve depo raporları |
| Ayarlar | Sistem ayarları |

---

## 12.2. Dashboard İçeriği

Dashboard ekranında şu bilgiler gösterilebilir:

- Toplam ürün sayısı
- Toplam depo sayısı
- Kritik stokta olan ürün sayısı
- En çok stoğu olan ürünler
- Stokta azalan ürünler
- Son stok hareketleri
- Depolara göre ürün dağılımı

---

## 12.3. Frontend Klasör Yapısı

Örnek React proje yapısı:

```text
frontend
│
├── src
│   ├── components
│   │   ├── Sidebar.jsx
│   │   ├── Navbar.jsx
│   │   ├── StockTable.jsx
│   │   └── DashboardCard.jsx
│   │
│   ├── pages
│   │   ├── Login.jsx
│   │   ├── Dashboard.jsx
│   │   ├── Stocks.jsx
│   │   ├── Materials.jsx
│   │   ├── Warehouses.jsx
│   │   └── Reports.jsx
│   │
│   ├── services
│   │   └── api.js
│   │
│   ├── App.jsx
│   └── main.jsx
│
├── package.json
└── vite.config.js
```

---

## 13. Kullanıcı Rolleri

Projede ilerleyen aşamada rol bazlı yetkilendirme eklenebilir.

### 13.1. Admin

Admin kullanıcısı sistemde tam yetkiye sahiptir.

Yapabilecekleri:

- Ürün ekleme
- Ürün silme veya pasife alma
- Stok güncelleme
- Depo ekleme
- Kullanıcı yönetimi
- Raporları görüntüleme

### 13.2. Depo Sorumlusu

Depo sorumlusu daha sınırlı yetkilere sahiptir.

Yapabilecekleri:

- Stok listesi görüntüleme
- Stok girişi yapma
- Stok çıkışı yapma
- Depo bazlı stok kontrolü

### 13.3. Sadece Görüntüleme Yetkisi Olan Kullanıcı

Bu kullanıcı sadece verileri görüntüler.

Yapabilecekleri:

- Stok listesi görüntüleme
- Depo listesi görüntüleme
- Raporları inceleme

---

## 14. Veri Akış Senaryoları

## 14.1. Stok Listesi Görüntüleme

```text
1. Kullanıcı frontend’de Stok Listesi sayfasını açar.
2. Frontend `/api/stocks` endpointine GET isteği gönderir.
3. Backend SAP bağlantısı açar.
4. Backend `Z_GET_STOCK_LIST` fonksiyonunu çağırır.
5. SAP `ZBK_STOCK` tablosundan verileri okur.
6. Sonuç backend’e döner.
7. Backend sonucu JSON formatına çevirir.
8. Frontend tablo halinde kullanıcıya gösterir.
```

---

## 14.2. Yeni Malzeme Ekleme

```text
1. Kullanıcı Ürün Ekle ekranını açar.
2. Malzeme adı, kodu, depo ve stok miktarı girilir.
3. Frontend backend’e POST isteği gönderir.
4. Backend gelen verileri kontrol eder.
5. Backend SAP’de `ZADD_MATERIAL` fonksiyonunu çağırır.
6. SAP verileri `ZBK_STOCK` tablosuna ekler.
7. SAP işlem sonucunu backend’e döndürür.
8. Backend frontend’e başarı veya hata mesajı gönderir.
```

---

## 14.3. Depo Listesi Görüntüleme

```text
1. Kullanıcı Depolar sayfasını açar.
2. Frontend `/api/warehouses` endpointine GET isteği gönderir.
3. Backend SAP’ye bağlanır.
4. SAP tarafında depo listesini döndüren fonksiyon çağrılır.
5. `ZBK_WAREHOUSES` tablosundaki kayıtlar okunur.
6. Sonuç frontend’e gönderilir.
```

---

## 15. Projede GitHub Kullanımı

Proje GitHub üzerinde ayrı repolar halinde tutulabilir.

Önerilen repo yapısı:

```text
Stock_Warehouse_Tracking_Project_ABAP
Stock_Warehouse_Tracking_Project_Backend
Stock_Warehouse_Tracking_Project_Frontend
```

Bu yapı sayesinde her katman ayrı ayrı yönetilir.

### 15.1. ABAP Repo

ABAP tarafında oluşturulan SAP nesneleri abapGit ile GitHub’a gönderilir.

Bu repoda şunlar bulunabilir:

- Z tablolar
- Function Module’ler
- Function Group’lar
- Package bilgileri
- ABAP programları

### 15.2. Backend Repo

Backend API kodları bu repoda tutulur.

İçeriğinde:

- ASP.NET Core API
- Controller’lar
- Service sınıfları
- SAP bağlantı ayarları
- DTO modelleri
- Authentication yapısı

bulunur.

### 15.3. Frontend Repo

React arayüz kodları bu repoda tutulur.

İçeriğinde:

- Sayfalar
- Component’ler
- API bağlantı servisleri
- CSS dosyaları
- Login ve dashboard yapısı

bulunur.

---

## 16. abapGit Kullanımı

ABAP kodlarını GitHub’a göndermek için abapGit kullanılır.

Genel süreç:

```text
1. GitHub’da yeni repo oluşturulur.
2. SAP sisteminde abapGit çalıştırılır.
3. Online repository bağlantısı eklenir.
4. Package seçilir.
5. SAP nesneleri repo ile eşleştirilir.
6. Stage işlemi yapılır.
7. Commit atılır.
8. Push ile GitHub’a gönderilir.
```

### 16.1. Package Seçimi

ABAP tarafındaki nesnelerin GitHub’a gitmesi için doğru package altında olması gerekir.

Örneğin:

```text
ZABAP_STOCK
```

Bu package seçilirse, bu package altındaki SAP nesneleri abapGit tarafından görülebilir.

Eğer bir nesne local object olarak `$TMP` altında kaldıysa GitHub’a düzgün gitmeyebilir. Bu nedenle nesnelerin doğru package’a taşınması gerekir.

---

## 17. Projenin Geliştirme Sırası

Projeyi sağlıklı ilerletmek için aşağıdaki sırayla geliştirmek mantıklıdır.

### 17.1. SAP Tarafı

1. Package yapısını tamamla.
2. `ZBK_STOCK` tablosunu kontrol et.
3. `ZBK_WAREHOUSES` tablosunu kontrol et.
4. `Z_GET_STOCK_LIST` fonksiyonunu tamamla.
5. `ZADD_MATERIAL` fonksiyonunu tamamla.
6. Depo listeleme fonksiyonunu ekle.
7. Stok güncelleme fonksiyonunu ekle.
8. Test verileri gir.
9. SE37 ile Function Module testlerini yap.
10. abapGit ile GitHub’a gönder.

### 17.2. Backend Tarafı

1. ASP.NET Core Web API projesi oluştur.
2. Controller yapısını kur.
3. SAP NCo bağlantısını ayarla.
4. `GetStockList` servisini yaz.
5. `AddMaterial` servisini yaz.
6. Endpointleri oluştur.
7. Swagger ile test et.
8. Hata yönetimi ekle.
9. JWT login yapısı ekle.
10. Frontend ile bağlantıya hazır hale getir.

### 17.3. Frontend Tarafı

1. React + Vite projesi oluştur.
2. Login ekranı hazırla.
3. Dashboard tasarla.
4. Sidebar ve navbar ekle.
5. Stok listesi ekranını yap.
6. Ürün ekleme formunu yap.
7. Depo ekranlarını yap.
8. API bağlantılarını Axios ile kur.
9. Hata ve başarı mesajları ekle.
10. Arayüzü responsive hale getir.

---

## 18. Önerilen SAP Function Module Listesi

Proje büyüdükçe aşağıdaki Function Module’ler eklenebilir.

| Function Module | Açıklama |
|---|---|
| `Z_GET_STOCK_LIST` | Tüm stok listesini getirir |
| `ZADD_MATERIAL` | Yeni malzeme ekler |
| `Z_UPDATE_STOCK` | Stok miktarını günceller |
| `Z_DELETE_MATERIAL` | Malzemeyi siler veya pasife alır |
| `Z_GET_WAREHOUSE_LIST` | Depo listesini getirir |
| `Z_ADD_WAREHOUSE` | Yeni depo ekler |
| `Z_STOCK_IN` | Stok girişi yapar |
| `Z_STOCK_OUT` | Stok çıkışı yapar |
| `Z_TRANSFER_STOCK` | Depolar arası stok transferi yapar |
| `Z_GET_LOW_STOCK_LIST` | Kritik stoktaki ürünleri getirir |

---

## 19. Önerilen Ek SAP Tabloları

İlerleyen aşamalarda sistem daha gerçekçi hale getirilmek istenirse ek tablolar oluşturulabilir.

| Tablo | Açıklama |
|---|---|
| `ZBK_STOCK` | Ana stok tablosu |
| `ZBK_WAREHOUSES` | Depo tablosu |
| `ZBK_MATERIALS` | Malzeme ana bilgileri |
| `ZBK_STOCK_MOVES` | Stok hareket geçmişi |
| `ZBK_USERS` | Kullanıcı bilgileri |
| `ZBK_ROLES` | Rol bilgileri |
| `ZBK_LOGS` | Sistem işlem logları |

---

## 20. Stok Hareket Mantığı

Gerçek bir stok sisteminde sadece toplam stok miktarını tutmak yeterli değildir. Stok hareketlerinin de kayıt altına alınması gerekir.

Örneğin:

- Ürün sisteme eklendi.
- Stok girişi yapıldı.
- Stok çıkışı yapıldı.
- Depolar arası transfer yapıldı.
- Stok miktarı manuel düzeltildi.

Bu işlemler için `ZBK_STOCK_MOVES` gibi bir hareket tablosu oluşturulabilir.

Örnek alanlar:

| Alan | Açıklama |
|---|---|
| `MOVE_ID` | Hareket numarası |
| `MATERIAL_ID` | Malzeme kodu |
| `WAREHOUSE_ID` | Depo kodu |
| `MOVE_TYPE` | Giriş, çıkış, transfer |
| `QUANTITY` | Hareket miktarı |
| `CREATED_BY` | İşlemi yapan kullanıcı |
| `CREATED_DATE` | İşlem tarihi |

---

## 21. Kritik Stok Mantığı

Kritik stok, ürün miktarının belirlenen minimum seviyenin altına düşmesidir.

Örnek:

```text
Ürün: Klavye
Mevcut Stok: 3
Minimum Stok: 10
Durum: Kritik stok
```

Frontend dashboard’da kritik stok ürünleri özel renkle gösterilebilir.

Backend tarafında şu mantık uygulanabilir:

```text
if quantity <= minStock
    criticalStock = true
else
    criticalStock = false
```

SAP tarafında ise `Z_GET_LOW_STOCK_LIST` fonksiyonu yazılabilir.

---

## 22. Güvenlik Yapısı

Projede güvenlik için backend tarafında JWT kullanılabilir.

### 22.1. Login Akışı

```text
1. Kullanıcı email ve şifre girer.
2. Frontend backend’e login isteği gönderir.
3. Backend kullanıcıyı doğrular.
4. Başarılıysa JWT token üretir.
5. Frontend token bilgisini saklar.
6. Sonraki API isteklerinde token gönderilir.
```

### 22.2. Yetkilendirme

Admin ve depo sorumlusu gibi roller için endpoint bazlı yetkilendirme yapılabilir.

Örnek:

```text
Admin:
- Ürün ekleyebilir
- Ürün silebilir
- Kullanıcı yönetebilir

Depo Sorumlusu:
- Stok girişi yapabilir
- Stok çıkışı yapabilir
- Liste görüntüleyebilir

Viewer:
- Sadece görüntüleme yapabilir
```

---

## 23. Hata Yönetimi

Projede kullanıcıya anlaşılır hata mesajları verilmelidir.

Örnek hatalar:

| Hata | Kullanıcıya Gösterilecek Mesaj |
|---|---|
| SAP bağlantısı yok | SAP sistemine bağlantı kurulamadı. |
| Ürün zaten var | Bu ürün kodu daha önce eklenmiş. |
| Depo bulunamadı | Seçilen depo sistemde bulunamadı. |
| Stok yetersiz | Çıkış yapılacak miktar mevcut stoktan fazla olamaz. |
| Yetkisiz işlem | Bu işlemi yapmak için yetkiniz yok. |

---

## 24. Test Planı

Projede her katman ayrı ayrı test edilmelidir.

### 24.1. SAP Testleri

- SE16 ile tablo verileri kontrol edilir.
- SE37 ile Function Module test edilir.
- Yeni malzeme ekleme testi yapılır.
- Stok listeleme testi yapılır.
- Hatalı veri girilerek kontrol yapılır.

### 24.2. Backend Testleri

- Swagger üzerinden endpointler test edilir.
- SAP bağlantısı kontrol edilir.
- Doğru ve hatalı istekler denenir.
- JSON çıktıları kontrol edilir.
- Hata mesajları test edilir.

### 24.3. Frontend Testleri

- Login ekranı test edilir.
- Stok listesi ekranı test edilir.
- Ürün ekleme formu test edilir.
- Boş alan kontrolleri yapılır.
- API’den gelen hata mesajları kontrol edilir.

---

## 25. Projenin Öğrenme Kazanımları

Bu proje sayesinde aşağıdaki konularda pratik kazanılır:

### 25.1. SAP / ABAP Kazanımları

- SAP package mantığı
- Z tablo oluşturma
- Data Element ve Domain kullanımı
- Function Module geliştirme
- Function Group mantığı
- SAP verilerini dış sisteme açma
- abapGit ile GitHub kullanımı

### 25.2. Backend Kazanımları

- ASP.NET Core API geliştirme
- SAP NCo kullanımı
- REST API mantığı
- DTO kullanımı
- Servis katmanı mimarisi
- JWT authentication
- Hata yönetimi

### 25.3. Frontend Kazanımları

- React component yapısı
- Vite proje kurulumu
- Axios ile API bağlantısı
- Dashboard tasarımı
- Form yönetimi
- Kullanıcı deneyimi geliştirme

### 25.4. Genel Yazılım Kazanımları

- Katmanlı mimari
- Entegrasyon mantığı
- GitHub kullanımı
- Proje dokümantasyonu
- Takım çalışması
- Gerçek hayata yakın iş süreci tasarımı

---

## 26. Projenin Sunumda Anlatılabilecek Kısa Özeti

Bu proje, SAP tabanlı bir stok ve depo takip sistemidir. SAP tarafında ürün ve depo bilgilerini tutmak için özel Z tablolar oluşturulmuştur. Bu tablolar üzerindeki işlemler ABAP Function Module’ler ile yönetilmektedir.

Backend tarafında ASP.NET Core API kullanılarak SAP ile frontend arasında bir köprü kurulması planlanmıştır. Backend, SAP Function Module’lerini çağırarak stok ve depo verilerini alır veya yeni kayıtları SAP’ye gönderir.

Frontend tarafında ise React ile kullanıcı dostu bir panel geliştirilecektir. Kullanıcı bu panel üzerinden stok listesini görebilecek, yeni ürün ekleyebilecek, depo bilgilerini inceleyebilecek ve kritik stok durumlarını takip edebilecektir.

Proje aynı zamanda SAP, backend ve frontend entegrasyonunu öğrenmek için güçlü bir uygulama örneğidir.

---

## 27. Proje İçin Örnek LinkedIn Açıklaması

Arkadaşımla birlikte SAP/ABAP, backend API ve frontend teknolojilerini bir araya getiren Stok ve Depo Takip Sistemi projesi üzerinde çalışıyoruz.

Proje kapsamında SAP tarafında stok ve depo bilgilerinin tutulduğu özel Z tablolar oluşturduk. ABAP Function Module’ler ile bu verilerin dış sistemler tarafından okunabilir ve yönetilebilir hale gelmesini hedefledik.

Backend tarafında ASP.NET Core Web API ile SAP arasında entegrasyon kurulması, frontend tarafında ise React ile kullanıcı dostu bir stok yönetim paneli geliştirilmesi planlanmaktadır.

Bu proje sayesinde SAP sistemlerinin modern web teknolojileriyle nasıl entegre edilebileceğini uygulamalı olarak öğrenme fırsatı bulduk.

---

## 28. Proje İçin GitHub README Taslağı

```markdown
# Stock and Warehouse Tracking System

This project is a SAP-based stock and warehouse tracking system integrated with a modern backend API and frontend web interface.

## Project Layers

- SAP / ABAP Layer
- ASP.NET Core Backend API
- React Frontend

## SAP Objects

- ZBK_STOCK
- ZBK_WAREHOUSES
- Z_GET_STOCK_LIST
- ZADD_MATERIAL

## Main Features

- Stock listing
- Material creation
- Warehouse listing
- Stock update
- Critical stock tracking
- SAP integration

## Technologies

- SAP ABAP
- abapGit
- ASP.NET Core Web API
- SAP .NET Connector
- React
- GitHub
```

---

## 29. Sonuç

Stok ve Depo Takip Sistemi projesi, SAP/ABAP ile modern web teknolojilerini bir araya getiren gerçekçi ve öğretici bir projedir.

Bu proje sayesinde SAP tarafında veri modelleme, Function Module geliştirme ve abapGit kullanımı öğrenilirken; backend tarafında API geliştirme ve SAP entegrasyonu, frontend tarafında ise kullanıcı dostu bir yönetim paneli geliştirme tecrübesi kazanılır.

Proje tamamlandığında kullanıcılar ürünleri, stok miktarlarını ve depo bilgilerini tek bir panelden yönetebilecek; SAP tarafındaki veriler modern bir web arayüzü üzerinden erişilebilir hale gelecektir.

Bu yönüyle proje hem öğrenme amaçlı hem de portföyde gösterilebilecek güçlü bir çalışma niteliğindedir.

