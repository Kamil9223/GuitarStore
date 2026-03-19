# Auth Implementation 3

Data: 2026-03-17
Status: Draft

## 0. Cel kroku 3

Krok 3 realizuje sekcje `Account UI` z `.ai/Auth.md`.

Po tym kroku ApiGateway ma dostarczyc czysty, server-side UI dla:
- logowania,
- rejestracji,
- wylogowania,
- odmowy dostepu.

Krok 3 nie powinien jeszcze domykac:
- rejestracji klienta SPA w OpenIddict,
- Customers integration,
- seed admin,
- pelnych roles/policies,
- email confirmation.

To sa osobne etapy z glownego planu.

## 1. Wejscie do kroku 3

Zakladany stan po kroku 2:
- Identity jest skonfigurowane,
- OpenIddict Server i Validation sa skonfigurowane,
- discovery dziala,
- `/connect/token` jest obslugiwany przez OpenIddict,
- cookie auth jest skonfigurowane,
- nie ma jeszcze endpointow `/auth/*`.

## 2. Zakres funkcjonalny

Krok 3 ma dodac:
- `GET /auth/login`
- `POST /auth/login`
- `GET /auth/register`
- `POST /auth/register`
- `POST /auth/logout`
- `GET /auth/forbidden`

Forma implementacji:
- ASP.NET Core MVC lub Razor Views hostowane w `GuitarStore.ApiGateway`
- bez recznie skladanego HTML w kontrolerach
- cienkie kontrolery, logika poza kontrolerem tam, gdzie ma sens

## 3. Architektura implementacji

### 3.1 Kontroler UI

Dodac modul UI auth w `GuitarStore.ApiGateway`, np.:
- `Modules/Auth/Controllers/AccountController.cs`

Odpowiedzialnosc kontrolera:
- przyjmowanie requestow,
- walidacja model state,
- delegowanie do `SignInManager` i `UserManager`,
- redirecty i zwracanie widokow.

Kontroler nie powinien:
- budowac HTML jako string,
- zawierac duzej logiki mapowania claims/protokolu OpenIddict,
- mieszac logiki rejestracji z Customers integration.

### 3.2 ViewModels

Dodac modele widokow, np.:
- `LoginViewModel`
- `RegisterViewModel`
- `ForbiddenViewModel` jesli potrzebny

Minimalny zakres pol:
- Login: `EmailOrUserName`, `Password`, `RememberMe`, `ReturnUrl`
- Register: `Email`, `Password`, `ConfirmPassword`, ewentualnie `ReturnUrl`

Uwaga:
- `.ai/Auth.md` wskazuje, ze dla Customers pozostaje temat `Name/LastName`.
- Na etapie planu kroku 3 trzeba podjac decyzje:
  - albo rozszerzamy formularz register o `Name` i `LastName`,
  - albo register tworzy tylko usera auth i temat Customers zostaje rozstrzygniety w kroku 6.

Rekomendacja:
- nie mieszac jeszcze Customers do samego UI kroku 3,
- ale zostawic jawny punkt decyzyjny przed implementacja POST `/auth/register`.

### 3.3 Views

Dodac widoki Razor, np.:
- `Views/Account/Login.cshtml`
- `Views/Account/Register.cshtml`
- `Views/Account/Forbidden.cshtml`

Wymagania:
- prosty, czytelny layout,
- pelna obsluga walidacji serwerowej,
- wyswietlanie bledow logowania/rejestracji,
- zachowanie `returnUrl`.

### 3.4 Powiazanie z cookie auth

W kroku 3 trzeba aktywnie zaczac korzystac z cookie Identity:
- `GET /auth/login` pokazuje formularz,
- `POST /auth/login` loguje przez `SignInManager.PasswordSignInAsync(...)`,
- `POST /auth/logout` wylogowuje przez `SignInManager.SignOutAsync()`.

Cookie pozostaje mechanizmem browser session.

### 3.5 Powiazanie z `/connect/authorize`

Tutaj trzeba zachowac ostroznosc architektoniczna.

Sa dwa sensowne warianty:

Wariant A:
- dodac passthrough dla authorization/logout,
- dodac cienki endpoint MVC/minimal API tylko dla czesci interaktywnej,
- utrzymac logike protokolu po stronie OpenIddict.

Wariant B:
- pozostawic OpenIddict jako wlasciciela endpointow,
- a challenge/login opierac o standardowe zachowanie hosta ASP.NET Core, jesli da sie to czysto spiac bez rozlewania logiki do kontrolerow.

Rekomendacja dla tego repo:
- Wariant A, ale bardzo cienko.

Powod:
- browser flow dla authorization code wymaga swiadomego sterowania loginem usera i powrotem do requestu authorize,
- bez passthrough i cienkiej warstwy interaktywnej kod staje sie mniej czytelny,
- poprzednia wersja byla zla nie dlatego, ze miala kontroler, tylko dlatego, ze za duzo logiki i placeholder UI siedzialo w kontrolerach.

W praktyce:
- krok 3 moze przywrocic obsluge authorization endpoint,
- ale tylko razem z prawdziwym UI i z cienkim kontrolerem/serwisem.

### 3.6 Serwis pomocniczy dla OIDC principal

Jesli pojawi sie logika budowy principal dla authorize flow, wydzielic ja do osobnego serwisu, np.:
- `IOidcClaimsPrincipalFactory`
- `OidcClaimsPrincipalFactory`

Odpowiedzialnosc serwisu:
- budowa principal z usera Identity,
- ustawienie `sub`, `email`, `name`, `preferred_username`, `role`,
- ustawienie scopes i claim destinations.

To ma odciazyc kontroler.

## 4. Plan implementacji technicznej

### Etap 3.1: Wlaczenie MVC / Views w ApiGateway
- Potwierdzic, ze `Program.cs` ma `AddControllersWithViews()` lub rownowazna konfiguracje.
- Potwierdzic, ze pipeline ma `UseRouting()`, `UseAuthentication()`, `UseAuthorization()` i mapowanie kontrolerow.

### Etap 3.2: Dodanie endpointow UI
- Dodac `AccountController`.
- Dodac akcje `Login`, `Register`, `Logout`, `Forbidden`.
- Ochrona `returnUrl` przez `Url.IsLocalUrl(...)` lub analogiczna walidacje.

### Etap 3.3: Dodanie modeli widokow i walidacji
- `LoginViewModel`
- `RegisterViewModel`
- atrybuty walidacyjne zgodne z password policy tam, gdzie to ma sens
- mapowanie bledow `IdentityResult` do `ModelState`

### Etap 3.4: Dodanie widokow Razor
- formularz login
- formularz register
- ekran forbidden
- spójna prezentacja bledow i komunikatow

### Etap 3.5: Integracja z authorize flow
- przywrocic tylko te elementy OpenIddict ASP.NET Core passthrough, ktore sa niezbedne do browser flow,
- dodac cienka obsluge authorize/logout,
- wykorzystac cookie auth zamiast stringowego HTML.

### Etap 3.6: Rejestracja usera
- utworzenie usera przez `UserManager`
- przypisanie roli `user` jesli ma to juz sens na tym etapie
- decyzja, czy od razu logowac po rejestracji
- jawne odnotowanie zaleznosci do kroku 6, jesli Customers integration jeszcze nie jest gotowa

### Etap 3.7: Testy
- `GET /auth/login` zwraca widok
- `POST /auth/login` zlymi danymi zwraca bledy
- `POST /auth/login` poprawnymi danymi ustawia cookie i redirect
- `POST /auth/logout` usuwa cookie
- `GET /auth/forbidden` zwraca ekran odmowy dostepu
- authorize flow przekierowuje niezalogowanego usera do login UI

## 5. Ryzyka i decyzje do podjecia

### 5.1 Register a Customers

Nadal otwarty temat:
- Customers wymaga `Name/LastName`,
- Auth krok 3 formalnie dotyczy UI account,
- Customers integration jest dopiero w kroku 6.

Decyzja potrzebna przed finalnym POST `/auth/register`:
- czy UI rejestracji juz zbiera `Name/LastName`,
- czy register w kroku 3 tworzy tylko konto auth,
- czy rejestracja ma byc chwilowo ograniczona do scenariusza admin/dev.

### 5.2 Zakres authorize endpoint

Trzeba utrzymac cienka granice:
- krok 3 moze dodac obsluge browser authorize flow,
- ale nie powinien dublowac silnika OpenIddict.

### 5.3 Styling UI

Nie robic rozbudowanego frontu.

Wystarczy:
- czytelne widoki Razor,
- sensowna semantyka formularzy,
- poprawne komunikaty walidacyjne.

## 6. Definition of Done dla kroku 3

Krok 3 uznajemy za zamkniety, gdy:
- istnieja endpointy `/auth/login`, `/auth/register`, `/auth/logout`, `/auth/forbidden`,
- UI jest zrobione w Razor/MVC, nie przez reczny HTML,
- login i logout dzialaja na cookie Identity,
- niezalogowany user w authorize flow trafia do login UI,
- po poprawnym logowaniu flow moze wrocic do authorize request,
- sa testy integracyjne dla login/logout i podstawowego redirect flow,
- kod nie rozlewa logiki OIDC po kontrolerach.
