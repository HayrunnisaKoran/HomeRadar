import pandas as pd
import json
import os

print('='*60)
print('DETAYLI VERİ ÇEKME DURUMU')
print('='*60)

# CSV durumu
if os.path.exists('manisa_satilik_detayli.csv'):
    df = pd.read_csv('manisa_satilik_detayli.csv', encoding='utf-8-sig')
    print(f'\n✅ CSV Dosyası: manisa_satilik_detayli.csv')
    print(f'   Toplam satır: {len(df)}')
    print(f'   Benzersiz link: {df["Link"].nunique()}')
else:
    print(f'\n⚠️  CSV dosyası bulunamadı')

# Checkpoint durumu
if os.path.exists('checkpoint_detay.json'):
    with open('checkpoint_detay.json', 'r', encoding='utf-8') as f:
        checkpoint = json.load(f)
    print(f'\n✅ Checkpoint: checkpoint_detay.json')
    print(f'   İşlenen ilan: {checkpoint.get("processed_count", 0)}')
    print(f'   Toplam ilan: {checkpoint.get("total_listings", 0)}')
    kalan = checkpoint.get("total_listings", 0) - checkpoint.get("processed_count", 0)
    print(f'   Kalan ilan: {kalan}')
else:
    print(f'\n⚠️  Checkpoint dosyası bulunamadı')

# Input CSV durumu
if os.path.exists('manisa_satilik_ilce_bazli.csv'):
    df_input = pd.read_csv('manisa_satilik_ilce_bazli.csv', encoding='utf-8-sig')
    print(f'\n✅ Input CSV: manisa_satilik_ilce_bazli.csv')
    print(f'   Toplam link: {len(df_input)}')
else:
    print(f'\n⚠️  Input CSV dosyası bulunamadı')

print('\n' + '='*60)
print('Program hazır! Detay çekmeye başlayabilirsiniz.')
print('='*60)




