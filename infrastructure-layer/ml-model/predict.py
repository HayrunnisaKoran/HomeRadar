# -*- coding: utf-8 -*-

import sys
import json
import pandas as pd
import joblib
import numpy as np

import io
import locale
import os

# ENCODING FIX - Tüm encoding'leri UTF-8 yap
sys.stdin = io.TextIOWrapper(sys.stdin.buffer, encoding='utf-8')
sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8')
sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8')

# Locale'i de UTF-8 yap
locale.setlocale(locale.LC_ALL, 'tr_TR.UTF-8')

print(f"DEBUG: Python encoding: {sys.getdefaultencoding()}", file=sys.stderr)
print(f"DEBUG: Filesystem encoding: {sys.getfilesystemencoding()}", file=sys.stderr)

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
print(f"DEBUG: Working in: {SCRIPT_DIR}", file=sys.stderr)

# 1. MODEL� VE S�TUN �S�MLER�N� Y�KLE
try:
    model = joblib.load('manisa_ev_fiyat_modeli_v1_060.pkl')
    # model_columns.pkl dosyas�n� olu�turmad�ysan, a�a��daki listeyi kullan�r�z ama do�rusu pkl'den �ekmektir.
    try:
        model_columns = joblib.load('model_columns.pkl')
    except:
        # Acil durum: E�er pkl yoksa  listeyi elle tan�ml�yorum
        model_columns = ['M2', 'Banyo', 'Bina_Yasi', 'Balkon', 'Asansor', 'Garaj', 'Takas', 'E�ya Durumu', 
                         'oda_sayisi', 'salon_sayisi', 'Kat', 'Isinma', 'yapi_durumu', 
                         'Kullanim_Bo�', 'Kullanim_Kirac�l�', 'Kullanim_M�lk Sahibi', 'tapu_durumu', 
                         'Ilce_Akhisar', 'Ilce_Alasehir', 'Ilce_Demirci', 'Ilce_Gordes', 'Ilce_Kirkagac', 
                         'Ilce_Koprubasi', 'Ilce_Kula', 'Ilce_Salihli', 'Ilce_Sarigol', 'Ilce_Saruhanli', 
                         'Ilce_Sehzadeler', 'Ilce_Selendi', 'Ilce_Soma', 'Ilce_Turgutlu', 'Ilce_Yunusemre', 
                         'Oda_Buyuklugu', 'Luks_Skoru']

except FileNotFoundError:
    print(json.dumps({"status": "error", "message": "Model dosyalari (pkl) bulunamadi."}))
    sys.exit(1)

def tahmin_et():
    try:
        print(f"DEBUG: Starting tahmin_et()", file=sys.stderr)
        print(f"DEBUG: sys.argv: {sys.argv}", file=sys.stderr)
        print(f"DEBUG: Number of args: {len(sys.argv)}", file=sys.stderr)
        
        # 2. NODE.JS'DEN GELEN VERİYİ OKU
        if len(sys.argv) > 1:
            input_json = sys.argv[1]
            print(f"DEBUG: Input JSON length: {len(input_json)}", file=sys.stderr)
            print(f"DEBUG: Input JSON first 100 chars: {input_json[:100]}", file=sys.stderr)
        else:
            print(f"DEBUG: No args, reading from stdin", file=sys.stderr)
            input_json = sys.stdin.read()
            print(f"DEBUG: STDIN length: {len(input_json)}", file=sys.stderr)
        
        # JSON'ı parse etmeden önce kontrol et
        if not input_json or input_json.strip() == '':
            print(f"DEBUG: Empty input!", file=sys.stderr)
            error_result = {"status": "error", "message": "Empty input received"}
            print(json.dumps(error_result))
            return
        
        input_data = json.loads(input_json)
        print(f"DEBUG: JSON parsed successfully", file=sys.stderr)
        print(f"DEBUG: Keys in input: {list(input_data.keys())}", file=sys.stderr)
        

        # 3. VER�LER� HAZIRLA (Varsay�lan de�erlerle)
        # Node.js'ten �ngilizce key'ler gelecek, biz senin T�rk�e s�tunlar�na �evirece�iz.
        
        # Temel Veriler
        data = {
            'M2': [float(input_data.get('square_meters', 100))],
            'Banyo': [int(input_data.get('bathrooms', 1))],
            'Bina_Yasi': [int(input_data.get('building_age', 5))],
            'Balkon': [int(input_data.get('balcony', 1))],          # Var:1, Yok:0
            'Asansor': [int(input_data.get('elevator', 0))],        # Var:1, Yok:0
            'Garaj': [int(input_data.get('garage', 0))],            # Var:1, Yok:0
            'Takas': [int(input_data.get('swap', 0))],              # Var:1, Yok:0
            'E�ya Durumu': [int(input_data.get('furnished', 0))],   # E�yal�:1, Bo�:0
            'oda_sayisi': [int(input_data.get('rooms', 3))],
            'salon_sayisi': [int(input_data.get('living_rooms', 1))],
            'Kat': [int(input_data.get('floor', 2))],
            'Isinma': [int(input_data.get('heating', 1))],          # Do�algaz:1, Di�er:0 (�rnek)
            'yapi_durumu': [int(input_data.get('building_status', 0))], # S�f�r:0, �kinci El:1 (Mant���na g�re de�i�ir)
            'tapu_durumu': [int(input_data.get('title_deed', 1))]   # Kat M�lkiyeti vb.
        }
        
        df = pd.DataFrame(data)

        # 4. ONE-HOT S�TUNLARINI AYARLA (Kullan�m Durumu)
        # Gelen veri: "Empty", "Tenant", "Owner" gibi olabilir. Biz bunu 1-0'a �evirece�iz.
        usage_input = input_data.get('usage_status', 'Owner') # Varsay�lan: M�lk Sahibi
        
        # �nce hepsini 0 yap
        df['Kullanim_Bo�'] = 0
        df['Kullanim_Kirac�l�'] = 0
        df['Kullanim_M�lk Sahibi'] = 0

        # Gelene g�re 1 yap
        if usage_input == 'Empty': df['Kullanim_Bo�'] = 1
        elif usage_input == 'Tenant': df['Kullanim_Kirac�l�'] = 1
        else: df['Kullanim_M�lk Sahibi'] = 1

        # 5. FEATURE ENGINEERING (Modelin Olmazsa Olmaz�)
        # Oda b�y�kl���n� hesaplarken Salonu da katmal� m�y�z? Modelde nas�l yapt�ysak �yle.
        # Biz �nceden: M2 / (oda + 1) yapm��t�k. Aynen devam.
        df['Oda_Buyuklugu'] = df['M2'] / (df['oda_sayisi'] + 1)
        df['Luks_Skoru'] = df['Banyo'] * df['Oda_Buyuklugu']

        # 6. SON HAZIRLIK: �ABLONA OTURTMA
        # Modelin bekledi�i 34 s�tunluk bo� bir DataFrame olu�tur
        df_final = pd.DataFrame(columns=model_columns)
        
        # Elimizdeki verileri �ablona aktar
        df_final.loc[0] = 0 # �nce hepsini 0 yap
        
        for col in df.columns:
            if col in df_final.columns:
                df_final[col] = df[col]

        # �L�E AYARI (One-Hot)
        gelen_ilce = input_data.get('district', 'Yunusemre') 
        ilce_sutunu = f"Ilce_{gelen_ilce}"
        
        if ilce_sutunu in df_final.columns:
            df_final[ilce_sutunu] = 1
            
        # Garanti olsun diye NaN kontrol�
        df_final = df_final.fillna(0)

        # 7. TAHM�N
        tahmin_fiyat = model.predict(df_final)[0]

        # 8. SONU�
        sonuc = {
            "status": "success",
            "price": round(float(tahmin_fiyat), 0),
            "currency": "TL",
            "details": {
                "features_received": list(input_data.keys())
            }
        }
        print(json.dumps(sonuc))

    except Exception as e:
        hata = {
            "status": "error",
            "message": str(e)
        }
        print(json.dumps(hata))

if __name__ == "__main__":
    try:
        tahmin_et()
    except Exception as e:
        import traceback
        hata = {
            "status": "error",
            "message": str(e),
            "traceback": traceback.format_exc(),
            "python_error": "Script crashed"
        }
        print(json.dumps(hata))