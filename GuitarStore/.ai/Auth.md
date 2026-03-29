# GuitarStore Auth Module Implementation Plan

Data: 2026-03-12
Status: Draft (approved requirements)

## 0. Context i cel

Projekt: modularny monolit .NET 8, jedna baza MSSQL (schematy per modul).
Cel: wdrozyc darmowy, bezpieczny i rozszerzalny modul Auth oparty o
- ASP.NET Core Identity (user store, hasla, role)
- OpenIddict (OIDC/OAuth2, JWT access tokens, refresh tokens)
- Policy-based authorization + resource-based checks

Frontend: SPA (React) z Authorization Code + PKCE.
Login UI: hostowany w ApiGateway (server-side, osobny folder UI).

### 0.1 Stan repo (przeglad)

- ApiGateway obecnie uzywa Duende IdentityServer (AddIdentityServer/UseIdentityServer).
- Auth.Core zawiera DuendeConfig + referencje do Duende.IdentityServer.
- Customers ma istniejący event SignedUpEvent (Name, LastName, Email) i
  wymaga Name/LastName w Customer.Create().

Wniosek: potrzebna migracja z Duende do OpenIddict oraz decyzja jak
mapowac rejestracje Auth -> Customers (Name/LastName).

## 1. Kluczowe decyzje (ustalone)

- Self-hosted: OpenIddict + Identity (darmowe).
- Login UI w ApiGateway (osobny folder UI), SPA tylko redirect OIDC.
- Konfiguracja per modul: osobne pliki appsettings.<Module>.json ladowane przez ApiGateway.
- Refresh tokens: tak, rotacja tak.
- Access token krotki (konfigurowalny, domyslnie 15 min).
- Email confirmation: nie na start (TODO).
- Lockout: tak.
- Roles: user, admin, support.
- Support: read + ograniczone akcje (patrz sekcja 6).
- Customers: osobny modul z danymi profilu klienta; Auth tylko login/haslo.
- Integracja z Customers: event UserRegistered.
- Seed admin: dev/local tak; prod - bootstrap once.
 - Konieczna decyzja dot. Name/LastName przy rejestracji (patrz sekcja 8).

## 2. Architektura docelowa (high level)

Auth module:
- AuthDbContext (Identity + OpenIddict) w schemacie Auth.
- Konfiguracja OpenIddict i endpointow rejestrowana przez modul w ApiGateway.
- Brak osobnego hosta UI.

ApiGateway / inne moduly:
- Hostuje Auth UI (Razor/MVC, /auth/*).
- Udostepnia endpointy OIDC (/connect/*, discovery) z Auth module.
- JwtBearer auth z Authority ustawionym na Issuer Auth.
- Policies i resource checks w handlerach.

Customers module:
- Odbiera UserRegistered event i tworzy rekord klienta.
- Idempotentny handler (np. dedupe po UserId).

## 3. Flow logowania (SPA)

1) SPA redirectuje usera do /connect/authorize (Authorization Code + PKCE).
2) User loguje sie w Auth UI (cookie auth).
3) Auth zwraca kod do redirect_uri SPA.
4) SPA wymienia kod na tokeny w /connect/token (PKCE).
5) SPA uzywa access token w Authorization: Bearer ...
6) Refresh token rotowany przy odswiezeniu.

## 4. Model danych (MSSQL)

Schemat: Auth

- Identity tables: AspNetUsers, AspNetRoles, AspNetUserRoles, ...
- OpenIddict tables: OpenIddictApplications, OpenIddictAuthorizations,
  OpenIddictScopes, OpenIddictTokens

Uwagi:
- Klucz uzytkownika: Guid.
- Email i UserName (login) w Identity.
- EmailConfirmed na start false (bez wymagania).

## 5. Konfiguracja (appsettings)

Sekcje (przyklad):

"Auth": {
  "Issuer": "https://localhost:5001",
  "AccessTokenMinutes": 15,
  "RefreshTokenDays": 30,
  "RequireEmailConfirmed": false,
  "Clients": [
    {
      "ClientId": "guitarstore-spa",
      "DisplayName": "GuitarStore SPA",
      "RedirectUris": ["http://localhost:3000/auth/callback"],
      "PostLogoutRedirectUris": ["http://localhost:3000/auth/logout-callback"]
    }
  ],
  "Password": {
    "RequiredLength": 8,
    "RequireDigit": true,
    "RequireLowercase": true,
    "RequireUppercase": true,
    "RequireNonAlphanumeric": true
  },
  "Lockout": {
    "MaxFailedAccessAttempts": 5,
    "DefaultLockoutMinutes": 10
  }
}

"Cors": {
  "AllowedOrigins": ["http://localhost:3000"]
}

"SeedAdmin": {
  "Enabled": true,
  "Email": "admin@guitarstore.local",
  "Password": "ChangeMe!123"
}

Struktura plikow konfiguracyjnych:
- appsettings.json (globalne)
- appsettings.Auth.json (Auth)
- appsettings.Customers.json (Customers)
- appsettings.Orders.json (Orders)
... itd

ApiGateway laduje wszystkie pliki i mapuje sekcje modulowe do DI.

Produkcja:
- SeedAdmin.Enabled = false
- Bootstrap admin: tylko gdy env var ADMIN_BOOTSTRAP_SECRET ustawiony

## 6. Role i uprawnienia

Role:
- user
- support
- admin

Proponowane policies (permission claims):
- Catalog.Manage (admin)
- Orders.ViewAny (support, admin)
- Orders.CancelAny (support, admin) - tylko PendingPayment
- Customers.ViewAny (support, admin)

Resource-based check (order ownership):
- Orders.ViewOwn (user) -> order.CustomerId == current userId

Mapping role->permissions w kodzie (seed lub konfiguracja).

## 7. Endpointy

### OpenIddict

Standardowe endpointy protokolu:
- `GET /.well-known/openid-configuration`
- `GET/POST /connect/authorize`
- `POST /connect/token`
- `GET/POST /connect/logout`

### Account UI w ApiGateway

Endpointy interaktywnego UI:
- `GET /auth/login`
- `POST /auth/login`
- `GET /auth/register`
- `POST /auth/register`
- `POST /auth/logout`
- `GET /auth/forbidden` (opcjonalnie)

### API Gateway

Zabezpieczenie endpointow API:
- `[Authorize]` w kontrolerach
- policies na akcjach wymagajacych uprawnien `admin` lub `support`

## 8. Integracja z Customers (event-driven)

Event: UserRegistered (lub mapowanie na istniejacy SignedUpEvent)
Payload:
- UserId (Guid)
- Email
- OccurredAtUtc

Customers handler:
- If Customer with UserId exists -> no-op.
- Else create Customer.

UWAGA: Obecny model Customers wymaga Name/LastName.
Opcje do decyzji:
Wybrana opcja:
1) Rozszerzyc rejestracje w Auth UI o Name/LastName.
3) Wprowadzic CustomerDraft i wymuszenie uzupelnienia profilu przed checkout.

Rekomendacja (opcjonalnie): Outbox w Auth dla niezawodnosci eventu.

## 9. Seed admin (dev/local) i bootstrap (prod)

Dev/local:
- SeedAdmin.Enabled = true
- Tworzy role i admina przy starcie, jesli nie istnieje.

Prod (standard):
- SeedAdmin.Enabled = false
- Bootstrap only once:
  - Jesli brak admina i env var ADMIN_BOOTSTRAP_SECRET jest ustawiony,
    tworzy admina z haslem jednorazowym i wymusza zmiane hasla przy pierwszym logowaniu.
  - Po utworzeniu admina - mechanizm nie dziala ponownie.

## 10. Security hardening (MVP)

- HTTPS required w prod.
- PKCE wymagane dla SPA.
- CORS tylko dla znanych originow.
- Cookies: SameSite=Lax/Strict, Secure w prod.
- Lockout po nieudanych probach.
- Refresh token rotation + revoke on reuse.
- Access token krotki (konfigurowalny).
- Rate limiting na /connect/token (opcjonalnie w kolejnej iteracji).

## 11. Plan implementacji (etapy)

### Etap 0: Konfiguracja per modul
- Dodac mechanizm ladowania appsettings.<Module>.json w ApiGateway.
- Zdefiniowac sekcje Auth i mapowanie do DI.

### Etap 1: Skeleton Auth module
- Dodac Auth module (projekt) + schema Auth.
- Dodac AuthDbContext (Identity + OpenIddict EF Core).
- Dodac migracje.
- Usunac Duende IdentityServer (pakiety, DuendeConfig, AddIdentityServer/UseIdentityServer).

### Etap 2: OpenIddict konfiguracja
- Configure OpenIddict:
  - authorization code + PKCE
  - refresh token + rotation
  - JWT tokens
  - endpoints + discovery
- Skonfigurowac signing/encryption certs (dev self-signed).

### Etap 3: Account UI
- Razor/MVC UI w ApiGateway: login, register, logout.
- Integracja z Identity.
- Walidacje hasel wg policy.
- TODO: email confirmation

### Etap 4: SPA client registration
- Zarejestrowac SPA client w OpenIddict (public client, no secret).
- Redirect URIs z configu.

Stan po wdrozeniu:
- klient SPA jest seedowany przy starcie aplikacji,
- konfiguracja klienta siedzi w `Auth:Clients`,
- klient jest typu public,
- flow to authorization code + PKCE + refresh token.

### Etap 5: Roles i policies
- Seed roles: user, support, admin.
- Map role->permissions (claims) i policies.
- Dodac [Authorize(Policy=...)] w API.

### Etap 6: Customers integration
- Emit UserRegistered (lub SignedUpEvent) po rejestracji.
- Customers handler tworzy Customer zgodnie z decyzja o Name/LastName.
- Idempotencja.

### Etap 7: Seed admin
- Dev/local seed admin z appsettings.
- Prod bootstrap once z env var.

### Etap 8: Testy
- Unit: Identity rules, password policy, lockout.
- Integration: OIDC auth code flow + refresh token rotation.
- Integration: UserRegistered -> Customer created.

## 12. Definition of Done

- Auth module dziala lokalnie.
- SPA moze zalogowac sie przez OIDC (code + PKCE).
- JWT bearer dziala w API.
- Refresh token rotation dziala (reuse = revoke).
- Role i policies dzialaja.
- UserRegistered tworzy Customer.
- Seed admin w dev/local.

## 13. Ryzyka / TODO

- Email confirmation (TODO).
- MFA (TODO).
- Outbox w Auth (opcjonalnie).
- Rate limiting na auth endpoints (opcjonalnie).


