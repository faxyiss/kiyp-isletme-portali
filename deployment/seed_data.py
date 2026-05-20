#!/usr/bin/env python3
"""
Sunum Verisi Yükleyici — Süpermarket & Fırın (Hibrit)

Kullanıcı / Kategori / Ürün / Müşteri / Personel / Maaş / İzin → API
Gider / Stok / Üretim / Satış                                   → Direkt SQL (bulk)

Kurulum  : pip install pymysql requests
Kullanım :
  Sunucu (Docker iç ağı)  →  python seed_data.py
  Dışarıdan               →  python seed_data.py --api-url http://31.210.36.10:5000 --db-host localhost

Hesap : ornekisletme@posta.com / ornek123
"""

import argparse, uuid, random, calendar as cal, sys, time
from datetime import datetime, timedelta

try:
    import pymysql, pymysql.cursors
except ImportError:
    sys.exit("HATA: pip install pymysql")
try:
    import requests
except ImportError:
    sys.exit("HATA: pip install requests")

# ── Sabitler ──────────────────────────────────────────────────────────────────

random.seed(42)

START = datetime(2025, 5,  1)
END   = datetime(2026, 5, 20)

SEASONAL   = {1:0.82,2:0.88,3:0.98,4:1.02,5:1.08,6:1.18,7:1.28,8:1.20,9:0.93,10:1.00,11:1.12,12:1.45}
YEAR_GROWTH = {2025:1.00, 2026:1.12}

NAKIT=0; KREDI_KART=1; VERESIYE=2
CAT_URUN=0; CAT_HAMMADDE=1; BIZ_URETIM=1

DEMO_EMAIL    = "ornekisletme@posta.com"
DEMO_PASSWORD = "ornek123"

# ── Ürün/Kategori tanımları ────────────────────────────────────────────────────

URUN_KATEGORILERI = [
    {"key":"firin","name":"Fırın Ürünleri","products":[
        ("ekmek",    "Ekmek (250g)",              14.0,  5.5, 90, 1.05, True),
        ("simit",    "Simit",                      8.0,  3.0,130, 1.03, True),
        ("pogaca",   "Poğaça",                    16.0,  6.5, 65, 1.07, True),
        ("borek",    "El Açması Börek (Dilim)",   38.0, 15.0, 28, 1.09, True),
        ("kurabiye", "Özel Kurabiye (10'lu)",    100.0, 44.0, 18, 1.14, True),
        ("pasta",    "Pasta (Küçük)",            290.0,125.0,  4, 0.90, True),
    ]},
    {"key":"gida","name":"Gıda & Bakliyat","products":[
        ("makarna",  "Makarna (500g)",  22.0, 14.0, 42, 1.04, False),
        ("pirinc",   "Pirinç (1kg)",   40.0, 26.0, 36, 1.06, False),
        ("mercimek", "Mercimek (1kg)", 34.0, 22.0, 30, 1.03, False),
        ("bulgur",   "Bulgur (1kg)",   26.0, 17.0, 24, 1.02, False),
        ("nohut",    "Nohut (1kg)",    32.0, 21.0, 22, 1.04, False),
    ]},
    {"key":"icecek","name":"İçecekler","products":[
        ("su_15",      "Su (1.5L)",               10.0,  5.5, 75, 1.10, False),
        ("kola",       "Kola (1L)",               40.0, 26.0, 48, 1.08, False),
        ("meyve_suyu", "Meyve Suyu (1L)",         34.0, 22.0, 35, 1.07, False),
        ("cay",        "Çay (500g)",              98.0, 64.0, 22, 1.13, False),
        ("enerji",     "Enerji İçeceği (250ml)",  28.0, 17.0, 30, 1.18, False),
    ]},
    {"key":"sut","name":"Süt & Süt Ürünleri","products":[
        ("sut",    "Süt (1L)",             28.0, 19.0, 55, 1.06, False),
        ("yogurt", "Yoğurt (1kg)",         45.0, 31.0, 40, 1.05, False),
        ("peynir", "Beyaz Peynir (500g)", 115.0, 80.0, 22, 1.08, False),
        ("ayran",  "Ayran (500ml)",        22.0, 14.0, 45, 1.07, False),
    ]},
    {"key":"et","name":"Et & Şarküteri","products":[
        ("sucuk",    "Sucuk (200g)",        98.0, 70.0, 20, 1.05, False),
        ("salam",    "Salam (200g)",        72.0, 51.0, 18, 1.03, False),
        ("tavuk",    "Tavuk Göğsü (1kg)", 145.0,102.0, 28, 1.11, False),
        ("pastirma", "Pastırma (100g)",   108.0, 75.0, 12, 1.02, False),
    ]},
    {"key":"temizlik","name":"Temizlik Ürünleri","products":[
        ("cam_det",  "Çamaşır Deterjanı (3kg)",  165.0,105.0, 24, 1.08, False),
        ("bul_det",  "Bulaşık Deterjanı (750ml)", 62.0, 39.0, 30, 1.07, False),
        ("cam_suyu", "Çamaşır Suyu (1L)",         38.0, 24.0, 26, 1.05, False),
        ("yuzey",    "Yüzey Temizleyici (500ml)", 50.0, 31.0, 22, 1.06, False),
        ("el_sabunu","Sıvı El Sabunu (500ml)",    40.0, 25.0, 20, 1.10, False),
    ]},
    {"key":"bakim","name":"Kişisel Bakım","products":[
        ("sampuan",   "Şampuan (400ml)",     75.0, 46.0, 22, 1.09, False),
        ("dis_macunu","Diş Macunu (150ml)",  44.0, 27.0, 28, 1.07, False),
        ("sabun",     "Sabun (4'lü paket)",  52.0, 32.0, 20, 1.05, False),
        ("deodorant", "Deodorant (150ml)",   85.0, 53.0, 16, 1.08, False),
    ]},
    {"key":"atistirma","name":"Atıştırmalık & Çerez","products":[
        ("cips",      "Cips (150g)",                40.0, 25.0, 40, 1.12, False),
        ("cikolata",  "Çikolata (100g)",            35.0, 22.0, 44, 1.10, False),
        ("biskuvi",   "Bisküvi (200g)",             28.0, 17.0, 36, 1.08, False),
        ("kuruyemis", "Karışık Kuruyemiş (200g)",   78.0, 50.0, 22, 1.14, False),
    ]},
]

HAMMADDE_KATEGORILERI = [
    {"key":"temel_hm","name":"Temel Hammaddeler","products":[
        ("un",       "Un (25kg çuval)",          540.0, 50),
        ("seker",    "Kristal Şeker (50kg)",     610.0, 25),
        ("tereyagi", "Tereyağı (1kg paket)",     245.0, 90),
        ("yumurta",  "Yumurta (30'lu viyol)",   150.0, 70),
        ("tuz",      "Tuz (5kg)",                48.0, 15),
        ("maya",     "Instant Maya (500g)",       65.0, 20),
    ]},
    {"key":"ambalaj_hm","name":"Ambalaj Malzemeleri","products":[
        ("ambalaj",  "Ambalaj Poşeti & Kutu (100 adet)", 310.0, 40),
    ]},
]

RECETELER = {
    "ekmek":    [("un",0.25),("tuz",0.01),("maya",0.005),("ambalaj",0.005)],
    "simit":    [("un",0.10),("seker",0.02),("yumurta",0.03),("ambalaj",0.003)],
    "pogaca":   [("un",0.15),("tereyagi",0.05),("yumurta",0.05),("ambalaj",0.008)],
    "borek":    [("un",0.20),("tereyagi",0.08),("yumurta",0.05),("tuz",0.005),("ambalaj",0.01)],
    "kurabiye": [("un",0.30),("seker",0.15),("tereyagi",0.12),("yumurta",0.05),("ambalaj",0.01)],
    "pasta":    [("un",0.50),("seker",0.30),("tereyagi",0.25),("yumurta",0.20),("ambalaj",0.01)],
}

# ── Yardımcılar ───────────────────────────────────────────────────────────────

def r2(v):  return round(float(v), 2)

def months_in(s, e):
    res, d = [], s.replace(day=1,hour=0,minute=0,second=0,microsecond=0)
    while d <= e:
        res.append(d)
        d = d.replace(month=d.month%12+1, year=d.year+(1 if d.month==12 else 0))
    return res

def db_connect(host, port, db, user, pw):
    return pymysql.connect(host=host,port=port,database=db,user=user,password=pw,
                           charset="utf8mb4",autocommit=False,
                           cursorclass=pymysql.cursors.DictCursor)

def ex(c,sql,a=None):
    with c.cursor() as cur: cur.execute(sql,a or ())

def exmany(c,sql,rows,n=400):
    for i in range(0,len(rows),n):
        with c.cursor() as cur: cur.executemany(sql,rows[i:i+n])

def qall(c,sql,a=None):
    with c.cursor() as cur: cur.execute(sql,a or ()); return cur.fetchall()

def qone(c,sql,a=None):
    with c.cursor() as cur: cur.execute(sql,a or ()); return cur.fetchone()

# ── API yardımcıları ──────────────────────────────────────────────────────────

def api(method, api_url, path, body=None, token=None):
    headers = {"Content-Type":"application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    url = f"{api_url}{path}"
    resp = getattr(requests, method)(url, json=body, headers=headers, timeout=30)
    if not resp.ok:
        raise Exception(f"API {method.upper()} {path} → {resp.status_code}: {resp.text[:200]}")
    return resp.json() if resp.content else {}

def api_post(api_url, path, body, token=None):
    return api("post", api_url, path, body, token)

# ── SQL temizlik ──────────────────────────────────────────────────────────────

def cleanup(conn, user_id):
    uid = user_id
    print(f"  Eski veriler temizleniyor (userId={uid})...")
    for r in qall(conn,"SELECT r.Id FROM Recipes r JOIN Products p ON r.ProductId=p.Id WHERE p.UserId=%s",(uid,)):
        ex(conn,"DELETE FROM RecipeItems WHERE RecipeId=%s",(r["Id"],))
        ex(conn,"DELETE FROM Recipes      WHERE Id=%s",     (r["Id"],))
    for r in qall(conn,"SELECT Id FROM Employees WHERE UserId=%s",(uid,)):
        ex(conn,"DELETE FROM SalaryPayments WHERE EmployeeId=%s",(r["Id"],))
        ex(conn,"DELETE FROM LeaveRecords   WHERE EmployeeId=%s",(r["Id"],))
    ex(conn,"DELETE FROM Employees WHERE UserId=%s",(uid,))
    for t in ["Sales","ProductionLogs","Stocks","Customers","Expenses",
              "ExpenseCategories","Products","Categories","BusinessProfiles"]:
        ex(conn,f"DELETE FROM `{t}` WHERE UserId=%s",(uid,))
    conn.commit()
    print("  ✓ Temizlik tamamlandı")
    return uid

# ── Ana seed ──────────────────────────────────────────────────────────────────

def seed(api_url, conn):
    MONTHS = months_in(START, END)

    # ── 1. Giriş (API) ────────────────────────────────────────────
    print("\n[1/8] Giriş yapılıyor (API)...")
    data = api_post(api_url, "/api/auth/login", {
        "email":    DEMO_EMAIL,
        "password": DEMO_PASSWORD,
    })
    token   = data["token"]
    user_id = str(data["userId"])
    print(f"  ✓ {DEMO_EMAIL}  (userId={user_id})")

    cleanup(conn, user_id)

    # İşletme profili (SQL — API endpoint yok)
    t0 = START - timedelta(days=75)
    ex(conn,"""INSERT INTO BusinessProfiles
       (Id,UserId,CompanyName,Phone,ContactEmail,Address,TaxOffice,TaxNumber,CreatedAt,UpdatedAt)
       VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,%s)""",
       (str(uuid.uuid4()),user_id,"Demir Süpermarket & Fırın A.Ş.","0212 345 67 89",
        "info@demirsupermarket.com","Atatürk Cad. No:42, Kadıköy / İstanbul",
        "Kadıköy VD","1122334455",t0,t0))
    conn.commit()

    # ── 2. Kategoriler (API) ───────────────────────────────────────
    print("\n[2/8] Kategoriler oluşturuluyor (API)...")
    cat_ids = {}   # key → guid

    for cat in URUN_KATEGORILERI:
        res = api_post(api_url, "/api/categories/create",
                       {"name": cat["name"], "type": CAT_URUN}, token)
        cat_ids[cat["key"]] = str(res["category"]["id"])
        print(f"  ✓ {cat['name']}")

    for cat in HAMMADDE_KATEGORILERI:
        res = api_post(api_url, "/api/categories/create",
                       {"name": cat["name"], "type": CAT_HAMMADDE}, token)
        cat_ids[cat["key"]] = str(res["category"]["id"])
        print(f"  ✓ {cat['name']}")

    # ── 3. Ürünler (API) ───────────────────────────────────────────
    print("\n[3/8] Ürünler oluşturuluyor (API)...")
    prod_map = {}   # key → {id, price, cost, daily, growth, produced}
    firin_keys = set()

    for cat in URUN_KATEGORILERI:
        for pk, pname, price, cost, daily, growth, produced in cat["products"]:
            res = api_post(api_url, "/api/products/create", {
                "categoryId": cat_ids[cat["key"]],
                "name":       pname,
                "unitPrice":  price,
            }, token)
            prod_map[pk] = dict(id=str(res["id"]),price=price,cost=cost,daily=daily,growth=growth,produced=produced)
            if produced: firin_keys.add(pk)
        print(f"  ✓ {cat['name']} ({len(cat['products'])} ürün)")

    hm_map = {}   # key → {id, cost, monthly_qty}
    for cat in HAMMADDE_KATEGORILERI:
        for pk, pname, cost, mq in cat["products"]:
            res = api_post(api_url, "/api/products/create", {
                "categoryId": cat_ids[cat["key"]],
                "name":       pname,
                "unitPrice":  cost,
            }, token)
            hm_map[pk] = dict(id=str(res["id"]),cost=cost,monthly_qty=mq)
        print(f"  ✓ {cat['name']} ({len(cat['products'])} hammadde)")

    # ── 4. Reçeteler (SQL — API endpoint yok) ─────────────────────
    print("\n[4/8] Reçeteler oluşturuluyor (SQL)...")
    t2 = START - timedelta(days=68)
    for fp_key, items in RECETELER.items():
        rid = str(uuid.uuid4())
        ex(conn,"INSERT INTO Recipes (Id,ProductId,CreatedAt,UpdatedAt) VALUES (%s,%s,%s,%s)",
           (rid, prod_map[fp_key]["id"], t2, t2))
        for hm_key, qty in items:
            if hm_key in hm_map:
                ex(conn,"INSERT INTO RecipeItems (Id,RecipeId,RawMaterialId,RequiredQuantity) VALUES (%s,%s,%s,%s)",
                   (str(uuid.uuid4()), rid, hm_map[hm_key]["id"], qty))
    conn.commit()
    print(f"  ✓ {len(RECETELER)} reçete")

    # ── 5. Müşteriler (API) ────────────────────────────────────────
    print("\n[5/8] Müşteriler oluşturuluyor (API)...")
    NAMES = [
        ("Fatma","Kaya"),("Ali","Yıldız"),("Zeynep","Şahin"),("Hüseyin","Çelik"),
        ("Elif","Arslan"),("Murat","Doğan"),("Selin","Korkmaz"),("Emre","Özdemir"),
        ("Ayşe","Karataş"),("Burak","Yılmaz"),("Gülden","Aydın"),("Serkan","Kılıç"),
        ("Merve","Öztürk"),("Tolga","Güneş"),("Canan","Erdoğan"),("Kemal","Demir"),
        ("Esra","Çınar"),("Uğur","Taşkın"),("Derya","Polat"),("Tarık","Şimşek"),
        ("Leyla","Acar"),("Orhan","Bulut"),("Sevgi","Çetin"),("Hakan","Doğru"),
        ("Pınar","Yıldırım"),
    ]
    PRFX = ["0532","0533","0535","0542","0543","0545","0505","0506"]
    CR_IDX = {0,2,4,6,9,12,15,18,21}

    customers  = []   # (id, month_idx, is_credit)
    cust_bal   = {}

    for i,(fn,ln) in enumerate(NAMES):
        ph   = f"{random.choice(PRFX)} {random.randint(100,999)} {random.randint(10,99):02d} {random.randint(10,99):02d}"
        res  = api_post(api_url, "/api/customers",
                        {"firstName":fn,"lastName":ln,"phoneNumber":ph,"initialDebt":0}, token)
        cid  = str(res["id"])
        mi   = min(int(i*len(MONTHS)/len(NAMES)), len(MONTHS)-1)
        is_cr = (i in CR_IDX)
        customers.append((cid, mi, is_cr))
        cust_bal[cid] = 0.0

        # createdAt'ı gerçekçi tarihe çek (SQL update)
        cdt = MONTHS[mi] + timedelta(days=random.randint(1,18))
        ex(conn,"UPDATE Customers SET CreatedAt=%s,UpdatedAt=%s WHERE Id=%s",(cdt,cdt,cid))

    conn.commit()
    print(f"  ✓ {len(customers)} müşteri")

    def avail(mi):
        return [(c,cr) for c,cm,cr in customers if cm<=mi]

    # ── 6. Personel (API) + Maaş/İzin (API) ──────────────────────
    print("\n[6/8] Personel oluşturuluyor (API)...")
    EMP_DEFS = [
        ("Ahmet Demir",   "Mağaza Müdürü",        "0532 111 22 33", 45000.0, 24),
        ("Fatma Kaya",    "Fırın Ustası",          "0542 222 33 44", 32000.0, 24),
        ("Mehmet Yıldız", "Muhasebe Sorumlusu",    "0553 333 44 55", 30000.0, 24),
        ("Ayşe Çelik",    "Kasiyer",               "0505 444 55 66", 22000.0, 15),
        ("Ali Arslan",    "Kasiyer",               "0544 555 66 77", 22000.0, 15),
        ("Selin Öztürk",  "Fırın İşçisi",          "0533 666 77 88", 24000.0, 12),
        ("Burak Çetin",   "Depo & Market Sorumlusu","0506 777 88 99",26000.0, 12),
        ("Zeynep Koç",    "Fırın İşçisi",          "0551 888 99 00", 23000.0,  8),
    ]
    employees = []   # (id, gross)

    for name, pos, ph, gross, months_ago in EMP_DEFS:
        sd = (START - timedelta(days=months_ago*30)).strftime("%Y-%m-%dT%H:%M:%S")
        res = api_post(api_url, "/api/employees",
                       {"fullName":name,"position":pos,"phone":ph,
                        "startDate":sd,"grossSalary":gross,"isActive":True}, token)
        eid = str(res["employee"]["id"])
        employees.append((eid, gross))

        # Maaş ödemeleri (API) — kademeli zam: 6. ayda %10, 12. ayda %20
        for mi2, mdt2 in enumerate(MONTHS):
            if mdt2 > END: break
            if mi2 < 6:
                eff_gross = gross
            elif mi2 < 12:
                eff_gross = round(gross * 1.10 / 500) * 500
            else:
                eff_gross = round(gross * 1.20 / 500) * 500
            api_post(api_url, f"/api/employees/{eid}/payments",
                     {"month":mdt2.month,"year":mdt2.year,"grossSalary":eff_gross}, token)

        print(f"  ✓ {name} — {len(MONTHS)} maaş ödendi")

    # İzin kayıtları (API)
    LEAVE = [
        (employees[0][0], START+timedelta(days=45),  7, "Yıllık İzin"),
        (employees[1][0], START+timedelta(days=82),  5, "Yıllık İzin"),
        (employees[2][0], START+timedelta(days=110), 3, "Hastalık İzni"),
        (employees[3][0], START+timedelta(days=155), 7, "Yıllık İzin"),
        (employees[4][0], START+timedelta(days=200),10, "Yıllık İzin"),
        (employees[5][0], START+timedelta(days=62),  2, "Hastalık İzni"),
        (employees[6][0], START+timedelta(days=235), 5, "Yıllık İzin"),
        (employees[7][0], START+timedelta(days=95),  3, "Hastalık İzni"),
        (employees[0][0], START+timedelta(days=278), 7, "Yıllık İzin"),
        (employees[1][0], START+timedelta(days=315),14, "Yıllık İzin"),
    ]
    for eid, st, days, reason in LEAVE:
        if st <= END:
            ed = st + timedelta(days=days)
            api_post(api_url, f"/api/employees/{eid}/leaves",
                     {"startDate": st.strftime("%Y-%m-%dT%H:%M:%S"),
                      "endDate":   ed.strftime("%Y-%m-%dT%H:%M:%S"),
                      "reason":    reason}, token)
    print(f"  ✓ {len([l for l in LEAVE if l[1]<=END])} izin kaydı")

    # ── 7. Giderler (SQL — API endpoint yok) ──────────────────────
    print("\n[7/8] Giderler oluşturuluyor (SQL)...")
    EC = {}
    for k,n in [("kira","Kira Giderleri"),("enerji","Enerji Giderleri"),
                ("pazarlama","Pazarlama & Reklam"),("bakim","Bakım & Onarım"),
                ("idari","İdari Giderler"),("lojistik","Lojistik & Taşıma")]:
        eid2 = str(uuid.uuid4()); EC[k] = eid2
        ex(conn,"INSERT INTO ExpenseCategories (Id,UserId,Name,CreatedAt) VALUES (%s,%s,%s,%s)",
           (eid2,user_id,n,START-timedelta(days=65)))

    exp_rows = []
    def ae(cat,title,amt,dt,etype=0):
        exp_rows.append((str(uuid.uuid4()),user_id,EC[cat],title,r2(amt),etype,dt,None,None,None,dt,dt))

    for mdt in MONTHS:
        if mdt > END: break
        m = mdt.month
        ae("kira",     "İşyeri Kirası",                       52000,               mdt.replace(day=1),  etype=1)
        ae("enerji",   "Elektrik Faturası",  (8500 if m in(12,1,2) else 6000 if m in(6,7,8) else 4500)+random.gauss(0,350), mdt.replace(day=10), etype=1)
        ae("enerji",   "Doğalgaz Faturası",  (9000 if m in(12,1,2) else 3500 if m in(3,11) else 1200)+random.gauss(0,300), mdt.replace(day=12), etype=1)
        ae("enerji",   "Su Faturası",         1500+random.gauss(0,120),         mdt.replace(day=15), etype=1)
        ae("idari",    "İnternet & Telefon",  2000,                             mdt.replace(day=5),  etype=1)
        ae("idari",    "ERP & Kasa Yazılımı", 1600,                             mdt.replace(day=1),  etype=1)
        ae("idari",    "Güvenlik Sistemi",    2500,                             mdt.replace(day=3),  etype=1)
        ae("idari",    "Temizlik Hizmeti",    3000+random.gauss(0,200),         mdt.replace(day=7),  etype=2)
        ae("pazarlama","Dijital Reklam",      (8000 if m in(12,6,7) else 5000)+random.gauss(0,600), mdt.replace(day=3),  etype=2)
        ae("lojistik", "Tedarikçi Nakliye",  (11000 if m in(12,1) else 7500)+random.gauss(0,700),  mdt.replace(day=8),  etype=2)
        ae("bakim",    "Fırın Ekipman Bakımı",2800+random.gauss(0,300),        mdt.replace(day=20), etype=2)

    for dt,cat,title,amt in [
        (START+timedelta(days=20),  "bakim",     "Fırın Ekipmanı Yenileme",   28000),
        (START+timedelta(days=55),  "pazarlama", "Açılış Kampanyası",         22000),
        (START+timedelta(days=120), "bakim",     "Soğuk Hava Deposu Tamiri",  12000),
        (START+timedelta(days=165), "pazarlama", "Yılbaşı Kampanyası",        25000),
        (START+timedelta(days=200), "idari",     "Kasa & Barkod Sistemi",     40000),
        (START+timedelta(days=248), "bakim",     "Fırın Konveyör Bakımı",     18000),
        (START+timedelta(days=290), "lojistik",  "Araç Kiralama (Yıllık)",    32000),
        (START+timedelta(days=330), "pazarlama", "Katalog & Tabela",           9000),
    ]:
        if dt <= END: ae(cat,title,amt,dt)

    exmany(conn,"""INSERT INTO Expenses
        (Id,UserId,CategoryId,Title,Amount,ExpenseType,StartDate,EndDate,RecurringDay,Notes,CreatedAt,UpdatedAt)
        VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s)""", exp_rows)
    conn.commit()
    print(f"  ✓ {len(exp_rows)} gider kaydı")

    # ── 8. Stok + Üretim + Satış (SQL — bulk) ─────────────────────
    print("\n[8/8] Stok, üretim ve satış verileri oluşturuluyor (SQL)...")

    stk_in  = {}; stk_sold = {}; mon_stk = {}
    hm_rows = []; fp_rows = []; prod_rows = []; sale_rows = []

    for mi, mdt in enumerate(MONTHS):
        if mdt > END: break
        m, y = mdt.month, mdt.year
        seasonal = SEASONAL.get(m,1.0) * YEAR_GROWTH.get(y,1.0)
        days_cnt = END.day if (y==END.year and m==END.month) else cal.monthrange(y,m)[1]
        av      = avail(mi)
        cr_custs = [c for c,cr in av if cr]
        pur_day  = mdt.replace(day=random.randint(1,4))

        # Hammadde alımı
        for cat in HAMMADDE_KATEGORILERI:
            for pk,*_ in cat["products"]:
                info = hm_map[pk]
                cost = r2(info["cost"] * (1.020**mi) * random.uniform(0.97, 1.03))
                qty  = max(int(info["monthly_qty"]*seasonal*random.uniform(0.90,1.15)),8)
                rem  = max(1,int(qty*0.12))
                sid  = str(uuid.uuid4())
                hm_rows.append((sid,user_id,info["id"],qty,rem,cost,pur_day,pur_day,pur_day))

        # Fırın: üretim + stok
        for pk in firin_keys:
            info = prod_map[pk]
            gf   = info["growth"]**(mi/12.0)
            qty  = max(int(info["daily"]*days_cnt*seasonal*gf*1.10),info["daily"]*4)
            cost = r2(info["cost"]*qty)
            prod_day = mdt.replace(day=random.randint(2,6))
            prod_rows.append((str(uuid.uuid4()),user_id,info["id"],qty,cost,prod_day))
            sid = str(uuid.uuid4())
            mon_stk[(mi,pk)] = sid
            fp_rows.append((sid,user_id,info["id"],qty,qty,info["cost"],prod_day,prod_day,prod_day))
            stk_in[sid]=qty; stk_sold[sid]=0

        # Market ürünleri: tedarikçi stoku
        for cat in URUN_KATEGORILERI:
            for pk,*rest in cat["products"]:
                if rest[-1]: continue   # fırın ürünleri yukarıda
                info = prod_map[pk]
                gf   = info["growth"]**(mi/12.0)
                qty  = max(int(info["daily"]*days_cnt*seasonal*gf*1.20),info["daily"]*4)
                cost = r2(info["cost"] * (1.015**mi) * random.uniform(0.97, 1.03))
                sid  = str(uuid.uuid4())
                mon_stk[(mi,pk)] = sid
                fp_rows.append((sid,user_id,info["id"],qty,qty,cost,pur_day,pur_day,pur_day))
                stk_in[sid]=qty; stk_sold[sid]=0

        # Günlük satışlar
        for day in range(1, days_cnt+1):
            sale_date = mdt.replace(day=day)
            if sale_date > END: break
            wd = sale_date.weekday()
            dm = 0.65 if wd>=5 else 1.0
            if m==12 and day>=22:            dm*=2.0
            elif m==1 and day<=5:            dm*=1.5
            elif m==2 and day in(13,14,15):  dm*=1.7
            elif m==3 and day==8:            dm*=1.5
            elif m==5 and day in(18,19,20):  dm*=1.4

            for cat in URUN_KATEGORILERI:
                for pk,*rest in cat["products"]:
                    info = prod_map[pk]
                    gf   = info["growth"]**(mi/12.0)
                    exp  = info["daily"]*seasonal*gf*dm
                    qty  = max(0,round(random.gauss(exp,exp*0.28)))
                    if qty==0: continue

                    sid = mon_stk.get((mi,pk))
                    if not sid: continue
                    avq = stk_in[sid]-stk_sold[sid]
                    if avq<=0:
                        prev = mon_stk.get((mi-1,pk)) if mi>0 else None
                        if prev and (stk_in[prev]-stk_sold[prev])>0:
                            sid=prev; avq=stk_in[prev]-stk_sold[prev]
                        else: continue

                    qty = min(qty,avq)
                    if qty<=0: continue

                    rn = random.random()
                    if rn<0.42:
                        ptype=NAKIT;      cust_id=None
                    elif rn<0.72:
                        ptype=KREDI_KART; cust_id=None
                    else:
                        ptype   = VERESIYE if cr_custs else NAKIT
                        cust_id = random.choice(cr_custs) if cr_custs else None

                    if cust_id is None and av and random.random()<0.20:
                        cust_id = random.choice(av)[0]

                    unit_cost  = r2(info["cost"]*(1.018**(mi//3)))
                    sale_price = r2(info["price"]*random.uniform(0.97,1.04))
                    total_amt  = r2(qty*sale_price)

                    if ptype==VERESIYE and cust_id:
                        cust_bal[cust_id] = cust_bal.get(cust_id,0.0)+total_amt

                    sale_rows.append((str(uuid.uuid4()),user_id,info["id"],sid,cust_id,
                                      qty,sale_price,unit_cost,total_amt,ptype,sale_date))
                    stk_sold[sid]+=qty

    print(f"  Hammadde stok : {len(hm_rows):,} kayıt yazılıyor...")
    exmany(conn,"""INSERT INTO Stocks (Id,UserId,ProductId,InflowQuantity,RemainingQuantity,UnitCost,InflowDate,CreatedAt,UpdatedAt)
        VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s)""", hm_rows)

    print(f"  Üretim logu   : {len(prod_rows):,} kayıt yazılıyor...")
    exmany(conn,"INSERT INTO ProductionLogs (Id,UserId,ProductId,Quantity,TotalCost,ProducedAt) VALUES (%s,%s,%s,%s,%s,%s)", prod_rows)

    print(f"  Ürün stoku    : {len(fp_rows):,} kayıt yazılıyor...")
    exmany(conn,"""INSERT INTO Stocks (Id,UserId,ProductId,InflowQuantity,RemainingQuantity,UnitCost,InflowDate,CreatedAt,UpdatedAt)
        VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s)""", fp_rows)

    print(f"  Satışlar      : {len(sale_rows):,} kayıt yazılıyor...")
    exmany(conn,"""INSERT INTO Sales (Id,UserId,ProductId,StockId,CustomerId,Quantity,SalePrice,UnitCostAtSale,TotalAmount,PaymentType,SaleDate)
        VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s)""", sale_rows)

    # Stok kalan miktarları
    for sid,inflow in stk_in.items():
        ex(conn,"UPDATE Stocks SET RemainingQuantity=%s WHERE Id=%s",(max(0,inflow-stk_sold.get(sid,0)),sid))

    # Müşteri bakiyeleri
    for cid,bal in cust_bal.items():
        if bal>0:
            ex(conn,"UPDATE Customers SET CurrentBalance=%s WHERE Id=%s",(r2(bal),cid))

    conn.commit()

    # ── Özet ─────────────────────────────────────────────────────
    total_rev = sum(r[8] for r in sale_rows)
    by_type   = {NAKIT:0.0,KREDI_KART:0.0,VERESIYE:0.0}
    for r in sale_rows: by_type[r[9]] += r[8]

    print()
    print("═"*52)
    print("  ÖZET")
    print("═"*52)
    print(f"  Giriş       : {DEMO_EMAIL} / {DEMO_PASSWORD}")
    print(f"  İşletme     : Demir Süpermarket & Fırın A.Ş.")
    print(f"  Kategori    : {len(URUN_KATEGORILERI)+len(HAMMADDE_KATEGORILERI)}")
    print(f"  Ürün        : {len(prod_map)+len(hm_map)}")
    print(f"  Müşteri     : {len(customers)}")
    print(f"  Personel    : {len(employees)}")
    print(f"  Gider       : {len(exp_rows):,}")
    print(f"  Üretim logu : {len(prod_rows):,}")
    print(f"  Satış       : {len(sale_rows):,}")
    print(f"  Toplam ciro : {total_rev:>14,.0f} TL")
    print(f"    Nakit     : {by_type[NAKIT]:>14,.0f} TL")
    print(f"    Krd.Kartı : {by_type[KREDI_KART]:>14,.0f} TL")
    print(f"    Veresiye  : {by_type[VERESIYE]:>14,.0f} TL")
    print("═"*52)

# ── Giriş noktası ─────────────────────────────────────────────────────────────

def main():
    p = argparse.ArgumentParser()
    p.add_argument("--api-url",  default="http://stok_api:8080", help="API base URL")
    p.add_argument("--db-host",  default="mysql_server")
    p.add_argument("--db-port",  default=3306, type=int)
    p.add_argument("--db-name",  default="stok_takip_db")
    p.add_argument("--db-user",  default="root")
    p.add_argument("--db-pass",  default="123Baran")
    a = p.parse_args()

    print(f"API  → {a.api_url}")
    print(f"DB   → {a.db_user}@{a.db_host}:{a.db_port}/{a.db_name}")

    # API hazır olana kadar bekle
    print("\nAPI bekleniyor", end="", flush=True)
    for _ in range(30):
        try:
            requests.get(f"{a.api_url}/api/auth/login", timeout=3)
            break
        except:
            print(".", end="", flush=True)
            time.sleep(2)
    print(" hazır!")

    try:
        conn = db_connect(a.db_host, a.db_port, a.db_name, a.db_user, a.db_pass)
    except Exception as e:
        sys.exit(f"DB bağlantı hatası: {e}")

    try:
        seed(a.api_url, conn)
        print("\n✓ Tüm veriler başarıyla yüklendi!")
    except Exception as e:
        conn.rollback()
        import traceback; traceback.print_exc()
        sys.exit(f"\n✗ Hata: {e}")
    finally:
        conn.close()

if __name__ == "__main__":
    main()
