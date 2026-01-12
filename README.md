# UniaWarehouseManagementSystem (WMS)

**UniaWarehouseManagementSystem** to aplikacja desktopowa typu WMS (Warehouse Management System) stworzona w technologii **WPF (Windows Presentation Foundation)** na platformie .NET 6. System służy do kompleksowego zarządzania stanami magazynowymi, dokumentacją przyjęć i wydań (PZ/WZ), bazą kontrahentów oraz generowania raportów PDF.

Projekt realizowany jest zgodnie ze wzorcem architektonicznym **MVVM (Model-View-ViewModel)**.

---

## 🚀 Kluczowe Funkcjonalności

### 📦 Zarządzanie Magazynem
* **Kartoteki Produktów:** Dodawanie, edycja i usuwanie towarów.
* **Stany Magazynowe:** Śledzenie aktualnej ilości produktów oraz ich minimalnych stanów (alarmowanie kolorem w raportach).
* **Inwentaryzacja:** Generowanie raportów zbiorczych ze spisu z natury.

### 📄 Obieg Dokumentów
* **Dokumenty Magazynowe:** Obsługa dokumentów typu **PZ** (Przyjęcie Zewnętrzne) oraz **WZ** (Wydanie Zewnętrzne).
* **Historia Ruchów:** Pełna historia operacji magazynowych z możliwością filtrowania po dacie.
* **Generowanie PDF:** Automatyczne tworzenie i otwieranie plików PDF z dokumentami (zapisywane na Pulpicie).

### 👥 Zarządzanie Kontrahentami i Firmą
* **Baza Kontrahentów:** Przechowywanie danych dostawców i odbiorców (NIP, Adres).
* **Dane Firmy:** Możliwość skonfigurowania danych własnej firmy, które pojawiają się na nagłówkach wydruków.

### 🔐 Bezpieczeństwo
* **System Logowania:** Weryfikacja tożsamości użytkowników.
* **Szyfrowanie Haseł:** Hasła użytkowników są haszowane przy użyciu algorytmu **SHA256**.
* **Sesje:** Zarządzanie sesją aktualnie zalogowanego użytkownika (`AuthService`).

---

## 🛠️ Stack Technologiczny

Aplikacja została zbudowana przy użyciu nowoczesnych bibliotek .NET:

* **Framework:** .NET 6.0 (Windows)
* **UI:** WPF (Windows Presentation Foundation)
* **Baza Danych:** SQLite
* **ORM:** Entity Framework Core 7.0.20 (`Microsoft.EntityFrameworkCore.Sqlite`)
* **Architektura MVVM:** `CommunityToolkit.Mvvm` (v8.4.0) - obsługa poleceń (RelayCommand) i powiadamiania o zmianach (ObservableObject).
* **Generowanie PDF:** `QuestPDF` (v2025.12.1) - tworzenie profesjonalnych wydruków przy użyciu Fluent API.

---

## 📂 Struktura Projektu

Projekt podzielony jest na logiczne moduły zgodnie ze wzorcem MVVM:

* **Data/**
    * `UniaDbContext.cs` - Konfiguracja Entity Framework, definicja `DbSet` dla tabel (Products, Documents, Contractors, Users itd.) oraz konfiguracja połączenia z plikiem `UniaWarehouse.db`.
* **Models/**
    * Reprezentacja obiektów biznesowych (mapowanie tabel bazy danych), m.in.: `Product`, `Warehouse`, `Document`, `Contractor`, `User`.
* **Services/**
    * `AuthService.cs` - Logika autoryzacji, haszowanie haseł, obsługa zalogowanego użytkownika.
    * `PdfGenerator.cs` - Serwis odpowiedzialny za tworzenie raportów (Inwentaryzacja, Pojedynczy Dokument, Historia Ruchów) i zapisywanie ich na pulpicie.
* **ViewModels/**
    * Logika biznesowa i sterowanie widokami (pośrednik między View a Model).
* **Views/**
    * Pliki `.xaml` i `.xaml.cs` (Interfejs użytkownika).

---

## 🗄️ Baza Danych

Aplikacja korzysta z lokalnej bazy danych **SQLite**. Plik bazy danych tworzony jest automatycznie pod nazwą `UniaWarehouse.db` w katalogu roboczym aplikacji.

### Główne Encje:
1.  **Users** - Użytkownicy systemu (Login, PasswordHash).
2.  **Products** - Towary (Kod, Nazwa, Jm, Ilość, MinStan).
3.  **Documents** - Nagłówki dokumentów (Nr, Typ, Data, ID Kontrahenta).
4.  **DocumentItems** - Pozycje na dokumentach (Produkt, Ilość).
5.  **Contractors** - Baza kontrahentów.
6.  **CompanyInfos** - Dane firmy do nagłówków faktur/dokumentów.

---

## 📥 Instalacja i Uruchomienie

1.  **Wymagania wstępne:**
    * Zainstalowane środowisko .NET 6.0 SDK lub nowsze.
    * Visual Studio 2022 (zalecane) z obsługą "Tworzenie aplikacji klasycznych dla platformy .NET".

2.  **Pobranie repozytorium:**
    ```bash
    git clone [URL_DO_REPOZYTORIUM]
    ```

3.  **Budowanie projektu:**
    Otwórz plik `UniaWarehouseManagementSystem.sln` w Visual Studio. Przywróć pakiety NuGet i zbuduj rozwiązanie (Build Solution).

    Pakiety NuGet zostaną pobrane automatycznie na podstawie pliku `.csproj`:
    * `CommunityToolkit.Mvvm`
    * `Microsoft.EntityFrameworkCore.Sqlite`
    * `QuestPDF`

4.  **Uruchomienie:**
    Naciśnij `F5` lub przycisk **Start** w Visual Studio.

    *Uwaga: Przy pierwszym uruchomieniu aplikacja utworzy plik bazy danych. Jeśli baza jest pusta, konieczne może być ręczne dodanie pierwszego użytkownika przez bazę danych lub skorzystanie z formularza rejestracji (jeśli jest dostępny w GUI).*

---

## 🖨️ Generowanie Raportów (Szczegóły)

System generuje pliki PDF bezpośrednio na **Pulpit** użytkownika. Dostępne są trzy rodzaje wydruków:

1.  **Raport Stanów Magazynowych:**
    * Plik: `Stan_Magazynowy_YYYYMMDD_HHmm.pdf`
    * Zawiera listę wszystkich produktów.
    * Produkty poniżej stanu minimalnego są oznaczone kolorem **czerwonym**.

2.  **Wydruk Dokumentu (PZ/WZ):**
    * Plik: `Dokument_[NumerDokumentu].pdf`
    * Zawiera dane firmy, kontrahenta, listę pozycji, oraz miejsce na podpisy (Wystawiający/Odbierający).

3.  **Raport Ruchów (Historia):**
    * Plik: `Raport_Ruchow_YYYYMMDD_YYYYMMDD.pdf`
    * Zestawienie operacji w zadanym okresie.
    * Dokumenty **PZ** oznaczone kolorem zielonym, **WZ** kolorem czerwonym.

---

## 📜 Licencja

Projekt jest dostępny na licencji [Nazwa Licencji, np. MIT], patrz plik LICENSE po szczegóły.
