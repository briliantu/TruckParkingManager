# 🚚 TruckParkingManager

Sistem de gestiune parcare pentru camioane, cu aplicație desktop pentru operatorul de la poartă și server central pentru sincronizarea datelor pe rețeaua locală.

---

## 🏛️ Arhitectură

Proiectul are două componente independente, care comunică prin HTTP pe rețeaua locală:

| Componentă | Ce este | Unde rulează |
|---|---|---|
| **`Client/`** | Aplicație desktop WinForms (C#, .NET 8) | Pe fiecare PC de la poarta de acces |
| **`Server/`** | API central (ASP.NET Core, minimal API) | Pe un singur PC/server din rețeaua locală |

Clientul trimite cereri HTTP către server la fiecare intrare/ieșire de camion. Dacă serverul e temporar inaccesibil, clientul salvează evenimentul local și îl retrimite automat la reconectare — parcarea nu se blochează niciodată din cauza rețelei.

---

## ✨ Funcționalități

**Client (`Client/`)**
- Înregistrare intrare/ieșire camioane, cu alocare automată a locului liber
- Blocare automată a intrărilor noi când parcarea e plină
- Verificare duplicate — un camion nu poate fi înregistrat de două ori simultan
- Calcul precis al duratei de staționare (ani/luni/săptămâni/zile/ore/minute, pe bază de calendar real)
- Dropdown cu țara de proveniență (România, Spania, Germania, Italia, Bulgaria, Turcia, Republica Moldova), cu validare de format specifică fiecărei țări
- Hartă vizuală a locurilor (verde = liber, roșu = ocupat), actualizată în timp real
- Mod offline-first: dacă serverul nu răspunde, evenimentul e salvat în `offline_buffer.txt` (format text lizibil, nu JSON brut) și retrimis automat la următoarea pornire
- Configurare externă prin `appsettings.json` (adresa serverului, capacitatea parcării) — fără recompilare la schimbarea serverului
- Bara de titlu arată mereu la ce server e conectată aplicația

**Server (`Server/`)**
- API minimal cu trei rute: `GET /parcare/active`, `POST /parcare/intrare`, `PUT /parcare/iesire`
- Persistență simplă în fișier JSON (`parcare-date.json`) — nu necesită instalare de bază de date
- La pornire, detectează și afișează automat adresa IP din rețeaua locală

---

## 📂 Structura Proiectului

```
TruckParkingManager/
│
├── Client/
│   ├── Program.cs                    # Punctul de pornire al aplicației
│   ├── Camion.cs                     # Modelul camionului + calculul duratei
│   ├── ConfigurareApp.cs             # Încarcă appsettings.json la runtime
│   ├── FormMain.cs                   # Logica ferestrei principale + apeluri API
│   ├── FormMain.Designer.cs          # Design-ul interfeței
│   ├── appsettings.example.json      # Șablon de configurare (fără date reale)
│   └── TruckParkingManager.csproj
│
├── Server/
│   ├── ReteaLocala.cs                    # API-ul + logica de persistență
│   └── TruckParkingManagerServer.csproj
│
└── README.md
```

> `appsettings.json` (client) și `parcare-date.json` (server) conțin date reale de configurare/operare și **nu sunt urcate pe GitHub** — vezi `.gitignore`. Pornește de la `appsettings.example.json`.

---

## 🚀 Instalare & Rulare

### Cerințe
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) pe PC-ul unde dai build
- Windows pe PC-urile client (WinForms necesită Windows)
- Server-ul poate rula pe orice PC Windows din rețeaua locală

### 1. Serverul central

```powershell
cd Server
dotnet run
```

La pornire, consola afișează adresa la care ascultă, de exemplu:
```
Adresa pentru aplicațiile client: http://apiurl/parcare
```

**Firewall** — trebuie deschis portul 5000 pentru conexiuni de intrare (o singură dată, din PowerShell ca administrator):
```powershell
New-NetFirewallRule -DisplayName "TruckParkingManager" -Direction Inbound -Protocol TCP -LocalPort 5000 -Action Allow
```

Pentru rulare permanentă (fără terminal deschis), publică un executabil:
```powershell
dotnet publish -c Release
```

### 2. Aplicația client

```powershell
cd Client
copy appsettings.example.json appsettings.json
```

Editează `appsettings.json` cu adresa reală a serverului:
```json
{
  "ApiUrl": "http://apiurl/parcare",
  "CapacitateMaxima": 50
}
```

Apoi:
```powershell
dotnet run
```

Repetă pasul 2 pe fiecare PC de la poartă, cu același `ApiUrl`.

---

## 🔧 Configurare

Toate setările client sunt în `appsettings.json` (lângă executabil), nu în cod:

| Cheie | Descriere |
|---|---|
| `ApiUrl` | Adresa serverului central, ex: `http://apiurl/parcare` |
| `CapacitateMaxima` | Numărul de locuri de parcare disponibile |

---

## 🛠️ Roadmap posibil

- Control real al barierei fizice (releu serial/IP) în locul simulării actuale
- Autentificare pentru API (momentan orice client din rețea poate trimite cereri)
- Sincronizare periodică automată a buffer-ului offline (nu doar la pornire)
- Rulare server ca serviciu Windows (pornire automată la boot)