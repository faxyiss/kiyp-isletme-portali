#!/bin/bash
set -e

clear
echo ""
echo "  ██╗  ██╗██╗██╗   ██╗██████╗ "
echo "  ██║ ██╔╝██║╚██╗ ██╔╝██╔══██╗"
echo "  █████╔╝ ██║ ╚████╔╝ ██████╔╝"
echo "  ██╔═██╗ ██║  ╚██╔╝  ██╔═══╝ "
echo "  ██║  ██╗██║   ██║   ██║     "
echo "  ╚═╝  ╚═╝╚═╝   ╚═╝   ╚═╝     "
echo ""
echo "  Kucuk Isletme Yonetim Portali -- Kurulum Sihirbazi"
echo "  -------------------------------------------------"
echo ""

# Docker kontrolu
if ! command -v docker &> /dev/null; then
    echo "  [HATA] Docker bulunamadi."
    echo "  Kurmak icin: curl -fsSL https://get.docker.com | sh"
    exit 1
fi

echo "  [OK] Docker bulundu."
echo ""

# Klasor olustur
mkdir -p kiyp && cd kiyp

echo "  [..] docker-compose.yml indiriliyor..."
curl -fsSL -o docker-compose.yml https://raw.githubusercontent.com/faxyiss/kiyp-isletme-portali/main/docker-compose.yml
echo "  [OK] docker-compose.yml indirildi."
echo ""

# Demo veri secimi
echo "  DEMO VERI"
echo "  -------------------------------------------------"
echo "  [E] Evet -- 13 aylik ornek veri yukle"
echo "       44 urun, 25 musteri, 8 personel, 14.000+ satis"
echo "  [H] Hayir -- Bos baslat, kendi verilerini gir"
echo ""
read -p "  Seciminiz (E/H): " choice
echo ""

# Servisleri baslat
echo "  [..] Servisler baslatiliyor (MySQL, API, Frontend)..."
if [[ "$choice" == "E" || "$choice" == "e" ]]; then
    docker compose --profile seed up -d > /dev/null 2>&1
else
    docker compose up -d > /dev/null 2>&1
fi
echo "  [OK] Servisler baslatildi."
echo ""

# Seed loglarini goster
if [[ "$choice" == "E" || "$choice" == "e" ]]; then
    echo "  [..] Demo veri yukleniyor, lutfen bekleyin..."
    echo "  -------------------------------------------------"
    echo ""
    docker compose logs -f seed
    echo ""
    echo "  -------------------------------------------------"
fi

# Tamamlandi
echo ""
echo "  KURULUM TAMAMLANDI!"
echo ""
echo "  Uygulama  : http://localhost:8080"
echo "  Kullanici : demo@kiyp.com"
echo "  Sifre     : Demo1234!"
echo ""
