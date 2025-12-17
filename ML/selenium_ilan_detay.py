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
from datetime import datetime

# Encoding sorununu çöz
import io
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')

# CHECKPOINT SİSTEMİ
CHECKPOINT_FILE = "checkpoint_detay.json"
CSV_FILE = "manisa_satilik_detayli.csv"
INPUT_CSV = "manisa_satilik_ilce_bazli.csv"  # Linklerin bulunduğu CSV

def load_checkpoint():
    """Checkpoint dosyasından son durumu yükle, CSV'den mevcut işlenmiş linkleri de kontrol et"""
    checkpoint = None
    
    # Önce checkpoint dosyasını kontrol et
    if os.path.exists(CHECKPOINT_FILE):
        try:
            with open(CHECKPOINT_FILE, 'r', encoding='utf-8') as f:
                checkpoint = json.load(f)
                print(f"\n✅ Checkpoint bulundu!")
                print(f"   İşlenen ilan sayısı: {checkpoint.get('processed_count', 0)}")
                print(f"   Toplam ilan: {checkpoint.get('total_listings', 0)}")
        except:
            checkpoint = None
    
    # CSV'den mevcut işlenmiş linkleri kontrol et
    processed_links_from_csv = set()
    if os.path.exists(CSV_FILE):
        try:
            df_existing = pd.read_csv(CSV_FILE, encoding='utf-8-sig')
            if 'Link' in df_existing.columns:
                processed_links_from_csv = set(df_existing['Link'].dropna().unique())
                print(f"✅ CSV'den {len(processed_links_from_csv)} mevcut işlenmiş link yüklendi")
        except Exception as e:
            print(f"⚠️  CSV okuma hatası: {e}")
    
    # Checkpoint'teki linkleri de ekle
    if checkpoint and 'processed_links' in checkpoint:
        processed_links_from_csv.update(checkpoint.get('processed_links', []))
    
    # Checkpoint'i güncelle
    if checkpoint:
        checkpoint['processed_links'] = list(processed_links_from_csv)
        checkpoint['processed_count'] = len(processed_links_from_csv)
    else:
        checkpoint = {
            'processed_count': len(processed_links_from_csv),
            'total_listings': 0,
            'processed_links': list(processed_links_from_csv),
            'last_update': datetime.now().isoformat()
        }
    
    return checkpoint

def save_checkpoint(processed_count, total_listings, processed_links):
    """Checkpoint dosyasına mevcut durumu kaydet"""
    checkpoint = {
        'processed_count': processed_count,
        'total_listings': total_listings,
        'processed_links': list(processed_links),
        'last_update': datetime.now().isoformat()
    }
    try:
        with open(CHECKPOINT_FILE, 'w', encoding='utf-8') as f:
            json.dump(checkpoint, f, ensure_ascii=False, indent=2)
    except Exception as e:
        print(f"  Checkpoint kaydetme hatasi: {e}")

def save_to_csv_incremental(new_data):
    """Her ilan işlendikten sonra CSV'ye ekle"""
    try:
        # Hata mesajı içeren verileri kaydetme
        if "Hata" in new_data and new_data["Hata"]:
            print(f"  ⚠️  Hata içeren veri atlandı, CSV'ye kaydedilmedi")
            return
        
        # Link geçersizse kaydetme
        link = new_data.get("Link", "")
        if not link or link.startswith("Message:") or "invalid session" in str(link).lower() or "Error" in str(link) or "Exception" in str(link):
            print(f"  ⚠️  Geçersiz link, CSV'ye kaydedilmedi")
            return
        
        if not os.path.exists(CSV_FILE):
            # Dosya yoksa, yeni dosya oluştur
            df = pd.DataFrame([new_data])
            df.to_csv(CSV_FILE, index=False, encoding="utf-8-sig", mode='w')
        else:
            # Dosya varsa, append et
            df_new = pd.DataFrame([new_data])
            df_new.to_csv(CSV_FILE, index=False, encoding="utf-8-sig", mode='a', header=False)
    except Exception as e:
        print(f"  CSV kaydetme hatasi: {e}")

def random_wait(min_sec=3, max_sec=8):
    """Random bekleme süresi (bot tespitini önlemek için)"""
    wait_time = random.uniform(min_sec, max_sec)
    time.sleep(wait_time)
    return wait_time

def human_like_scroll(driver):
    """İnsan benzeri scroll hareketi"""
    try:
        # Sayfanın farklı yerlerine scroll et
        scroll_positions = [
            (0, 300),
            (0, 600),
            (0, 900),
            (0, 1200),
            (0, 0)  # En üste dön
        ]
        for x, y in scroll_positions:
            driver.execute_script(f"window.scrollTo({x}, {y});")
            time.sleep(random.uniform(0.5, 1.5))
    except:
        pass

def extract_listing_details(driver, url):
    """İlan detay sayfasından tüm bilgileri çek"""
    details = {}
    
    try:
        # Sayfa yüklensin (JavaScript içeriklerinin yüklenmesi için daha uzun bekle)
        WebDriverWait(driver, 15).until(
            EC.presence_of_element_located((By.TAG_NAME, "body"))
        )
        
        # JavaScript içeriklerinin yüklenmesi için bekle
        time.sleep(random.uniform(3, 5))
        
        # İnsan benzeri scroll
        human_like_scroll(driver)
        time.sleep(random.uniform(1, 2))
        
        # Fiyat elementinin yüklenmesini bekle (konum genellikle fiyatın yanında/altında)
        try:
            WebDriverWait(driver, 10).until(
                EC.presence_of_element_located((By.CSS_SELECTOR, ".price, [class*='price']"))
            )
            time.sleep(1)  # Ek bekleme
        except:
            pass
        
        # ========== TEMEL BİLGİLER ==========
        
        # Başlık
        try:
            baslik = driver.find_element(By.CSS_SELECTOR, "h1, .detail-title, .listing-title").text.strip()
            details["Baslik"] = baslik
        except:
            details["Baslik"] = "-"
        
        # Fiyat
        try:
            fiyat = driver.find_element(By.CSS_SELECTOR, ".price, .detail-price, [class*='price']").text.strip()
            details["Fiyat"] = fiyat
        except:
            details["Fiyat"] = "-"
        
        # Konum (İl, İlçe, Mahalle) - Görselde "Manisa / Yunusemre / Muradiye Mah." formatında
        try:
            # Önce URL'den çıkarmayı dene (URL'de bu bilgiler var)
            il = "Manisa"
            ilce = "-"
            mahalle = "-"
            konum_text = "-"
            
            # URL'den ilçe ve mahalle çıkar
            # Örnek: https://www.hepsiemlak.com/manisa-alasehir-yenice-satilik/daire/160695-51
            # "manisa-alasehir-yenice" -> ilçe: "alasehir", mahalle: "yenice"
            import re
            url_pattern = re.search(r'manisa-([^-]+)-([^-]+)-', url.lower())
            if url_pattern:
                ilce_from_url = url_pattern.group(1).strip()
                mahalle_from_url = url_pattern.group(2).strip()
                # İlk harfi büyük yap
                ilce = ilce_from_url.capitalize()
                mahalle = mahalle_from_url.capitalize() + " Mah."
                konum_text = f"{il} / {ilce} / {mahalle}"
            
            # CSS selector ile konum elementini bul (URL'den bulunamadıysa)
            
            # Konum için farklı selector'ları dene (görselde fiyatın altında)
            konum_selectors = [
                ".location",
                "[class*='location']",
                "[class*='Location']",
                ".detail-location",
                ".property-location",
                # Fiyatın altındaki elementler
                ".price + *",
                ".price ~ *",
                "[class*='price'] + *",
                "[class*='price'] ~ *"
            ]
            
            # Eğer URL'den bulunamadıysa, CSS selector ile dene
            if konum_text == "-" or ilce == "-":
                for selector in konum_selectors:
                    try:
                        konum_elements = driver.find_elements(By.CSS_SELECTOR, selector)
                        for konum_element in konum_elements:
                            text = konum_element.text.strip()
                            # "Manisa / İlçe / Mahalle" formatını kontrol et
                            if text and "Manisa" in text and " / " in text and len(text) < 100:
                                konum_text = text
                                # Parse et
                                parts = text.split(" / ")
                                if len(parts) >= 3:
                                    il = parts[0].strip()
                                    ilce = parts[1].strip()
                                    mahalle = parts[2].strip()
                                elif len(parts) == 2:
                                    il = parts[0].strip()
                                    ilce = parts[1].strip()
                                break
                        if konum_text != "-" and "Manisa" in konum_text and ilce != "-":
                            break
                    except:
                        continue
            
            # Eğer bulunamadıysa, sayfa metninden ara (daha geniş arama)
            if konum_text == "-" or "Manisa" not in konum_text:
                # Biraz daha bekle (JavaScript içerikleri için)
                time.sleep(2)
                page_text = driver.find_element(By.TAG_NAME, "body").text
                import re
                
                # Tüm "Manisa /" içeren satırları bul
                lines = page_text.split('\n')
                for line in lines:
                    line_clean = line.strip()
                    if "Manisa /" in line_clean and len(line_clean) < 100:
                        # "Manisa / İlçe / Mahalle" formatını kontrol et
                        parts = line_clean.split(" / ")
                        if len(parts) >= 3:
                            il_part = parts[0].strip()
                            ilce_part = parts[1].strip()
                            mahalle_part = parts[2].strip()
                            # "Satılık", "Kiralık" gibi kelimeleri temizle
                            ilce_part = re.sub(r'\s*(Satılık|Kiralık)\s*', '', ilce_part).strip()
                            mahalle_part = re.sub(r'\s*(Satılık|Kiralık)\s*', '', mahalle_part).strip()
                            if ilce_part and mahalle_part:
                                konum_text = f"{il_part} / {ilce_part} / {mahalle_part}"
                                break
                        elif len(parts) == 2:
                            il_part = parts[0].strip()
                            ilce_part = parts[1].strip()
                            ilce_part = re.sub(r'\s*(Satılık|Kiralık)\s*', '', ilce_part).strip()
                            if ilce_part:
                                konum_text = f"{il_part} / {ilce_part}"
                                break
                
                # Hala bulunamadıysa regex ile dene
                if konum_text == "-" or "Manisa" not in konum_text:
                    # Daha esnek pattern
                    konum_pattern = re.search(r'Manisa\s*/\s*([^/\n]{2,30}?)\s*/\s*([^/\n]{2,40}?)(?:\s|$|\.|Mah)', page_text)
                    if konum_pattern:
                        ilce_part = konum_pattern.group(1).strip()
                        mahalle_part = konum_pattern.group(2).strip()
                        ilce_part = re.sub(r'\s*(Satılık|Kiralık)\s*', '', ilce_part).strip()
                        mahalle_part = re.sub(r'\s*(Satılık|Kiralık)\s*', '', mahalle_part).strip()
                        if ilce_part and mahalle_part:
                            konum_text = f"Manisa / {ilce_part} / {mahalle_part}"
            
            # Konum metnini parse et
            if " / " in konum_text:
                parts = konum_text.split(" / ")
                if len(parts) >= 3:
                    il = parts[0].strip()
                    ilce = parts[1].strip()
                    mahalle = parts[2].strip()
                elif len(parts) == 2:
                    il = parts[0].strip()
                    ilce = parts[1].strip()
            
            # Temizleme: "Satılık", "Kiralık" gibi kelimeleri kaldır
            if "Satılık" in ilce:
                ilce = ilce.replace("Satılık", "").strip()
            if "Kiralık" in ilce:
                ilce = ilce.replace("Kiralık", "").strip()
            
            details["Il"] = il if il else "Manisa"
            details["Ilce"] = ilce if ilce and ilce != "-" else "-"
            details["Mahalle"] = mahalle if mahalle and mahalle != "-" else "-"
            details["Konum"] = konum_text if konum_text != "-" else f"{il} / {ilce} / {mahalle}" if ilce != "-" else f"{il} / {ilce}" if ilce != "-" else il
            
        except Exception as e:
            details["Konum"] = "-"
            details["Il"] = "Manisa"
            details["Ilce"] = "-"
            details["Mahalle"] = "-"
        
        # ========== ÖZELLİKLER (Hepsiemlak yapısına göre) ==========
        
        # Sayfa metninden direkt çek (daha güvenilir)
        page_text = driver.find_element(By.TAG_NAME, "body").text
        
        # Oda Sayısı
        try:
            oda_match = extract_from_text(page_text, "Oda Sayısı", "Banyo Sayısı")
            details["Oda"] = oda_match if oda_match else "-"
        except:
            details["Oda"] = "-"
        
        # M²
        try:
            m2_match = extract_from_text(page_text, "Brüt / Net M2", "Kat Sayısı")
            if m2_match:
                # Sadece sayıyı al (örn: "120 m2" -> "120")
                import re
                numbers = re.findall(r'\d+', m2_match)
                details["M2"] = numbers[0] if numbers else "-"
            else:
                details["M2"] = "-"
        except:
            details["M2"] = "-"
        
        # Banyo Sayısı
        try:
            banyo_match = extract_from_text(page_text, "Banyo Sayısı", "Brüt / Net M2")
            details["Banyo"] = banyo_match if banyo_match else "-"
        except:
            details["Banyo"] = "-"
        
        # Kat Bilgisi
        try:
            kat_match = extract_from_text(page_text, "Bulunduğu Kat", "Bina Yaşı")
            details["Kat"] = kat_match if kat_match else "-"
        except:
            details["Kat"] = "-"
        
        # Bina Yaşı
        try:
            yas_match = extract_from_text(page_text, "Bina Yaşı", "Isınma Tipi")
            details["Bina_Yasi"] = yas_match if yas_match else "-"
        except:
            details["Bina_Yasi"] = "-"
        
        # Isınma Tipi
        try:
            isinma_match = extract_from_text(page_text, "Isınma Tipi", "Yakıt Tipi")
            # Eğer çok uzunsa, sadece ilk satırı al
            if isinma_match and len(isinma_match) > 50:
                isinma_match = isinma_match.split('\n')[0].strip()
                # Sadece ilk kelimeyi al (örn: "Kombi\nKrediye..." -> "Kombi")
                if '\n' in isinma_match or len(isinma_match) > 30:
                    isinma_match = isinma_match.split()[0] if isinma_match.split() else "-"
            details["Isinma"] = isinma_match if isinma_match and len(str(isinma_match)) < 30 else "-"
        except:
            details["Isinma"] = "-"
        
        # Balkon, Asansör, Garaj kontrolü
        page_text_lower = page_text.lower()
        details["Balkon"] = "Var" if any(x in page_text_lower for x in ["balkon", "balcony"]) else "Yok"
        details["Asansor"] = "Var" if any(x in page_text_lower for x in ["asansör", "asansor", "elevator"]) else "Yok"
        details["Garaj"] = "Var" if any(x in page_text_lower for x in ["garaj", "garage", "otopark", "parking"]) else "Yok"
        
        # ========== DETAY BİLGİLER (Görselde key-value çiftleri halinde) ==========
        
        # Görseldeki detay bilgileri:
        # - İlan Durumu: Satılık
        # - Konut Tipi: Daire
        # - Krediye Uygunlu: Uygun
        # - Tapu Durumu: Kat Mülkiyeti
        # - Eşya Durumu: Eşyalı Değil
        # - Kullanım Durumu: Boş
        # - Takas: Evet
        
        detail_keys = [
            ("Yapı Durumu", ["Yapının Durumu", "Yapı Durumu"]),
            ("Kullanım Durumu", ["Kullanım Durumu"]),
            ("Tapu Durumu", ["Tapu Durumu"]),
            ("Cephe", ["Cephe"]),
            ("Yön", ["Yön"]),
            ("Aidat", ["Aidat"]),
            ("Krediye Uygun", ["Krediye Uygunlu", "Krediye Uygun"]),
            ("Takas", ["Takas"]),
            ("Site İçinde", ["Site İçinde", "Site"]),
            ("Güvenlik", ["Güvenlik"]),
            ("Eşya Durumu", ["Eşya Durumu"]),
            ("İlan Durumu", ["İlan Durumu"])
        ]
        
        for key_name, key_variants in detail_keys:
            value = "-"
            for variant in key_variants:
                value = extract_detail_value(driver, variant)
                if value != "-" and len(value) < 50:  # Çok uzunsa geçersiz
                    break
            details[key_name] = value
        
        # ========== DİĞER BİLGİLER ==========
        
        # İlan No - Sayfa metninden çek
        try:
            page_text = driver.find_element(By.TAG_NAME, "body").text
            ilan_no_match = extract_from_text(page_text, "İlan no", "Son Güncelleme")
            if not ilan_no_match:
                # Alternatif format
                import re
                ilan_no_pattern = re.search(r'(\d{6}-\d+)', page_text)
                if ilan_no_pattern:
                    ilan_no_match = ilan_no_pattern.group(1)
            details["Ilan_No"] = ilan_no_match if ilan_no_match else "-"
        except:
            details["Ilan_No"] = "-"
        
        # İlan Tarihi / Son Güncelleme
        try:
            page_text = driver.find_element(By.TAG_NAME, "body").text
            tarih_match = extract_from_text(page_text, "Son Güncelleme", "İlan Durumu")
            if not tarih_match:
                # Alternatif: Tarih formatını ara (DD-MM-YYYY)
                import re
                tarih_pattern = re.search(r'(\d{2}-\d{2}-\d{4})', page_text)
                if tarih_pattern:
                    tarih_match = tarih_pattern.group(1)
            details["Ilan_Tarihi"] = tarih_match if tarih_match else "-"
        except:
            details["Ilan_Tarihi"] = "-"
        
        # Link
        details["Link"] = url
        
        # Çekilme Tarihi
        details["Cekilme_Tarihi"] = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
        
    except Exception as e:
        error_msg = str(e)
        print(f"    Hata: {error_msg[:100]}...")  # İlk 100 karakteri göster
        details["Hata"] = error_msg
        # Temel alanları boş bırak
        if "Baslik" not in details:
            details["Baslik"] = "-"
        if "Fiyat" not in details:
            details["Fiyat"] = "-"
        if "Link" not in details:
            details["Link"] = url
        if "Cekilme_Tarihi" not in details:
            details["Cekilme_Tarihi"] = datetime.now().strftime("%Y-%m-%d %H:%M:%S")
    
    return details

def extract_from_text(text, start_key, end_key=None):
    """İki key arasındaki metni çıkar"""
    if not text:
        return None
    
    start_idx = text.find(start_key)
    if start_idx == -1:
        return None
    
    # Start key'den sonraki kısmı al
    after_start = text[start_idx + len(start_key):].strip()
    
    if end_key:
        # End key'e kadar al
        end_idx = after_start.find(end_key)
        if end_idx != -1:
            result = after_start[:end_idx].strip()
        else:
            # End key bulunamazsa, ilk satırı veya ilk 50 karakteri al
            lines = after_start.split('\n')
            result = lines[0].strip() if lines and lines[0].strip() else after_start[:50].strip()
    else:
        # İlk satıra kadar al
        lines = after_start.split('\n')
        result = lines[0].strip() if lines else after_start[:50].strip()
    
    # Çok uzunsa kısalt (max 50 karakter)
    if len(result) > 50:
        # İlk satırı al veya ilk 50 karakteri
        lines = result.split('\n')
        result = lines[0].strip()[:50] if lines else result[:50]
    
    # Boş veya sadece whitespace ise None döndür
    if not result or result.isspace():
        return None
    
    return result

def extract_detail_value(driver, key):
    """Detay sayfasından belirli bir key'in value'sunu çıkar (görseldeki yapıya göre)"""
    try:
        # Yöntem 1: Sayfa metninden direkt çek (en güvenilir)
        page_text = driver.find_element(By.TAG_NAME, "body").text
        
        # Key'i içeren satırı bul
        import re
        # "Key: Value" veya "Key\nValue" formatını ara
        pattern1 = re.search(rf'{re.escape(key)}\s*[:：]\s*([^\n]+)', page_text, re.IGNORECASE)
        if pattern1:
            value = pattern1.group(1).strip()
            if value and len(value) < 50:
                return value
        
        # Yöntem 2: Key'den sonraki ilk satırı al
        key_idx = page_text.find(key)
        if key_idx != -1:
            after_key = page_text[key_idx + len(key):].strip()
            # İlk satırı al
            first_line = after_key.split('\n')[0].strip()
            # ":" varsa ondan sonrasını al
            if ':' in first_line:
                first_line = first_line.split(':')[1].strip()
            if first_line and len(first_line) < 50 and first_line != key:
                return first_line
        
        # Yöntem 3: HTML element yapısından çek
        try:
            # dt/dd yapısı
            dt = driver.find_element(By.XPATH, f"//dt[contains(text(), '{key}')]")
            dd = dt.find_element(By.XPATH, "./following-sibling::dd[1]")
            value = dd.text.strip()
            if value and len(value) < 50:
                return value
        except:
            pass
        
        # Yöntem 4: th/td yapısı
        try:
            th = driver.find_element(By.XPATH, f"//th[contains(text(), '{key}')]")
            td = th.find_element(By.XPATH, "./following-sibling::td[1]")
            value = td.text.strip()
            if value and len(value) < 50:
                return value
        except:
            pass
            
    except:
        pass
    
    return "-"

print("="*60)
print("İLAN DETAY ÇEKME PROGRAMI")
print("="*60)

# Checkpoint kontrolü
checkpoint = load_checkpoint()
processed_links = set(checkpoint.get('processed_links', [])) if checkpoint else set()

# Input CSV'yi oku
if not os.path.exists(INPUT_CSV):
    print(f"\n❌ HATA: {INPUT_CSV} dosyasi bulunamadi!")
    print("Önce liste sayfasından linkleri çekmeniz gerekiyor.")
    sys.exit(1)

print(f"\n{INPUT_CSV} dosyasi okunuyor...")
df_links = pd.read_csv(INPUT_CSV, encoding='utf-8-sig')
total_listings = len(df_links)

# İşlenmiş link sayısını hesapla
processed_count = len(processed_links)
print(f"✅ Toplam {total_listings} ilan bulundu")
print(f"✅ {processed_count} ilan zaten işlenmiş")
print(f"✅ {total_listings - processed_count} ilan işlenecek")

# Tarayıcı ayarları
print("\nTarayıcı başlatılıyor...")
options = webdriver.ChromeOptions()
options.add_argument("--start-maximized")
options.add_argument("--disable-blink-features=AutomationControlled")
options.add_experimental_option("excludeSwitches", ["enable-automation"])
options.add_experimental_option('useAutomationExtension', False)

# Tarayıcı başlatma fonksiyonu (options parametresi ile)
def init_driver(chrome_options):
    """Tarayıcıyı başlat ve döndür"""
    try:
        driver = webdriver.Chrome(service=Service(ChromeDriverManager().install()), options=chrome_options)
        driver.set_page_load_timeout(60)  # 60 saniye timeout
        print("✅ Tarayıcı başarıyla başlatıldı!")
        return driver
    except Exception as e:
        print(f"❌ ChromeDriver hatası: {e}")
        return None

# ChromeDriver başlat
driver = init_driver(options)
if not driver:
    sys.exit(1)

# İlanları işle
processed_count = len(processed_links)
success_count = 0
error_count = 0
skipped_count = 0

print(f"\n{'='*60}")
print(f"İŞLEME BAŞLIYOR...")
print(f"{'='*60}\n")

try:
    for idx, row in df_links.iterrows():
        url = row.get('Link', '')
        if not url or url == '-':
            continue
        
        if url in processed_links:
            skipped_count += 1
            if skipped_count % 50 == 0:  # Her 50'de bir göster
                print(f"[{idx+1}/{total_listings}] Zaten işlenmiş, atlanıyor... (Toplam atlanan: {skipped_count})")
            continue
        
        print(f"\n[{idx+1}/{total_listings}] İşleniyor: {url[:60]}...")
        
        try:
            # Tarayıcı bağlantısını kontrol et
            try:
                driver.current_url  # Bağlantıyı test et
            except Exception as conn_error:
                if "invalid session" in str(conn_error).lower() or "disconnected" in str(conn_error).lower():
                    print(f"  ⚠️  Tarayıcı bağlantısı kesildi, yeniden başlatılıyor...")
                    try:
                        driver.quit()
                    except:
                        pass
                    time.sleep(5)
                    driver = init_driver(options)
                    if not driver:
                        print(f"  ❌ Tarayıcı yeniden başlatılamadı, atlanıyor...")
                        error_count += 1
                        continue
            
            # Sayfaya git
            driver.get(url)
            
            # Random bekleme
            wait_time = random_wait(3, 8)
            print(f"  ⏳ Bekleniyor ({wait_time:.1f} sn)...")
            
            # Detayları çek
            details = extract_listing_details(driver, url)
            
            # Hata kontrolü
            if "Hata" in details and details["Hata"]:
                print(f"  ⚠️  Veri çekme hatası: {details['Hata']}")
                error_count += 1
                # Hata olsa bile checkpoint güncelle (tekrar denememek için)
                processed_links.add(url)
                processed_count += 1
                save_checkpoint(processed_count, total_listings, processed_links)
                continue
            
            # CSV'ye kaydet
            save_to_csv_incremental(details)
            
            # Checkpoint güncelle
            processed_links.add(url)
            processed_count += 1
            save_checkpoint(processed_count, total_listings, processed_links)
            
            success_count += 1
            print(f"  ✅ Başarılı! (Başarılı: {success_count}, Hata: {error_count})")
            
            # Periyodik molalar
            if (idx + 1) % 10 == 0:
                long_wait = random.uniform(15, 30)
                print(f"\n  🛑 10 ilan tamamlandı, {long_wait:.1f} saniye mola...")
                time.sleep(long_wait)
            
            if (idx + 1) % 50 == 0:
                very_long_wait = random.uniform(60, 120)
                print(f"\n  🛑 50 ilan tamamlandı, {very_long_wait:.1f} saniye uzun mola...")
                time.sleep(very_long_wait)
                
        except Exception as e:
            error_count += 1
            error_msg = str(e)
            print(f"  ❌ Hata: {error_msg[:100]}...")  # İlk 100 karakteri göster
            
            # Tarayıcı bağlantı hatası ise yeniden başlat
            if "invalid session" in error_msg.lower() or "disconnected" in error_msg.lower():
                print(f"  ⚠️  Tarayıcı bağlantı hatası, yeniden başlatılıyor...")
                try:
                    driver.quit()
                except:
                    pass
                time.sleep(5)
                driver = init_driver(options)
                if not driver:
                    print(f"  ❌ Tarayıcı yeniden başlatılamadı!")
            
            # Hata olsa bile checkpoint güncelle (tekrar denememek için)
            processed_links.add(url)
            processed_count += 1
            save_checkpoint(processed_count, total_listings, processed_links)
            continue

except KeyboardInterrupt:
    print("\n\n⚠️  Program kullanıcı tarafından durduruldu!")
    print("✅ Mevcut veriler kaydedildi. Checkpoint kaydedildi.")

finally:
    print("\n" + "="*60)
    print("SONUÇ ÖZETİ")
    print("="*60)
    print(f"Toplam işlenen: {processed_count}/{total_listings}")
    print(f"Başarılı: {success_count}")
    print(f"Hata: {error_count}")
    print(f"\n✅ Veriler '{CSV_FILE}' dosyasına kaydedildi.")
    print("✅ Checkpoint kaydedildi.")
    
    driver.quit()
    print("✅ Tarayıcı kapatıldı.")

