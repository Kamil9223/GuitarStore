# Auth Endpoints

Ten dokument opisuje stan po wdrozeniu kroku 3.

Aktualny podzial odpowiedzialnosci:
- OpenIddict publikuje discovery i obsluguje token endpoint,
- ApiGateway hostuje UI `/auth/*`,
- ApiGateway ma cienka obsluge interaktywnego browser flow dla `/connect/authorize` i `/connect/logout`.

Wazne doprecyzowanie architektoniczne:
- fizycznie wszystko jest hostowane w jednym procesie `GuitarStore.ApiGateway`,
- logicznie ten host pelni dwie role:
  - authorization server dla `/auth/*` i `/connect/*`,
  - resource API dla endpointow biznesowych.

Dlatego sformulowania typu "idzie do API" w tym dokumencie oznaczaja:
- idzie do endpointow biznesowych resource API,
- a nie do endpointow auth/protocol endpoints.

## 1. Standardowe endpointy OpenIddict

### `GET /.well-known/openid-configuration`

To jest discovery endpoint OpenID Connect.

Co robi:
- zwraca metadata serwera OIDC,
- publikuje `issuer`, endpoint URIs, grant types, scopes i response types.

Skad bierze dane:
- `SetIssuer(...)`
- `SetAuthorizationEndpointUris(...)`
- `SetTokenEndpointUris(...)`
- `SetEndSessionEndpointUris(...)`
- `AllowAuthorizationCodeFlow()`
- `AllowRefreshTokenFlow()`
- `RegisterScopes(...)`

Ten endpoint nie ma naszego kontrolera. Wystawia go sam OpenIddict.

### `POST /connect/token`

To jest token endpoint OAuth2/OIDC.

Co robi:
- waliduje request tokenowy,
- zwraca bledy protokolowe dla niepoprawnych requestow,
- po pelnym authorize flow obsluguje wymiane authorization code i refresh token.

Na tym etapie:
- endpoint nadal jest obslugiwany przez OpenIddict,
- nie ma wlasnego kontrolera MVC,
- test walidacyjny sprawdza kontrakt `invalid_request` dla pustego requestu.

Skad to wiadomo w kodzie:
- `SetTokenEndpointUris("/connect/token")` rejestruje token endpoint w OpenIddict,
- w `UseAspNetCore()` nie ma `EnableTokenEndpointPassthrough()`,
- to znaczy, ze request nie jest przepuszczany do naszego MVC controllera i zostaje obsluzony przez sam OpenIddict.

Istotny fragment konfiguracji:

```csharp
options.SetAuthorizationEndpointUris("/connect/authorize");
options.SetTokenEndpointUris("/connect/token");
options.SetEndSessionEndpointUris("/connect/logout");

options.UseAspNetCore()
    .EnableAuthorizationEndpointPassthrough()
    .EnableEndSessionEndpointPassthrough();
```

Konsekwencja:
- `/connect/token` jest wystawiony i dziala,
- ale nie wymaga naszej akcji kontrolera,
- OpenIddict sam waliduje `grant_type`, `code`, `refresh_token`, `redirect_uri`, `client_id`, `code_verifier` i sam zwraca odpowiedz tokenowa.

## 2. Endpointy interaktywne dodane w kroku 3

### `GET /auth/login`

Co robi:
- zwraca widok Razor z formularzem logowania,
- zachowuje `returnUrl`, zeby po zalogowaniu wrocic do authorize flow albo lokalnej strony.

Po co:
- to glowny ekran wejscia dla browser login flow,
- cookie challenge z `/connect/authorize` przekierowuje wlasnie tutaj.

### `POST /auth/login`

Co robi:
- waliduje formularz,
- szuka usera po emailu albo user name,
- wywoluje `SignInManager.PasswordSignInAsync(...)`,
- przy sukcesie zapisuje cookie Identity,
- robi redirect do bezpiecznego `returnUrl` albo na `/`.

Uwagi:
- uzywa lockout policy skonfigurowanej w Identity,
- login UI jest server-side i dziala na cookie auth.

### `GET /auth/register`

Co robi:
- zwraca widok Razor z formularzem rejestracji.

Zakres kroku 3:
- rejestracja tworzy konto Auth,
- nie dotyka jeszcze Customers integration.

### `POST /auth/register`

Co robi:
- waliduje formularz,
- tworzy usera w ASP.NET Core Identity,
- ustawia `UserName = Email`,
- loguje usera przez cookie,
- redirectuje do bezpiecznego `returnUrl` albo na `/`.

Czego jeszcze nie robi:
- nie emituje eventu do Customers,
- nie zbiera jeszcze `Name/LastName`,
- nie przypisuje jeszcze roli `user` z osobnego seeda kroku 5.

### `POST /auth/logout`

Co robi:
- usuwa cookie Identity przez `SignInManager.SignOutAsync()`,
- redirectuje do lokalnego `returnUrl` albo na `/`.

To jest lokalny logout browser session, niezalezny od protokolu OIDC end-session.

### `GET /auth/forbidden`

Co robi:
- zwraca widok odmowy dostepu.

Po co:
- sluzy jako ekran UI dla browser flow, gdy account nie moze przejsc dalej.

## 3. Cienka obsluga browser flow OIDC

### `GET/POST /connect/authorize`

Ten endpoint ma teraz nasz cienki kontroler.

Co robi krok po kroku:
1. Pobiera request OIDC z `HttpContext.GetOpenIddictServerRequest()`.
2. Probuje odczytac cookie Identity przez jawne `AuthenticateAsync(IdentityConstants.ApplicationScheme)`.
3. Jesli user nie ma cookie:
   - robi `Challenge(...)` na cookie scheme,
   - przegladarka trafia na `/auth/login` z `ReturnUrl` do tego samego authorize requestu.
4. Jesli cookie istnieje:
   - pobiera usera z `UserManager`,
   - sprawdza `CanSignInAsync(...)`,
   - buduje `ClaimsPrincipal` dla OpenIddict w osobnym serwisie,
   - zwraca `SignIn(..., OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)`.
5. OpenIddict konczy protokol i wydaje authorization code.

Dlaczego kontroler istnieje:
- bo browser authorize flow wymaga interakcji z UI i cookie,
- ale logika protokolowa nadal zostaje po stronie OpenIddict,
- kontroler jest cienki i deleguje budowe principal do osobnego serwisu.

Skad to wiadomo w kodzie:
- `SetAuthorizationEndpointUris("/connect/authorize")` rejestruje endpoint po stronie OpenIddict,
- `EnableAuthorizationEndpointPassthrough()` przepuszcza request dalej do ASP.NET Core,
- dlatego mozemy i musimy zaimplementowac cienka akcje MVC, ktora obsluzy browser session i cookie.

Istotny fragment konfiguracji:

```csharp
options.SetAuthorizationEndpointUris("/connect/authorize");

options.UseAspNetCore()
    .EnableAuthorizationEndpointPassthrough()
    .EnableEndSessionEndpointPassthrough();
```

Istotny fragment kontrolera:

```csharp
[AcceptVerbs("GET", "POST")]
[Route("~/connect/authorize")]
public async Task<IActionResult> Authorize()
```

Konsekwencja:
- OpenIddict nadal rozpoznaje request jako OIDC authorize request,
- ale oddaje nam sterowanie, zebysmy mogli:
  - odczytac cookie Identity,
  - przekierowac na `/auth/login`,
  - po zalogowaniu zbudowac `ClaimsPrincipal`,
  - oddac sterowanie z powrotem do OpenIddict przez `SignIn(..., OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)`.

### `GET/POST /connect/logout`

Ten endpoint ma teraz nasz cienki kontroler.

Co robi:
- wylogowuje cookie Identity,
- jesli request jest OIDC end-session requestem, oddaje sterowanie OpenIddict do dokonczenia logout flow,
- w prostym lokalnym przypadku redirectuje na `/`.

## 4. Co robi `/.well-known/openid-configuration`, skoro nie ma go w kontrolerze

Nadal to samo:
- nie jest z kontrolera,
- publikuje go OpenIddict,
- kontrolery ApiGateway odpowiadaja tylko za ta czesc flow, ktora wymaga naszej interakcji z UI i cookie.

## 5. Czemu trasy kontrolerow maja `~/...`

W ASP.NET Core `~/...` oznacza trase absolutna od root aplikacji.

Przyklad:
- `[HttpGet("~/auth/login")]`
- `[Route("~/connect/authorize")]`

Po co:
- endpoint ma byc dokladnie pod standardowa sciezka,
- bez dopinania nazwy kontrolera albo prefiksu MVC.

## 6. Czemu jest Razor, a nie HTML string w kontrolerze

Po kroku 3 UI jest juz zrobione poprawnie:
- jako widoki Razor,
- z modelami widokow,
- z walidacja formularzy,
- bez recznego skladania HTML w kontrolerze.

To bylo glownym celem oczyszczenia po poprzedniej wersji.

## 7. Skad jest certyfikat

W dev:
- `AddDevelopmentEncryptionCertificate()`
- `AddDevelopmentSigningCertificate()`

To znaczy:
- certyfikaty dev nie sa w repo,
- OpenIddict generuje i utrzymuje je lokalnie.

W prod:
- nadal sa ladowane z konfiguracji,
- z `.pfx` albo z certificate store przez thumbprint.

## 8. Jak dziala cookie

Cookie jest wystawiane przez ASP.NET Core Identity po:
- `POST /auth/login`
- `POST /auth/register`

Jak jest uzywane:
- przegladarka zapisuje cookie,
- przy kolejnym `/connect/authorize` cookie wraca do serwera,
- kontroler authorize odczytuje je jawnie przez `AuthenticateAsync(IdentityConstants.ApplicationScheme)`.

Do czego cookie nie sluzy:
- nie sluzy do autoryzacji API,
- API nadal uzywa bearer tokena.

Wazne doprecyzowanie:
- cookie nie jest authorization code,
- cookie nie jest access tokenem,
- cookie tylko potwierdza, ze browser ma sesje na authorization serverze.

Sekwencja jest taka:
- `POST /auth/login` wystawia cookie Identity,
- kolejne wejscie na `/connect/authorize` wykorzystuje to cookie,
- dopiero wtedy OpenIddict wydaje authorization code,
- dopiero `POST /connect/token` zamienia authorization code na tokeny.

## 10. Dokladny request chain

### 10.1 Start

Klient OIDC kieruje browser na:

`GET /connect/authorize?client_id=...&redirect_uri=...&response_type=code&scope=openid%20profile%20offline_access&code_challenge=...&code_challenge_method=S256&state=...&nonce=...`

### 10.2 Wejscie na `/connect/authorize`

Request trafia do naszego `OpenIddictController.Authorize()`, bo:
- endpoint jest zarejestrowany w OpenIddict,
- ale `EnableAuthorizationEndpointPassthrough()` przepuszcza go do MVC.

Kontroler:
- pobiera request OIDC przez `HttpContext.GetOpenIddictServerRequest()`,
- probuje odczytac cookie Identity przez `AuthenticateAsync(IdentityConstants.ApplicationScheme)`.

### 10.3 Brak cookie

Jesli cookie nie ma:
- kontroler robi `Challenge(...)` na `IdentityConstants.ApplicationScheme`,
- Identity zna `LoginPath = "/auth/login"`,
- browser dostaje redirect na `/auth/login?ReturnUrl=...`.

### 10.4 Login

User wysyla:
- `GET /auth/login`
- `POST /auth/login`

Przy sukcesie:
- `PasswordSignInAsync(...)` zapisuje cookie Identity,
- serwer zwraca `302` do pierwotnego `ReturnUrl`,
- czyli z powrotem na `/connect/authorize?...`.

### 10.5 Powrot na authorize

Browser wchodzi jeszcze raz na `/connect/authorize?...`, ale tym razem z cookie Identity.

Kontroler:
- odczytuje cookie,
- pobiera usera,
- sprawdza `CanSignInAsync(...)`,
- buduje `ClaimsPrincipal` dla OpenIddict,
- zwraca `SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme)`.

### 10.6 Wydanie authorization code

Po `SignIn(...)` sterowanie przejmuje OpenIddict:
- konczy authorize flow,
- generuje authorization code,
- redirectuje na `redirect_uri` klienta z `code=...&state=...`.

To jest moment wydania authorization code.

### 10.7 Wymiana code na tokeny

Klient wysyla:

`POST /connect/token`

Typowy payload:
- `grant_type=authorization_code`
- `code=...`
- `redirect_uri=...`
- `client_id=...`
- `code_verifier=...`

OpenIddict:
- waliduje code,
- waliduje PKCE,
- waliduje klienta i redirect,
- zwraca access token i refresh token.

### 10.8 API request

Klient uzywa:

`Authorization: Bearer <access_token>`

API waliduje bearer token przez OpenIddict Validation.

## 11. Cookie vs code vs tokeny

| Artefakt | Kto wystawia | Gdzie zyje | Do czego sluzy | Czy idzie do API |
| --- | --- | --- | --- | --- |
| Identity cookie | ASP.NET Core Identity | Browser + auth server | Utrzymanie sesji browserowej po loginie | Nie |
| Authorization code | OpenIddict | Krotko po redirect flow | Jednorazowa wymiana na tokeny | Nie |
| Access token | OpenIddict | Client aplikacyjny | Autoryzacja wywolan API | Tak |
| Refresh token | OpenIddict | Client aplikacyjny | Odnowienie access tokena | Nie, nie do resource API |
| ID token | OpenIddict | Client aplikacyjny | Informacja o uwierzytelnieniu usera dla klienta OIDC | Nie do resource API |

Najwazniejsze:
- cookie sluzy do browser session,
- authorization code sluzy do przejscia z browser session do tokenow,
- access token sluzy do API,
- to sa trzy rozne role.

Co znaczy "Czy idzie do API":
- nie chodzi o to, czy request fizycznie trafia do hosta `GuitarStore.ApiGateway`,
- bo wszystkie requesty trafiaja do tego samego hosta,
- chodzi o to, czy dany artefakt sluzy do wywolywania endpointow biznesowych resource API.

W praktyce:
- Identity cookie idzie do endpointow browser/auth jak `/auth/login` albo `/connect/authorize`,
- authorization code idzie tylko do `/connect/token`,
- refresh token idzie tylko do `/connect/token`,
- access token idzie do endpointow biznesowych chronionych bearer auth.

## 9. Jak to wszystko dziala razem po kroku 3

Po kroku 3 mamy:
- Identity jako user store i cookie auth,
- Razor UI dla login/register/logout/forbidden,
- cienki browser flow dla `/connect/authorize` i `/connect/logout`,
- OpenIddict jako discovery + token issuance + protocol engine,
- validation bearer tokenow dla API.

Po kroku 3 nadal nie mamy:
- finalnej rejestracji klienta SPA w runtime aplikacji,
- Customers integration po rejestracji,
- seed roles/policies,
- seed admin.

To zostaje w kolejnych krokach planu.
