#!/bin/bash

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

# ── Docker kontrolu ───────────────────────────────────────────────
if ! command -v docker &> /dev/null; then
    echo "  [HATA] Docker bulunamadi."
    echo "  Kurmak icin: curl -fsSL https://get.docker.com | sh"
    exit 1
fi
echo "  [OK] Docker bulundu."

# Baska MySQL container calisiyorsa bilgi ver
other_mysql=$(docker ps --filter "ancestor=mysql" --format "{{.Names}}" 2>/dev/null | grep -v "kiyp_mysql" || true)
if [ -n "$other_mysql" ]; then
    echo ""
    echo "  [!] Sistemde baska bir MySQL container calisiyor: $other_mysql"
    echo "      KIYP kendi veritabanini ic agda kullanir, bu bir sorun olusturmaz."
fi

echo ""

# ── KIYP zaten kurulu mu? ─────────────────────────────────────────
kiyp_running=$(docker ps --filter "name=kiyp_mysql" --format "{{.Names}}" 2>/dev/null || true)
if [ -n "$kiyp_running" ]; then
    echo "  [!] KIYP zaten calisiyor!"
    echo ""
    echo "  [D] Devam et    -- dokunma, zaten calisiyor"
    echo "  [Y] Yeniden     -- veriyi koru, servisleri yenile"
    echo "  [S] Sifirla     -- her seyi sil, bastan kur"
    echo ""
    read -p "  Seciminiz (D/Y/S): " kiyp_choice
    kiyp_choice=$(echo "$kiyp_choice" | tr '[:lower:]' '[:upper:]')
    echo ""

    # Klasore gec (compose dosyasi burada)
    [ -d "kiyp" ] && cd kiyp

    if [ "$kiyp_choice" = "D" ]; then
        echo "  [OK] Mevcut kurulum korundu."
        echo ""
        echo "  Uygulama  : http://localhost:8080"
        echo "  Kullanici : demo@kiyp.com"
        echo "  Sifre     : Demo1234!"
        echo ""
        exit 0
    elif [ "$kiyp_choice" = "Y" ]; then
        echo "  [..] Servisler yeniden baslatiliyor..."
        docker compose restart > /dev/null 2>&1
        echo "  [OK] Servisler yenilendi."
        echo ""
        echo "  Uygulama  : http://localhost:8080"
        echo "  Kullanici : demo@kiyp.com"
        echo "  Sifre     : Demo1234!"
        echo ""
        exit 0
    elif [ "$kiyp_choice" = "S" ]; then
        echo "  [..] Mevcut kurulum siliniyor..."
        docker compose down -v > /dev/null 2>&1
        echo "  [OK] Silindi. Kuruluma devam ediliyor..."
        echo ""
        cd ..
    else
        echo "  Gecersiz secim, cikiliyor."
        exit 1
    fi
fi

# ── Klasor olustur ────────────────────────────────────────────────
mkdir -p kiyp && cd kiyp

# ── Compose dosyasini indir ───────────────────────────────────────
echo "  [..] docker-compose.yml indiriliyor..."
curl -fsSL -o docker-compose.yml https://raw.githubusercontent.com/faxyiss/kiyp-isletme-portali/main/docker-compose.yml
echo "  [OK] docker-compose.yml indirildi."
echo ""

# ── Port kontrolu ─────────────────────────────────────────────────
find_free_port() {
    local port=$1
    while ss -tuln 2>/dev/null | grep -q ":$port " || netstat -tuln 2>/dev/null | grep -q ":$port "; do
        port=$((port + 1))
    done
    echo $port
}

frontend_port=$(find_free_port 8080)
api_port=$(find_free_port 5000)

if [ "$frontend_port" != "8080" ]; then
    echo "  [!] 8080 portu dolu, frontend $frontend_port portunda acilacak."
fi
if [ "$api_port" != "5000" ]; then
    echo "  [!] 5000 portu dolu, API $api_port portunda acilacak."
fi

# Portlari compose dosyasina yaz
sed -i "s/\"5000:8080\"/\"${api_port}:8080\"/" docker-compose.yml
sed -i "s/\"8080:80\"/\"${frontend_port}:80\"/" docker-compose.yml

echo ""

# ── Demo veri secimi ──────────────────────────────────────────────
echo "  DEMO VERI"
echo "  -------------------------------------------------"
echo "  [E] Evet -- 13 aylik ornek veri yukle"
echo "       44 urun, 25 musteri, 8 personel, 14.000+ satis"
echo "  [H] Hayir -- Bos baslat, kendi verilerini gir"
echo ""
read -p "  Seciminiz (E/H): " choice
choice=$(echo "$choice" | tr '[:lower:]' '[:upper:]')
echo ""

# ── Servisleri baslat ─────────────────────────────────────────────
echo "  [..] Servisler baslatiliyor (MySQL, API, Frontend)..."
if [ "$choice" = "E" ]; then
    docker compose --profile seed up -d > /dev/null 2>&1
else
    docker compose up -d > /dev/null 2>&1
fi
echo "  [OK] Servisler baslatildi."
echo ""

# ── Seed loglarini goster ─────────────────────────────────────────
if [ "$choice" = "E" ]; then
    echo "  [..] Demo veri yukleniyor, lutfen bekleyin..."
    echo "  -------------------------------------------------"
    echo ""
    docker compose logs -f seed
    echo ""
    echo "  -------------------------------------------------"
fi

# ── Tamamlandi ────────────────────────────────────────────────────
echo ""
echo "  KURULUM TAMAMLANDI!"
echo ""
echo "  Uygulama  : http://localhost:$frontend_port"
echo "  Kullanici : demo@kiyp.com"
echo "  Sifre     : Demo1234!"
echo ""
