# Auth Implementation - Second Changes Plan

Data: 2026-03-14
Cel: domknac konfiguracje OpenIddict po dodaniu szkieletu Auth module.
Zakres: krok 2 (konfiguracja serwera OIDC/OAuth2, JWT, certyfikaty, discovery, walidacja).

---

## 0) Stan wyjsciowy po kroku 1

**Mamy juz:**
- `Auth.Core` z `Identity + OpenIddict + EF Core`
- `AuthDbContext` w schemacie `Auth`
- wspolny strongly typed key `AuthId` dla `User` i `Role`
- minimalna konfiguracja `OpenIddict` w `AuthModuleInitializator`
- migracje dla tabel `Identity` i `OpenIddict`
- ladowanie `appsettings.Auth.json` w `ApiGateway`

**Braki do domkniecia w kroku 2:**
- konfiguracja auth pipeline jest nadal zbyt minimalna
- trzeba doprecyzowac schematy autentykacji
- trzeba sformalizowac ustawienia tokenow i certyfikatow
- trzeba zweryfikowac discovery i endpointy OIDC
- trzeba przygotowac grunt pod krok 3 (UI) i krok 4 (SPA client)

---

## 1) Przeglad i uporzadkowanie konfiguracji OpenIddict

**Cel:** doprowadzic `AuthModuleInitializator` do postaci, ktora jasno odzwierciedla docelowy flow.

**Zmiany:**
- Przejrzec `Auth.Core/AuthModuleInitializator.cs`.
- Wydzielic logicznie:
  - konfiguracje `Identity`
  - konfiguracje `OpenIddict Core`
  - konfiguracje `OpenIddict Server`
  - konfiguracje `OpenIddict Validation`
- Upewnic sie, ze konfiguracja nie miesza odpowiedzialnosci kroku 2 z przyszlym UI.

**Efekt:** konfiguracja auth jest czytelna i gotowa do rozbudowy w kolejnych krokach.

---

## 2) Domkniecie schematow autentykacji

**Cel:** ustawic poprawny model auth dla:
- cookie auth pod przyszly login UI,
- validation/bearer auth dla API,
- interoperacyjnosc z endpointami OpenIddict.

**Zmiany:**
- Zweryfikowac `AddAuthentication(...)` i ustawic jawnie domyslne schematy.
- Dodac cookie scheme dla przyszlego UI logowania, np. `IdentityConstants.ApplicationScheme`.
- Upewnic sie, ze `app.UseAuthentication()` i `app.UseAuthorization()` sa poprawnie osadzone w pipeline.
- Zweryfikowac, jak `OpenIddict.Validation.AspNetCore` ma byc podpinane jako schemat dla API.

**Uwaga:** UI logowania nadal nie jest implementowane w tym kroku, ale schemat cookie powinien juz byc przygotowany.

---

## 3) OpenIddict Server - finalizacja minimalnego MVP

**Cel:** skonfigurowac serwer OIDC/OAuth2 zgodnie z zalozeniami architektonicznymi.

**Zmiany:**
- Potwierdzic i utrwalic support dla:
  - Authorization Code flow
  - PKCE (wymagane)
  - Refresh Token flow
- Wylaczyc / nie dodawac flow, ktorych nie chcemy na MVP:
  - implicit
  - password
  - client credentials
- Potwierdzic endpointy:
  - `/.well-known/openid-configuration`
  - `/connect/authorize`
  - `/connect/token`
  - `/connect/logout`
- Skonfigurowac URI issuera z `Auth:Issuer`.
- Zweryfikowac, czy potrzebne sa `UseAspNetCore()` options typu passthrough dla kroku 3.

**Efekt:** serwer OpenIddict ma jednoznacznie zdefiniowany zakres MVP.

---

## 4) JWT i lifetimes tokenow

**Cel:** dopracowac zachowanie wydawanych tokenow.

**Zmiany:**
- Potwierdzic, ze access tokeny sa JWT i nie sa przypadkiem konfigurowane jako reference tokens.
- Powiazac lifetimes z konfiguracja:
  - `Auth:AccessTokenMinutes`
  - `Auth:RefreshTokenDays`
- Zweryfikowac, czy potrzebne jest jawne wlaczenie `offline_access`.
- Przygotowac miejsce pod przyszla konfiguracje scopes/claims.

**Do decyzji implementacyjnej:**
- czy w kroku 2 robimy juz pelne ustawienia refresh token rotation,
- czy tylko przygotowujemy serwer i zostawiamy zaawansowane reuse detection na osobny podkrok.

**Rekomendacja:** na krok 2 skonfigurowac prawidlowy flow i lifetimes, a reuse/revoke potwierdzic testem lub event handlerem dopiero po ustaleniu finalnego modelu klienta SPA.

---

## 5) Certyfikaty signing/encryption

**Cel:** uporzadkowac temat kluczy podpisu i szyfrowania.

**Zmiany:**
- W dev:
  - zostawic development signing/encryption certificates
  - upewnic sie, ze uruchamiaja sie stabilnie lokalnie
- W produkcji:
  - przygotowac jawny TODO / kontrakt konfiguracji dla certyfikatow
  - zdecydowac, czy produkcja ma korzystac z:
    - thumbprintow z cert store,
    - plikow `.pfx`,
    - czy dedykowanego secret store
- Jesli potrzeba, rozszerzyc `AuthOptions` o sekcje certyfikatow.

**Efekt:** dev dziala lokalnie, a produkcyjny kierunek jest zapisany w kodzie lub konfiguracji.

---

## 6) Discovery i metadata

**Cel:** upewnic sie, ze OpenIddict publikuje poprawne metadata.

**Zmiany:**
- Zweryfikowac endpoint discovery:
  - issuer
  - authorization endpoint
  - token endpoint
  - end session endpoint
  - supported grant types
  - supported response types
- Upewnic sie, ze `Issuer` i endpointy sa spojne z hostem `ApiGateway`.

**Efekt:** zewnetrzny klient SPA bedzie mogl w kroku 4 pobrac poprawny discovery document.

---

## 7) Przygotowanie pod scopes i claims

**Cel:** nie wdrazac jeszcze pelnego modelu uprawnien, ale przygotowac techniczny grunt.

**Zmiany:**
- Ustalic minimalny zestaw scopes dla MVP:
  - `openid`
  - `profile` (opcjonalnie)
  - `offline_access`
- Sprawdzic, czy potrzebna jest w kroku 2 rejestracja scopes w bazie OpenIddict.
- Przygotowac miejsce pod przyszle claims:
  - subject / user id
  - role
  - permission claims (krok 5)

**Uwaga:** nie seedujemy jeszcze rol i permission claims w tym kroku.

---

## 8) Korekty w konfiguracji i opcjach

**Cel:** dopasowac kontrakty konfiguracyjne do kroku 2.

**Zmiany:**
- Przejrzec `AuthOptions` i ocenic, czy wystarczaja dla:
  - token lifetimes
  - issuer
  - cert config placeholders
  - ewentualnych scopes
- Jesli trzeba, dodac brakujace sekcje do:
  - `Auth.Core/Configuration/AuthOptions.cs`
  - `GuitarStore.ApiGateway/appsettings.Auth.json`

**Efekt:** konfiguracja nie wymaga hardcode przy dalszych krokach.

---

## 9) Weryfikacja techniczna po kroku 2

**Cel:** sprawdzic, ze konfiguracja serwera dziala zanim przejdziemy do UI.

**Akcje:**
- Build `GuitarStore.ApiGateway`.
- Sprawdzic lokalnie:
  - discovery document
  - odpowiedz endpointu token przy blednym zadaniu
  - czy pipeline auth nie powoduje regresji w API
- Zweryfikowac, ze migracje nadal sa spojne z modelem.

**Uwaga:** bez UI logowania i bez SPA client registration nie zamykamy jeszcze calego flow auth code end-to-end.

---

## 10) Checklist po drugim kroku

- [ ] OpenIddict server jest skonfigurowany jawnie pod auth code + PKCE + refresh token
- [ ] JWT access tokens maja poprawne lifetimes z configu
- [ ] Discovery document publikuje poprawne metadata
- [ ] Authentication schemes sa uporzadkowane pod API i przyszle UI
- [ ] Dev certyfikaty dzialaja, a kierunek dla prod jest zapisany
- [ ] Kod jest gotowy pod krok 3 (UI) i krok 4 (SPA client)

---

## 11) Ryzyka / zaleznosci

- Bez kroku 3 nie zweryfikujemy realnego logowania usera przez UI.
- Bez kroku 4 nie zweryfikujemy calego SPA auth code + PKCE.
- Refresh token rotation/reuse detection moze wymagac doprecyzowania semantyki OpenIddict i testow integracyjnych.
- Trzeba pilnowac spojnosci `Issuer`, hosta gatewaya i redirect URI klienta SPA.

---

## 12) Rekomendowana kolejnosc implementacji

1. Uporzadkowac `AddAuthentication` i `OpenIddict` configuration.
2. Domknac lifetimes tokenow, discovery i endpoint metadata.
3. Przygotowac kontrakt certyfikatow oraz ewentualne rozszerzenia `AuthOptions`.
4. Zweryfikowac lokalnie discovery i podstawowe zachowanie endpointow.
5. Dopiero potem przejsc do kroku 3 (Account UI).
