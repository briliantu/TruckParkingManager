# 🚚 Sistem Inteligent de Gestiune Parcare Camioane

Un sistem software complet de tip Enterprise conceput pentru automatizarea, monitorizarea și controlul accesului în parcările de camioane ale companiei. Proiectul utilizează o arhitectură pe trei niveluri (Client-Server) cu mecanisme avansate de redundanță pentru funcționare offline.

---

## 🏛️ Arhitectura Sistemului

Sistemul este împărțit în trei componente majore interconectate:
1. **Client Desktop (WinForms - C#):** Interfața grafică utilizată de operatorul/paznicul de la poartă pentru înregistrarea camioanelor și controlul barierei.
2. **Back-end (ASP.NET Core Web API):** Serviciul centralizat care preia cererile JSON de la clienți, validează logica de business și comunică cu baza de date.
3. **Baza de Date (SQL Server):** Stocarea persistentă și structurată a istoricului tranzacțiilor de parcare, ideală pentru audit și rapoarte de management.

---

## ✨ Caracteristici Principale

*   **Gestiune Dinamică a Locurilor:** Monitorizarea în timp real a gradului de ocupare (Capacitate maximă configurabilă, ex: 50 de locuri).
*   **Blocare Automată:** Dezactivarea automată a accesului în momentul în care parcarea devine complet plină.
*   **Calcul Inteligent al Staționării:** Algoritm integrat care transformă automat diferența de timp în format extins: `ani, luni, săptămâni, zile, ore, minute`.
*   **Control Barieră Fizică:** Funcție dedicată (`ActioneazaBariera()`) pregătită pentru integrarea cu relee electronice (port serial/rețea IP).
*   **Redundanță Critică (Offline-First):** Dacă API-ul centralizat sau conexiunea la internet pică, aplicația client salvează automat datele local în fișierul `offline_buffer.txt`.
*   **Gata de Producție:** Configurat pentru a fi compilat ca un singur fișier executabil (`.exe`) complet independent (Self-Contained).

---

## 📂 Structura Proiectului

```text
ParcareCamioane/
│
├── 💻 ParcareCamioane.Client/       # Aplicația de la poartă (WinForms)
│   ├── Program.cs                   # Punctul de pornire
│   ├── Camion.cs                    # Modelul și algoritmul de timp
│   ├── FormMain.cs                  # Logica din spatele ferestrei & Client API
│   └── FormMain.Designer.cs         # Design-ul interfeței vizuale
│
├── ⚙️ ParcareCamioane.API/          # Serverul de Backend (ASP.NET Core)
│   ├── Controllers/                 # Endpoint-uri (Entry, Exit, Active)
│   ├── Data/                        # AppDbContext (Conexiune SQL prin EF Core)
│   └── Models/                      # Structura tabelelor din baza de date
│
└── 📄 offline_buffer.txt            # Fișierul de backup local (generat la nevoie)