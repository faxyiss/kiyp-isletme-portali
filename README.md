# KIYP — Küçük İşletme Yönetim Portalı

> Küçük ve orta ölçekli işletmeler için geliştirilmiş, modern ve tam kapsamlı bir işletme yönetim sistemi.

**Canlı Demo:** [http://31.210.36.10:8080](http://31.210.36.10:8080)

---

## Proje Hakkında

KIYP, bir üniversite Sistem Analizi dersi projesi olarak geliştirilmiştir. İşletmelerin stok, satış, müşteri, üretim, gider ve personel süreçlerini tek bir platformdan yönetmesini sağlar. Uygulamada iki farklı iş modu mevcuttur: **Ticaret (Al-Sat)** ve **Üretim (İmalat/Reçete)**.

---

## Özellikler

| Modül | Açıklama |
|---|---|
| **Stok Takibi** | Lot bazlı stok yönetimi, FIFO maliyet hesabı, kritik stok uyarısı |
| **Satış Yönetimi** | Müşteri bazlı satış kaydı, otomatik stok düşümü, satış geçmişi |
| **Müşteri Portföyü** | Müşteri ekleme/düzenleme, alım geçmişi ve toplam harcama takibi |
| **Üretim Takibi** | Reçete tanımlama, hammadde tüketimi, üretim maliyet hesabı |
| **Gider Yönetimi** | Sabit, dönemsel ve tek seferlik gider kategorileri |
| **Personel & Maaş** | Personel kaydı, SGK/vergi hesaplı maaş ödeme takibi |
| **Analiz Merkezi** | Stok analizi, satış trendleri, kârlılık ve başabaş noktası analizi |
| **Dashboard** | Son 12 aylık gelir-gider grafiği, anlık özet kartları |

---

## Teknoloji Yığını

### Frontend
- **Vue 3** (Composition API + TypeScript)
- **Tailwind CSS** — utility-first stil
- **ApexCharts** — grafik ve görselleştirme
- **Lucide Vue** — ikonlar
- **Vite** — build aracı

### Backend
- **ASP.NET Core 9** (Web API)
- **Entity Framework Core** — ORM
- **MySQL** — veritabanı
- **JWT Bearer** — kimlik doğrulama

### Altyapı
- **Docker** + **Docker Compose** — konteyner orkestrasyonu
- **Nginx** — frontend sunucusu (reverse proxy)
- **Docker Hub** — imaj deposu (`caganipek/stok-proje`, `caganipek/stokappapi`)

---

## Proje Yapısı

```
kiyp/
├── Proje/                          # Vue.js Frontend uygulaması
│   ├── src/
│   │   ├── views/                  # Sayfa bileşenleri (Dashboard, Stocks, Sales…)
│   │   ├── layouts/                # AppLayout — sidebar + navigasyon
│   │   ├── router/                 # Vue Router sayfa yönlendirmeleri
│   │   ├── composables/            # useAlert gibi yeniden kullanılabilir mantıklar
│   │   └── assets/                 # Statik dosyalar
│   ├── public/
│   │   └── favicon.svg             # KIYP "K" logosu
│   ├── Dockerfile                  # Frontend Docker imajı (Node build + Nginx)
│   ├── nginx.conf                  # Nginx SPA yönlendirme ayarı
│   └── index.html                  # Uygulama giriş noktası
│
├── StokAppApi/                     # ASP.NET Core Backend API
│   └── StokAppApi/
│       ├── Controllers/            # HTTP endpoint'leri (Auth, Products, Sales…)
│       ├── Services/               # İş mantığı katmanı
│       ├── Entities/               # Veritabanı modelleri ve DTO'lar
│       ├── AppDbContext.cs         # Entity Framework veritabanı bağlamı
│       ├── appsettings.example.json # Yapılandırma şablonu (şifresiz)
│       └── Dockerfile              # Backend Docker imajı
│
├── deployment/                     # Altyapı ve kurulum dosyaları
│   ├── docker-compose.yml          # Tüm servisleri ayağa kaldıran orkestrasyon dosyası
│   ├── Dockerfile.seed             # Demo veri yükleyici container Dockerfile'ı
│   ├── seed_data.py                # 13 aylık gerçekçi demo veri üretici (Python)
│   └── dbdiagram.dbml              # Veritabanı şema diyagramı (dbdiagram.io)
│
├── .gitignore                      # Git dışında tutulan dosyalar (şifreler, build çıktıları)
└── README.md                       # Bu dosya
```

---

## Kolay Kurulum

### Yerel Bilgisayarda (Windows)

**Gereksinim:** [Docker Desktop](https://www.docker.com/products/docker-desktop)

`docker-compose.yml` dosyasını indirmek için aşağıdaki seçeneklerden birini kullan:

**Seçenek A — Tarayıcıdan indir (en kolay):**
[docker-compose.yml dosyasını buradan indir](https://raw.githubusercontent.com/faxyiss/kiyp-isletme-portali/main/docker-compose.yml) → Sağ tık → Farklı Kaydet

**Seçenek B — PowerShell:**
```powershell
Invoke-WebRequest -Uri "https://raw.githubusercontent.com/faxyiss/kiyp-isletme-portali/main/docker-compose.yml" -OutFile "docker-compose.yml"
```

**Seçenek C — Git Bash / curl.exe:**
```bash
curl.exe -O https://raw.githubusercontent.com/faxyiss/kiyp-isletme-portali/main/docker-compose.yml
```

Dosyayı indirdikten sonra bulunduğu klasörde çalıştır:
```powershell
docker compose up -d
```

Tarayıcıda açın: [http://localhost:8080](http://localhost:8080)

---

### Sunucuda (Linux VPS / Bulut)

```bash
# 1. Docker kur (Ubuntu/Debian)
curl -fsSL https://get.docker.com | sh
apt-get install -y docker-compose-plugin

# 2. Compose dosyasını indir ve başlat
curl -O https://raw.githubusercontent.com/faxyiss/kiyp-isletme-portali/main/docker-compose.yml
docker compose up -d
```

Tarayıcıda açın: `http://<sunucu-ip>:8080`

---

Her iki kurulumda da git clone, Node.js veya .NET gerekmez. Tüm imajlar Docker Hub'dan otomatik çekilir:

| İmaj | Açıklama |
|---|---|
| `mysql:8.0` | Veritabanı |
| `caganipek/kiyp-api` | Backend API |
| `caganipek/kiyp-frontend` | Frontend |
| `caganipek/kiyp-seed` | Demo veri yükleyici |

Docker Compose otomatik olarak şunları yapar:
- MySQL veritabanını indirir ve başlatır
- Tüm tabloları oluşturur
- Demo hesabı açar → `demo@kiyp.com` / `Demo1234!`
- 13 aylık gerçekçi demo veri yükler (44 ürün, 25 müşteri, 14.000+ satış)

> İlk çalıştırmada internet hızına göre 2–5 dakika sürebilir.

---

## Veritabanı Şeması

Veritabanı şeması `deployment/dbdiagram.dbml` dosyasında tanımlıdır.  
[dbdiagram.io](https://dbdiagram.io) üzerinde görüntüleyebilirsiniz.

**Başlıca tablolar:** `Users`, `Products`, `Categories`, `Stocks`, `Sales`, `Customers`, `Recipes`, `RecipeItems`, `ProductionLogs`, `Expenses`, `Employees`, `SalaryPayments`

---

## Ekran Görüntüleri

> Uygulamaya canlı olarak erişmek için: [http://31.210.36.10:8080](http://31.210.36.10:8080)

---

## Geliştirici

**Çağan İpek** — Sistem Analizi ve Tasarımı Dersi Projesi

---

## Lisans

Bu proje eğitim amaçlıdır.
