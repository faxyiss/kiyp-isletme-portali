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
Write-Host "  Kucuk Isletme Yonetim Portali — Kurulum Sihirbazi" -ForegroundColor White
Write-Host "  ─────────────────────────────────────────────────" -ForegroundColor DarkGray
Write-Host ""

# Docker kontrolu
if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Write-Host "  [HATA] Docker bulunamadi." -ForegroundColor Red
    Write-Host "  Lutfen Docker Desktop'i kurun: https://www.docker.com/products/docker-desktop" -ForegroundColor Yellow
    exit 1
}

Write-Host "  [OK] Docker bulundu." -ForegroundColor Green
Write-Host ""

# Klasor olustur
$folder = "kiyp"
if (-not (Test-Path $folder)) {
    New-Item -ItemType Directory -Path $folder | Out-Null
    Write-Host "  [OK] '$folder' klasoru olusturuldu." -ForegroundColor Green
}
Set-Location $folder

# Compose dosyasini indir
Write-Host "  [..] docker-compose.yml indiriliyor..." -ForegroundColor Cyan
Invoke-WebRequest -Uri "https://raw.githubusercontent.com/faxyiss/kiyp-isletme-portali/main/docker-compose.yml" -OutFile "docker-compose.yml" -UseBasicParsing
Write-Host "  [OK] docker-compose.yml indirildi." -ForegroundColor Green
Write-Host ""

# Demo veri secimi
Write-Host "  DEMO VERI" -ForegroundColor Yellow
Write-Host "  ─────────────────────────────────────────────────" -ForegroundColor DarkGray
Write-Host "  [E] Evet — 13 aylik ornek veri yukle" -ForegroundColor White
Write-Host "       44 urun, 25 musteri, 8 personel, 14.000+ satis" -ForegroundColor DarkGray
Write-Host "  [H] Hayir — Bos baslat, kendi verilerini gir" -ForegroundColor White
Write-Host ""
$choice = Read-Host "  Seciminiz (E/H)"
Write-Host ""

# Servisleri baslat
Write-Host "  [..] Servisler baslatiliyor (MySQL, API, Frontend)..." -ForegroundColor Cyan
if ($choice -eq "E" -or $choice -eq "e") {
    docker compose --profile seed up -d *> $null
} else {
    docker compose up -d *> $null
}
Write-Host "  [OK] Servisler baslatildi." -ForegroundColor Green
Write-Host ""

# Seed loglarini goster
if ($choice -eq "E" -or $choice -eq "e") {
    Write-Host "  [..] Demo veri yukleniyor, lutfen bekleyin..." -ForegroundColor Cyan
    Write-Host "  ─────────────────────────────────────────────────" -ForegroundColor DarkGray
    Write-Host ""
    docker compose logs -f seed
    Write-Host ""
    Write-Host "  ─────────────────────────────────────────────────" -ForegroundColor DarkGray
}

# Tamamlandi
Write-Host ""
Write-Host "  ✓ KURULUM TAMAMLANDI!" -ForegroundColor Green
Write-Host ""
Write-Host "  Uygulama   : http://localhost:8080" -ForegroundColor Cyan
Write-Host "  Kullanici  : demo@kiyp.com" -ForegroundColor White
Write-Host "  Sifre      : Demo1234!" -ForegroundColor White
Write-Host ""
