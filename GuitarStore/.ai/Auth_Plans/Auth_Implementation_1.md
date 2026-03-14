# Auth Implementation - First Changes Plan

Data: 2026-03-12
Cel: przygotowac repo do migracji z Duende do OpenIddict i podpiac Auth UI w ApiGateway.
Zakres: pierwszy, bezpieczny krok (konfiguracja + szkielet Auth).

---

## 1) Przeglad i przygotowanie struktury konfiguracji

**Cel:** wprowadzic appsettings.<Module>.json i mechanizm ladowania w ApiGateway.

**Zmiany:**
- Dodac pliki:
  - GuitarStore.ApiGateway/appsettings.Auth.json
  - GuitarStore.ApiGateway/appsettings.Customers.json (opcjonalnie na start, pusty)
  - (opcjonalnie) placeholdery dla innych modulow
- Zaktualizowac `GuitarStore.ApiGateway/Program.cs`:
  - `builder.Configuration.AddJsonFile("appsettings.Auth.json", optional: true, reloadOnChange: true);`
  - analogicznie dla innych modulow
  - zachowac dotychczasowe `appsettings.json` i `appsettings.Development.json`

**Uwaga:** Na tym etapie nie ruszamy jeszcze DI w modulach poza dodaniem konfiguracji.

---

## 2) Usuniecie Duende IdentityServer (migracja wstepna)

**Cel:** usunac stare elementy Duende, aby nie konfliktowaly z OpenIddict.

**Zmiany:**
- Usunac pakiet `Duende.IdentityServer` z `Auth.Core/Auth.Core.csproj`.
- Usunac `Auth.Core/DuendeConfig.cs`.
- W `GuitarStore.ApiGateway/Program.cs`:
  - usunac `AddIdentityServer()` i konfiguracje in-memory
  - usunac `app.UseIdentityServer()`

**Efekt:** projekt przestaje hostowac Duende.

---

## 3) Dodanie pakietow OpenIddict + Identity

**Cel:** przygotowac zaleznosci dla nowego Auth.

**Pakiety (propozycja):**
- `OpenIddict.AspNetCore`
- `OpenIddict.EntityFrameworkCore`
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.SqlServer` (juz jest w innych modulach)

**Gdzie:**
- Auth.Core (lub nowy Auth.Infrastructure, jesli wydzielimy DB layer)
- ApiGateway (jesli UI i endpoints beda hostowane tutaj)

---

## 4) Szkielet AuthDbContext + konfiguracja Identity

**Cel:** wprowadzic podstawowy context dla Identity + OpenIddict.

**Zmiany (Auth.Core lub nowy Auth.Infrastructure):**
- AuthDbContext dziedziczacy po `IdentityDbContext<ApplicationUser, ApplicationRole, Guid>`
- Konfiguracja schematu `Auth` dla tabel
- Encje `ApplicationUser` (Guid key)
- Encja `ApplicationRole` (Guid key)

**Konfiguracja w DI (ApiGateway):**
- `services.AddDbContext<AuthDbContext>(...UseSqlServer...)`
- `services.AddIdentity<ApplicationUser, ApplicationRole>(...)`

---

## 5) OpenIddict - minimalna konfiguracja (bez UI)

**Cel:** dodac podstawowa konfiguracje OpenIddict w ApiGateway.

**Zmiany:**
- `services.AddOpenIddict()`:
  - `.AddCore().UseEntityFrameworkCore().UseDbContext<AuthDbContext>()`
  - `.AddServer()`:
    - `AllowAuthorizationCodeFlow().RequireProofKeyForCodeExchange()`
    - `AllowRefreshTokenFlow()`
    - endpointy `/connect/authorize`, `/connect/token`, `/connect/logout`
    - `SetIssuer(...)` z configu
    - dev signing cert (`AddDevelopmentEncryptionCertificate()`, `AddDevelopmentSigningCertificate()`)
  - `.AddValidation()`:
    - `UseLocalServer()`
    - `UseAspNetCore()`

**Uwaga:** w tym etapie nie dodajemy jeszcze UI ani rejestracji klienta SPA.

---

## 6) Auth UI folder (Razor/MVC) - przygotowanie miejsca

**Cel:** przygotowac strukture UI w ApiGateway bez pelnej implementacji.

**Zmiany:**
- Dodac folder `GuitarStore.ApiGateway/AuthUI` (lub `Areas/Auth`) jako miejsce na Razor Pages/MVC.
- Dodac puste kontrolery/pages (placeholder) lub sam folder z README.

---

## 7) Konfiguracja sekcji Auth w appsettings.Auth.json

**Cel:** ustawic minimum do uruchomienia.

**Klucze (minimum):**
- Auth:Issuer
- Auth:AccessTokenMinutes
- Auth:RefreshTokenDays
- Auth:Password policy
- Auth:Lockout
- SeedAdmin
- Cors:AllowedOrigins

---

## 8) Migracje (na koniec kroku 1)

**Cel:** utworzyc migracje dla Auth schema.

**Akcje:**
- Dodac migracje w projekcie z AuthDbContext.
- Zweryfikowac, ze migracja tworzy tabele Identity + OpenIddict w schemacie Auth.

---

## 9) Checklist po pierwszym kroku

- [ ] Duende usuniete z repo i Program.cs
- [ ] Appsettings.<Module>.json ladowane przez ApiGateway
- [ ] AuthDbContext i Identity skonfigurowane
- [ ] OpenIddict server/validation dodane (bez UI)
- [ ] Migracja Auth gotowa

---

## 10) Ryzyka / zaleznosci

- Przed dodaniem UI trzeba doprecyzowac route pattern (np. /auth/login).
- Przed rejestracja SPA trzeba ustalic redirect URI.
- Ustalic miejsce na seed admin i role (etap 2).

