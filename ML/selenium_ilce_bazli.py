from selenium import webdriver
from selenium.webdriver.chrome.service import Service
from webdriver_manager.chrome import ChromeDriverManager
from selenium.webdriver.common.by import By
from selenium.webdriver.support.ui import WebDriverWait
from selenium.webdriver.support import expected_conditions as EC
from selenium.common.exceptions import WebDriverException, TimeoutException, NoSuchElementException
import time
import pandas as pd
import sys
import os
import json
import random
import re

# Encoding sorununu çöz
import io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

# CHECKPOINT SİSTEMİ
CHECKPOINT_FILE = "checkpoint_ilce.json"
CSV_FILE = "manisa_satilik_ilce_bazli.csv"

# Manisa ilçeleri (URL'den çıkarılacak)
MANISA_ILCELER = [
    "yunusemre", "sehzadeler", "akhisar", "turgutlu", "salihli",
    "soma", "alasehir", "saruhanli", "kula", "demirci",
    "kirkagac", "golmarmara", "gordes", "sarigol"
]

def load_checkpoint():
    """Checkpoint dosyasından son durumu yükle, yoksa CSV'den mevcut linkleri oku"""
    checkpoint = None
    
    # Önce checkpoint dosyasını kontrol et
    if os.path.exists(CHECKPOINT_FILE):
        try:
            with open(CHECKPOINT_FILE, 'r', encoding='utf-8') as f:
                checkpoint = json.load(f)
        except:
            checkpoint = None
    
    # CSV'den mevcut linkleri oku (checkpoint yoksa veya CSV'de daha fazla link varsa)
    existing_links = set()
    if os.path.exists(CSV_FILE):
        try:
            df = pd.read_csv(CSV_FILE, encoding='utf-8-sig')
            if 'Link' in df.columns:
                existing_links = set(df['Link'].dropna().unique())
                print(f"✅ CSV'den {len(existing_links)} mevcut link yüklendi")
        except Exception as e:
            print(f"⚠️  CSV okuma hatası: {e}")
    
    # Checkpoint'teki linkleri de ekle
    if checkpoint and 'all_links' in checkpoint:
        existing_links.update(checkpoint.get('all_links', []))
    
    # Checkpoint'i güncelle veya oluştur
    if checkpoint:
        checkpoint['all_links'] = list(existing_links)
        checkpoint['total_listings'] = len(existing_links)
    else:
        checkpoint = {
            'last_page': 1,
            'all_links': list(existing_links),
            'total_listings': len(existing_links)
        }
    
    return checkpoint

def save_checkpoint(page_number, all_links):
    """Checkpoint dosyasına mevcut durumu kaydet"""
    checkpoint = {
        'last_page': page_number,
        'all_links': list(all_links),
        'total_listings': len(all_links)
    }
    try:
        with open(CHECKPOINT_FILE, 'w', encoding='utf-8') as f:
            json.dump(checkpoint, f, ensure_ascii=False, indent=2)
    except Exception as e:
        print(f"Checkpoint kaydetme hatasi: {e}")

def save_to_csv_incremental(new_data):
    """Her sayfadan sonra CSV'ye ekle"""
    try:
        if not os.path.exists(CSV_FILE):
            df = pd.DataFrame(new_data)
            df.to_csv(CSV_FILE, index=False, encoding="utf-8-sig", mode='w')
        else:
            df_new = pd.DataFrame(new_data)
            df_new.to_csv(CSV_FILE, index=False, encoding="utf-8-sig", mode='a', header=False)
    except Exception as e:
        print(f"CSV kaydetme hatasi: {e}")

def extract_ilce_from_url(url):
    """URL'den ilçe bilgisini çıkar"""
    if not url:
        return "-"
    # URL formatı: manisa-{ilce}-{mahalle}-satilik
    match = re.search(r'manisa-([^-]+)-', url.lower())
    if match:
        ilce = match.group(1).strip()
        # İlçe adını düzelt
        ilce_map = {
            "yunusemre": "Yunusemre",
            "sehzadeler": "Şehzadeler",
            "akhisar": "Akhisar",
            "turgutlu": "Turgutlu",
            "salihli": "Salihli",
            "soma": "Soma",
            "alasehir": "Alaşehir",
            "saruhanli": "Saruhanlı",
            "kula": "Kula",
            "demirci": "Demirci",
            "kirkagac": "Kırkağaç",
            "golmarmara": "Gölmarmara",
            "gordes": "Gördes",
            "sarigol": "Sarıgöl"
        }
        return ilce_map.get(ilce, ilce.capitalize())
    return "-"

print("="*60)
print("GENEL SAYFADAN İLAN ÇEKME (İLÇE BİLGİSİ URL'DEN)")
print("="*60)

# Checkpoint kontrolü
checkpoint = load_checkpoint()
start_from_page = checkpoint.get('last_page', 1) + 1 if checkpoint else 1
existing_seen_links = set(checkpoint.get('all_links', [])) if checkpoint else set()

print(f"\nBaşlangıç sayfası: {start_from_page}")
print(f"Mevcut toplam ilan: {len(existing_seen_links)}")

# Tarayıcı başlat
print("\nTarayıcı başlatılıyor...")
options = webdriver.ChromeOptions()
options.add_argument("--start-maximized")
options.add_argument("--disable-blink-features=AutomationControlled")
options.add_experimental_option("excludeSwitches", ["enable-automation"])
options.add_experimental_option('useAutomationExtension', False)

try:
    driver = webdriver.Chrome(service=Service(ChromeDriverManager().install()), options=options)
    print("✅ Tarayıcı başarıyla başlatıldı!")
except Exception as e:
    print(f"❌ ChromeDriver hatası: {e}")
    sys.exit(1)

# Genel sayfaya git
url = "https://www.hepsiemlak.com/manisa-satilik"
print(f"\nSiteye gidiliyor: {url}")

try:
    driver.set_page_load_timeout(180)
    driver.get(url)
    print(f"✅ Sayfa yüklendi: {driver.title}")
    time.sleep(5)
except Exception as e:
    print(f"❌ Sayfa yüklenirken hata: {e}")
    driver.quit()
    sys.exit(1)

# İlanları çek
veri_listesi = []
seen_links = existing_seen_links.copy()
page_number = start_from_page
max_pages = 86  # Sitede maksimum 86 sayfa görüntülenebiliyor
duplicate_count = 0
no_new_listings_count = 0

print(f"\n{'='*60}")
print("İLAN ÇEKME BAŞLADI")
print(f"{'='*60}")

while page_number <= max_pages:
    print(f"\n--- Sayfa {page_number} işleniyor ---")
    
    # Sayfanın yüklenmesi için bekle
    time.sleep(3)
    
    # Scroll (lazy loading için)
    driver.execute_script("window.scrollTo(0, document.body.scrollHeight/2);")
    time.sleep(1)
    driver.execute_script("window.scrollTo(0, document.body.scrollHeight);")
    time.sleep(2)
    
    # İlanları bul
    current_listings = driver.find_elements(By.CLASS_NAME, "listing-item")
    if len(current_listings) == 0:
        current_listings = driver.find_elements(By.CSS_SELECTOR, "li.listing-item")
    
    print(f"  {len(current_listings)} ilan bulundu")
    
    if len(current_listings) == 0:
        print("  Bu sayfada ilan bulunamadi, durduruluyor...")
        break
    
    # İlanları işle
    page_ilan_count = 0
    page_new_ilanlar = []
    
    for ilan in current_listings:
        try:
            # Başlık ve Link
            baslik_element = ilan.find_elements(By.CSS_SELECTOR, ".card-link, a.img-link")
            link_url = None
            baslik = "-"
            
            if baslik_element:
                baslik = baslik_element[0].get_attribute("title") or baslik_element[0].text
                link_url = baslik_element[0].get_attribute("href")
                
                if link_url and not link_url.startswith("http"):
                    link_url = "https://www.hepsiemlak.com" + link_url
                
                # Duplicate kontrolü
                if link_url and link_url in seen_links:
                    duplicate_count += 1
                    continue
                
                if link_url:
                    seen_links.add(link_url)
            else:
                continue
            
            # Fiyat
            fiyat_element = ilan.find_elements(By.CSS_SELECTOR, ".list-view-price, .price-val")
            fiyat = fiyat_element[0].text.strip() if fiyat_element else "-"
            
            # Oda
            oda_element = ilan.find_elements(By.CSS_SELECTOR, ".houseRoomCount")
            oda = oda_element[0].text.strip() if oda_element else "-"
            
            # M2
            m2_element = ilan.find_elements(By.CSS_SELECTOR, ".squareMeter")
            m2 = m2_element[0].text.strip() if m2_element else "-"
            
            # Konum
            konum_element = ilan.find_elements(By.CSS_SELECTOR, ".list-view-location, .location")
            konum = konum_element[0].text.strip() if konum_element else "-"
            
            # URL'den ilçe çıkar
            ilce = extract_ilce_from_url(link_url)
            
            row = {
                "Baslik": baslik,
                "Link": link_url,
                "Fiyat": fiyat,
                "Oda": oda,
                "M2": m2,
                "Konum": konum,
                "Ilce": ilce
            }
            
            page_new_ilanlar.append(row)
            veri_listesi.append(row)
            page_ilan_count += 1
            
        except Exception as e:
            continue
    
    print(f"  ✅ {page_ilan_count} yeni ilan eklendi (Toplam: {len(seen_links)}, Duplicate: {duplicate_count})")
    
    # CSV'ye kaydet
    if page_new_ilanlar:
        save_to_csv_incremental(page_new_ilanlar)
        no_new_listings_count = 0
    else:
        no_new_listings_count += 1
        if no_new_listings_count >= 3:
            print("  ⚠️  3 sayfada yeni ilan bulunamadı, durduruluyor...")
            break
    
    # Checkpoint kaydet
    save_checkpoint(page_number, seen_links)
    
    # Sonraki sayfa
    try:
        current_url = driver.current_url
        if "page=" in current_url:
            new_url = re.sub(r'page=\d+', f'page={page_number + 1}', current_url)
        else:
            separator = "&" if "?" in current_url else "?"
            new_url = f"{current_url}{separator}page={page_number + 1}"
        
        driver.get(new_url)
        time.sleep(3)
    except Exception as e:
        print(f"  ❌ Sonraki sayfaya geçilemedi: {e}")
        break
    
    page_number += 1
    
    # Her 10 sayfada bir mola
    if page_number % 10 == 0:
        wait = random.uniform(10, 20)
        print(f"\n  🛑 10 sayfa tamamlandi, {wait:.1f} saniye mola...")
        time.sleep(wait)
    
    # Hedef sayıya ulaşıldı mı?
    if len(seen_links) >= 2000:
        print(f"\n  🎉 Hedef sayıya ulaşıldı! ({len(seen_links)} ilan)")
        break

driver.quit()

print("\n" + "="*60)
print("SONUÇ")
print("="*60)
print(f"Toplam {page_number - 1} sayfa işlendi")
print(f"Toplam {len(seen_links)} benzersiz ilan bulundu")
print(f"Duplicate sayısı: {duplicate_count}")
print(f"✅ Veriler '{CSV_FILE}' dosyasına kaydedildi")
