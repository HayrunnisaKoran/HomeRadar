import sys
import json
import pandas as pd
import joblib
import numpy as np

# 1. MODELÝ VE SÜTUN ÝSÝMLERÝNÝ YÜKLE
try:
    model = joblib.load('manisa_ev_fiyat_modeli_v1_060.pkl')
    # model_columns.pkl dosyasýný oluþturmadýysan, aþaðýdaki listeyi kullanýrýz ama doðrusu pkl'den çekmektir.
    try:
        model_columns = joblib.load('model_columns.pkl')
    except:
        # Acil durum: Eðer pkl yoksa  listeyi elle tanýmlýyorum
        model_columns = ['M2', 'Banyo', 'Bina_Yasi', 'Balkon', 'Asansor', 'Garaj', 'Takas', 'Eþya Durumu', 
                         'oda_sayisi', 'salon_sayisi', 'Kat', 'Isinma', 'yapi_durumu', 
                         'Kullanim_Boþ', 'Kullanim_Kiracýlý', 'Kullanim_Mülk Sahibi', 'tapu_durumu', 
                         'Ilce_Akhisar', 'Ilce_Alasehir', 'Ilce_Demirci', 'Ilce_Gordes', 'Ilce_Kirkagac', 
                         'Ilce_Koprubasi', 'Ilce_Kula', 'Ilce_Salihli', 'Ilce_Sarigol', 'Ilce_Saruhanli', 
                         'Ilce_Sehzadeler', 'Ilce_Selendi', 'Ilce_Soma', 'Ilce_Turgutlu', 'Ilce_Yunusemre', 
                         'Oda_Buyuklugu', 'Luks_Skoru']

except FileNotFoundError:
    print(json.dumps({"status": "error", "message": "Model dosyalari (pkl) bulunamadi."}))
    sys.exit(1)

def tahmin_et():
    try:
        # 2. NODE.JS'DEN GELEN VERÝYÝ OKU
        input_json = sys.argv[1]
        input_data = json.loads(input_json)

        # 3. VERÝLERÝ HAZIRLA (Varsayýlan deðerlerle)
        # Node.js'ten Ýngilizce key'ler gelecek, biz senin Türkçe sütunlarýna çevireceðiz.
        
        # Temel Veriler
        data = {
            'M2': [float(input_data.get('square_meters', 100))],
            'Banyo': [int(input_data.get('bathrooms', 1))],
            'Bina_Yasi': [int(input_data.get('building_age', 5))],
            'Balkon': [int(input_data.get('balcony', 1))],          # Var:1, Yok:0
            'Asansor': [int(input_data.get('elevator', 0))],        # Var:1, Yok:0
            'Garaj': [int(input_data.get('garage', 0))],            # Var:1, Yok:0
            'Takas': [int(input_data.get('swap', 0))],              # Var:1, Yok:0
            'Eþya Durumu': [int(input_data.get('furnished', 0))],   # Eþyalý:1, Boþ:0
            'oda_sayisi': [int(input_data.get('rooms', 3))],
            'salon_sayisi': [int(input_data.get('living_rooms', 1))],
            'Kat': [int(input_data.get('floor', 2))],
            'Isinma': [int(input_data.get('heating', 1))],          # Doðalgaz:1, Diðer:0 (Örnek)
            'yapi_durumu': [int(input_data.get('building_status', 0))], # Sýfýr:0, Ýkinci El:1 (Mantýðýna göre deðiþir)
            'tapu_durumu': [int(input_data.get('title_deed', 1))]   # Kat Mülkiyeti vb.
        }
        
        df = pd.DataFrame(data)

        # 4. ONE-HOT SÜTUNLARINI AYARLA (Kullaným Durumu)
        # Gelen veri: "Empty", "Tenant", "Owner" gibi olabilir. Biz bunu 1-0'a çevireceðiz.
        usage_input = input_data.get('usage_status', 'Owner') # Varsayýlan: Mülk Sahibi
        
        # Önce hepsini 0 yap
        df['Kullanim_Boþ'] = 0
        df['Kullanim_Kiracýlý'] = 0
        df['Kullanim_Mülk Sahibi'] = 0

        # Gelene göre 1 yap
        if usage_input == 'Empty': df['Kullanim_Boþ'] = 1
        elif usage_input == 'Tenant': df['Kullanim_Kiracýlý'] = 1
        else: df['Kullanim_Mülk Sahibi'] = 1

        # 5. FEATURE ENGINEERING (Modelin Olmazsa Olmazý)
        # Oda büyüklüðünü hesaplarken Salonu da katmalý mýyýz? Modelde nasýl yaptýysak öyle.
        # Biz önceden: M2 / (oda + 1) yapmýþtýk. Aynen devam.
        df['Oda_Buyuklugu'] = df['M2'] / (df['oda_sayisi'] + 1)
        df['Luks_Skoru'] = df['Banyo'] * df['Oda_Buyuklugu']

        # 6. SON HAZIRLIK: ÞABLONA OTURTMA
        # Modelin beklediði 34 sütunluk boþ bir DataFrame oluþtur
        df_final = pd.DataFrame(columns=model_columns)
        
        # Elimizdeki verileri þablona aktar
        df_final.loc[0] = 0 # Önce hepsini 0 yap
        
        for col in df.columns:
            if col in df_final.columns:
                df_final[col] = df[col]

        # ÝLÇE AYARI (One-Hot)
        gelen_ilce = input_data.get('district', 'Yunusemre') 
        ilce_sutunu = f"Ilce_{gelen_ilce}"
        
        if ilce_sutunu in df_final.columns:
            df_final[ilce_sutunu] = 1
            
        # Garanti olsun diye NaN kontrolü
        df_final = df_final.fillna(0)

        # 7. TAHMÝN
        tahmin_fiyat = model.predict(df_final)[0]

        # 8. SONUÇ
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
    tahmin_et()