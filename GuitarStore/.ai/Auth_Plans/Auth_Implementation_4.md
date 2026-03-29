# Auth Implementation 4

Data: 2026-03-26
Status: Implemented

## 0. Cel kroku 4

Krok 4 domyka runtime registration klienta SPA w OpenIddict.

Po tym kroku:
- aplikacja sama rejestruje klienta OIDC przy starcie,
- konfiguracja klienta jest trzymana w `Auth:Clients`,
- testy nie seeduja klienta recznie przez `IOpenIddictApplicationManager`.

## 1. Zakres

Krok 4 obejmuje:
- model konfiguracji klientow OIDC w `AuthOptions`,
- wpis klienta SPA w `appsettings.Auth.json`,
- startup seeding aplikacji OpenIddict,
- test potwierdzajacy, ze klient istnieje po starcie hosta,
- przepiecie istniejacych testow authorize/login na klienta z konfiguracji.

Krok 4 nadal nie obejmuje:
- Customers integration,
- role/policies,
- seed admin,
- refresh token rotation tests end-to-end.

## 2. Decyzje implementacyjne

- klient jest typu `public`,
- nie ma secretu,
- wspiera:
  - authorization endpoint,
  - token endpoint,
  - end-session endpoint,
  - authorization code flow,
  - refresh token flow,
  - PKCE.

- redirect URIs i post-logout redirect URIs pochodza z konfiguracji,
- przy starcie aplikacja:
  - tworzy klienta, jesli go nie ma,
  - aktualizuje klienta, jesli juz istnieje.

## 3. Konfiguracja

Docelowy wpis konfiguracyjny:

```json
"Auth": {
  "Clients": [
    {
      "ClientId": "guitarstore-spa",
      "DisplayName": "GuitarStore SPA",
      "RedirectUris": [
        "http://localhost:3000/auth/callback"
      ],
      "PostLogoutRedirectUris": [
        "http://localhost:3000/auth/logout-callback"
      ]
    }
  ]
}
```

Walidacja przy starcie:
- musi istniec co najmniej jeden klient,
- `ClientId` nie moze byc pusty,
- klient musi miec co najmniej jeden `RedirectUri`,
- URI musza byc absolutne,
- `ClientId` nie moze sie duplikowac.

## 4. Zachowanie przy starcie

Seeder klientow OpenIddict:
- uruchamia sie jako hosted service,
- korzysta z `IOpenIddictApplicationManager`,
- buduje descriptor klienta na podstawie `Auth:Clients`,
- ustawia permissions zgodne z authorization code + PKCE + refresh token,
- robi create albo update.

## 5. Wplyw na testy

Po kroku 4:
- testy nie powinny juz same rejestrowac klienta typu `step3-spa`,
- powinny korzystac z jednego klienta konfiguracyjnego,
- autorize flow testuje runtime registration, a nie testowy seed pomocniczy.

## 6. Definition of Done

Krok 4 uznajemy za zamkniety, gdy:
- klient SPA istnieje w OpenIddict po starcie aplikacji,
- authorize request z tym `client_id` jest akceptowany protokolowo,
- testy auth nie seeduja juz klienta recznie,
- redirect URIs sa konfigurowalne z `appsettings.Auth.json`.
