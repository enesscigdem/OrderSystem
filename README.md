# OrderSystem – Sipariş Yönetimi (ASP.NET Core Web API)

Bu projede temel bir **sipariş yönetimi** API’si geliştirdim. Ürün, sipariş ve sipariş kalemi üzerinden **stok düşümü** yapıyorum; siparişleri **listeleyip**, **detay** dönüp, **silme** işlemlerini sağladım. Veritabanı **SQL Server**, veri erişimi **EF Core (code-first)**, test için **Swagger** kullandım.

---

## Kullandıklarım

* **.NET 9** / ASP.NET Core Web API
* **Entity Framework Core** (SqlServer, Migrations, Concurrency)
* **SQL Server**
* **Swagger (Swashbuckle)**

---

## Mimari

Bakımı kolay ve genişlemeye açık olsun diye katmanlı bir mimari benimsedim:

```
OrderSystem.sln
 ├─ OrderSystem.Api               → Controller’lar, DI, Swagger
 ├─ OrderSystem.Application       → Servis arayüzleri + DTO’lar
 ├─ OrderSystem.Domain            → Entity’ler + Base modeller (IAudit vs.)
 └─ OrderSystem.Infrastructure    → DbContext, Migrations, Servis implementasyonları, Product için Seed
```

**Base modeller (Domain/BaseModels):**

* `IAuditEntity`  → `IsDeleted`, `IsActive`, `CreatedAt/By`, `ModifiedAt/By`
* `IEntity`, `IIntEntity`, `IIsDeletedEntity`, `IActivateableEntity`

Ben tüm sorgularda varsayılan olarak **IsDeleted = 0** ve **IsActive = 1** yaklaşımıyla ilerliyorum (soft delete).

---

## Veri Modeli

**İlişkiler:**
`Order (1) ── (N) OrderItem (N) ── (1) Product`

#### Product

| Alan       | Tip                  | Açıklama                        |
| ---------- | -------------------- | ------------------------------- |
| Id         | int (PK)             |                                 |
| Name       | nvarchar(200)        |                                 |
| Price      | decimal(18,2)        | ürünün anlık birim fiyatı       |
| Stock      | int                  | 0+                              |
| RowVersion | timestamp            | **optimistic concurrency** için |
| …          | (IAudit alanları)  |                                    |

> `RowVersion` sayesinde aynı ürünü aynı anda güncelleme girişimlerinde çakışma varsa **409 Conflict** döndürüyorum. (EF Core’da `IsRowVersion()` olarak işaretli.)

#### Order

| Alan      | Tip                     | Açıklama                                        |
| --------- | ----------------------- | ----------------------------------------------- |
| Id        | int (PK)                |                                                 |
| UserId    | nvarchar(100)           | siparişi veren kullanıcı                        |
| Total     | decimal(18,2)           | sipariş toplamı                                 |
| CreatedAt | datetime2               | **sipariş tarihi olarak kullanıyorum** (IAudit) |
| …         | (IAudit alanları)       |                                                 |
| Items     | ICollection\<OrderItem> | navigation                                      |

#### OrderItem

| Alan      | Tip                | Açıklama                                                          |
| --------- | ------------------ | ----------------------------------------------------------------- |
| Id        | int (PK)           |                                                                   |
| OrderId   | int (FK → Order)   | 1-N                                                               |
| ProductId | int (FK → Product) | 1-N                                                               |
| UnitPrice | decimal(18,2)      | **sipariş anındaki** ürün fiyatı (Product.Price’tan kopyalıyorum) |
| Quantity  | int                | >0                                                                |
| LineTotal | decimal(18,2)      | Quantity × UnitPrice                                              |
| …         | (IAudit alanları)  |                                                                   |

---

## Kurulum

* .NET 9 SDK (`dotnet --version`)
* SQL Server

### Connection string

`OrderSystem.Api/appsettings.json` içinde:

```json
"ConnectionStrings": {
  "DefaultConnection": "server=localhost;uid=...;password=...;database=OrderSystemDb;TrustServerCertificate=true;"
}
```

### Veritabanını hazırlama

Migration’lar **Infrastructure** projesinde, uygulamayı **Api** üzerinden başlatıyorum:

```bash
dotnet ef database update -p OrderSystem.Infrastructure -s OrderSystem.Api
```

Yeni migration ekleyecekseniz:

```bash
dotnet ef migrations add <MigrationAdi> -p OrderSystem.Infrastructure -s OrderSystem.Api
dotnet ef database update -p OrderSystem.Infrastructure -s OrderSystem.Api
```

Uygulama start’ında basit bir **Product seed** de atıyorum.

### 4) Çalıştırma

```bash
dotnet run --project OrderSystem.Api
```
---

## API – Uçlar ve Örnekler

### Products

* `GET    /api/Products`
* `GET    /api/Products/{id}`
* `POST   /api/Products`
* `PUT    /api/Products/{id}`        → `rowVersion` bekler
* `PATCH  /api/Products/{id}/stock`  → **stok artır/azalt (delta + rowVersion)**
* `DELETE /api/Products/{id}`        → soft delete

**Ürün oluşturma**

```http
POST /api/Products
Content-Type: application/json

{
  "name": "Klavye",
  "unitPrice": 250.00,
  "stock": 20,
  "isActive": true
}
```

**Stok güncelleme (PATCH) – iki adım:**

1. GET ile ürünü çekip `rowVersion` alıyorum.
2. Delta gönderiyorum (eksi→düşür, artı→ekle).

```http
PATCH /api/Products/1/stock
Content-Type: application/json

{
  "delta": -2,
  "rowVersion": "AAAAAAAAB9Y="
}
```

Eski `rowVersion` ile gelirsem **409** alırım (bilerek).

### Orders

* `POST   /api/Orders`
* `GET    /api/Orders?userId={id}`
* `GET    /api/Orders/{id}`
* `DELETE /api/Orders/{id}`

**Sipariş oluşturma**

```http
POST /api/Orders
Content-Type: application/json

{
  "userId": "enesscigdem",
  "items": [
    { "productId": 1, "quantity": 2 },
    { "productId": 2, "quantity": 1 }
  ]
}
```

Sunucu tarafında stok yeterliliğini kontrol ediyorum, **UnitPrice**’ı o anki değerden kilitliyorum, **LineTotal/TotalAmount** hesaplanıyor ve stok düşüyor.

**Kullanıcının siparişlerini listeleme**

```http
GET /api/Orders?userId=enesscigdem
```

---

## Notlar

* **IAudit** standardını tüm tablolara uyguladım (soft delete + audit).
* **Optimistic concurrency**’i ürün tarafında `RowVersion` ile çözdüm.
* Stok güncelleme için **PATCH /stock** uçunu tercih ettim.
* Katmanlı yapı: Domain → Application → Infrastructure → Api
* Geliştirmede hızlı test için **seed** var. Kolaylıkla test edilebilir.

---
