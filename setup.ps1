# KIYP Kurulum Sihirbazi
$ErrorActionPreference = "Continue"

Clear-Host
Write-Host ""
Write-Host "  ██╗  ██╗██╗██╗   ██╗██████╗ " -ForegroundColor Cyan
Write-Host "  ██║ ██╔╝██║╚██╗ ██╔╝██╔══██╗" -ForegroundColor Cyan
Write-Host "  █████╔╝ ██║ ╚████╔╝ ██████╔╝" -ForegroundColor Cyan
Write-Host "  ██╔═██╗ ██║  ╚██╔╝  ██╔═══╝ " -ForegroundColor Cyan
Write-Host "  ██║  ██╗██║   ██║   ██║     " -ForegroundColor Cyan
Write-Host "  ╚═╝  ╚═╝╚═╝   ╚═╝   ╚═╝     " -ForegroundColor Cyan
Write-Host ""
Write-Host "  Kucuk Isletme Yonetim Portali -- Kurulum Sihirbazi" -ForegroundColor White
Write-Host "  -------------------------------------------------" -ForegroundColor DarkGray
Write-Host ""

# ── Docker kontrolu ───────────────────────────────────────────────
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "  [HATA] Docker bulunamadi." -ForegroundColor Red
    Write-Host "  Lutfen Docker Desktop'i kurun: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}
Write-Host "  [OK] Docker bulundu." -ForegroundColor Green

# Baska MySQL container calisiyorsa bilgi ver
$otherMysql = docker ps --filter "ancestor=mysql" --filter "name=!kiyp_mysql" --format "{{.Names}}" 2>$null
if ($otherMysql) {
    Write-Host ""
    Write-Host "  [!] Sistemde baska bir MySQL container calisiyor: $otherMysql" -ForegroundColor Yellow
    Write-Host "      KIYP kendi veritabanini ic agda kullanir, bu bir sorun olusturmaz." -ForegroundColor DarkGray
}

Write-Host ""

# ── KIYP zaten kurulu mu? ─────────────────────────────────────────
$kiyp_running = docker ps --filter "name=kiyp_mysql" --format "{{.Names}}" 2>$null
if ($kiyp_running) {
    Write-Host "  [!] KIYP zaten calisiyor!" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  [D] Devam et    — dokunma, zaten calisiyor" -ForegroundColor White
    Write-Host "  [Y] Yeniden     — veriyi koru, servisleri yenile" -ForegroundColor White
    Write-Host "  [S] Sifirla     — her seyi sil, bastan kur" -ForegroundColor White
    Write-Host ""
    $kiyp_choice = (Read-Host "  Seciminiz (D/Y/S)").Trim().ToUpper()
    Write-Host ""

    # Klasore gec (compose dosyasi burada)
    $folder = "kiyp"
    if (Test-Path $folder) { Set-Location $folder }

    if ($kiyp_choice -eq "D") {
        Write-Host "  [OK] Mevcut kurulum korundu." -ForegroundColor Green
        Write-Host ""
        Write-Host "  Uygulama   : http://localhost:8080" -ForegroundColor Cyan
        Write-Host "  Kullanici  : demo@kiyp.com" -ForegroundColor White
        Write-Host "  Sifre      : Demo1234!" -ForegroundColor White
        Write-Host ""
        exit 0
    } elseif ($kiyp_choice -eq "Y") {
        Write-Host "  [..] Servisler yeniden baslatiliyor..." -ForegroundColor Cyan
        docker restart kiyp_mysql stok_api stok-proje *> $null
        Write-Host "  [OK] Servisler yenilendi." -ForegroundColor Green
        Write-Host ""
        Write-Host "  Uygulama   : http://localhost:8080" -ForegroundColor Cyan
        Write-Host "  Kullanici  : demo@kiyp.com" -ForegroundColor White
        Write-Host "  Sifre      : Demo1234!" -ForegroundColor White
        Write-Host ""
        exit 0
    } elseif ($kiyp_choice -eq "S") {
        Write-Host "  [..] Mevcut kurulum siliniyor..." -ForegroundColor Cyan
        docker compose down -v *> $null
        Write-Host "  [OK] Silindi. Kuruluma devam ediliyor..." -ForegroundColor Green
        Write-Host ""
        Set-Location ..
    } else {
        Write-Host "  Gecersiz secim, cikiliyor." -ForegroundColor Red
        exit 1
    }
}

# ── Klasor olustur ────────────────────────────────────────────────
$folder = "kiyp"
if (-not (Test-Path $folder)) {
    New-Item -ItemType Directory -Path $folder | Out-Null
    Write-Host "  [OK] '$folder' klasoru olusturuldu." -ForegroundColor Green
}
Set-Location $folder

# ── Compose dosyasini indir ───────────────────────────────────────
Write-Host "  [..] docker-compose.yml indiriliyor..." -ForegroundColor Cyan
Invoke-WebRequest -Uri "https://raw.githubusercontent.com/faxyiss/kiyp-isletme-portali/main/docker-compose.yml" -OutFile "docker-compose.yml" -UseBasicParsing
Write-Host "  [OK] docker-compose.yml indirildi." -ForegroundColor Green
Write-Host ""

# ── Port kontrolu ─────────────────────────────────────────────────
function Find-FreePort($start) {
    $port = $start
    while ($port -lt ($start + 20)) {
        $busy = (Get-NetTCPConnection -LocalPort $port -ErrorAction SilentlyContinue)
        if (-not $busy) { return $port }
        $port++
    }
    return $start
}

$frontendPort = Find-FreePort 8080
$apiPort      = Find-FreePort 5000

if ($frontendPort -ne 8080) {
    Write-Host "  [!] 8080 portu dolu, frontend $frontendPort portunda acilacak." -ForegroundColor Yellow
}
if ($apiPort -ne 5000) {
    Write-Host "  [!] 5000 portu dolu, API $apiPort portunda acilacak." -ForegroundColor Yellow
}

# Portlari compose dosyasina yaz
(Get-Content docker-compose.yml) `
    -replace '"5000:8080"', "`"${apiPort}:8080`"" `
    -replace '"8080:80"',   "`"${frontendPort}:80`"" |
    Set-Content docker-compose.yml

Write-Host ""

# ── Demo veri secimi ──────────────────────────────────────────────
Write-Host "  DEMO VERI" -ForegroundColor Yellow
Write-Host "  -------------------------------------------------" -ForegroundColor DarkGray
Write-Host "  [E] Evet -- 13 aylik ornek veri yukle" -ForegroundColor White
Write-Host "       44 urun, 25 musteri, 8 personel, 14.000+ satis" -ForegroundColor DarkGray
Write-Host "  [H] Hayir -- Bos baslat, kendi verilerini gir" -ForegroundColor White
Write-Host ""
$choice = (Read-Host "  Seciminiz (E/H)").Trim().ToUpper()
Write-Host ""

# ── Servisleri baslat ─────────────────────────────────────────────
Write-Host "  [..] Servisler baslatiliyor (MySQL, API, Frontend)..." -ForegroundColor Cyan
if ($choice -eq "E") {
    docker compose --profile seed up -d *> $null
} else {
    docker compose up -d *> $null
}
Write-Host "  [OK] Servisler baslatildi." -ForegroundColor Green
Write-Host ""

# ── Seed loglarini goster ─────────────────────────────────────────
if ($choice -eq "E") {
    Write-Host "  [..] Demo veri yukleniyor, lutfen bekleyin..." -ForegroundColor Cyan
    Write-Host "  -------------------------------------------------" -ForegroundColor DarkGray
    Write-Host ""
    docker compose logs -f seed
    Write-Host ""
    Write-Host "  -------------------------------------------------" -ForegroundColor DarkGray
}

# ── Tamamlandi ────────────────────────────────────────────────────
Write-Host ""
Write-Host "  KURULUM TAMAMLANDI!" -ForegroundColor Green
Write-Host ""
Write-Host "  Uygulama   : http://localhost:$frontendPort" -ForegroundColor Cyan
Write-Host "  Kullanici  : demo@kiyp.com" -ForegroundColor White
Write-Host "  Sifre      : Demo1234!" -ForegroundColor White
Write-Host ""
