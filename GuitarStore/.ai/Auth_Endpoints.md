# Auth Endpoints

Ten dokument opisuje stan po poprawnym, czystym kroku 2 z `.ai/Auth.md`.

Najwazniejsza zasada:
- krok 2 konczy sie na konfiguracji Identity + OpenIddict,
- krok 3 dopiero dodaje UI `/auth/*` i pelny browser login flow.

To oznacza, ze w kroku 2 nie mamy juz wlasnych kontrolerow dla `/auth/*` ani dla `/connect/*`.

## 1. Jakie endpointy realnie istnieja po kroku 2

Po konfiguracji OpenIddict serwer publikuje standardowe endpointy OIDC/OAuth2:
- `/.well-known/openid-configuration`
- `/connect/authorize`
- `/connect/token`
- `/connect/logout`

Sa one zdefiniowane w konfiguracji OpenIddict, a nie w naszych kontrolerach MVC.

## 2. Co robi kazdy endpoint

### `GET /.well-known/openid-configuration`

To jest discovery endpoint OpenID Connect.

Co robi:
- zwraca metadane serwera OIDC,
- informuje klienta gdzie sa endpointy,
- publikuje `issuer`, wspierane granty, scopes i response types.

Po co:
- klient SPA lub inny klient OIDC moze automatycznie pobrac konfiguracje authorization server,
- test discovery sprawdza wlasnie ten kontrakt.

Skad bierze dane:
- `SetIssuer(...)`
- `SetAuthorizationEndpointUris(...)`
- `SetTokenEndpointUris(...)`
- `SetEndSessionEndpointUris(...)`
- `AllowAuthorizationCodeFlow()`
- `AllowRefreshTokenFlow()`
- `RegisterScopes(...)`

Czyli z konfiguracji OpenIddict w `AuthModuleInitializator`, nie z kontrolera.

### `GET/POST /connect/authorize`

To jest standardowy authorization endpoint OIDC.

W kroku 2:
- endpoint jest skonfigurowany i publikowany w metadata,
- ale nie ma jeszcze naszej warstwy UI i interaktywnego logowania,
- pelna obsluga browser flow jest celowo przesunieta do kroku 3.

Dlaczego tak:
- dokument `.ai/Auth.md` rozdziela te odpowiedzialnosci,
- krok 2 ma zamknac konfiguracje protokolu,
- krok 3 ma dopiero dostarczyc login/register/logout UI i spiecie z cookie.

Praktyczny wniosek:
- endpoint istnieje jako czesc serwera OIDC,
- ale poprawny login interaktywny bedzie domkniety dopiero po implementacji kroku 3.

### `POST /connect/token`

To jest token endpoint OIDC/OAuth2.

Co robi:
- przyjmuje request tokenowy,
- waliduje parametry grantu,
- zwraca odpowiedz zgodna z OIDC/OAuth2,
- po pelnym wdrozeniu flow bedzie obslugiwal wymiane authorization code i refresh token.

W kroku 2:
- endpoint jest wystawiany przez OpenIddict,
- dlatego test `TokenEndpointValidationTest` moze sprawdzic np. blad `invalid_request` dla pustego requestu,
- nie potrzebujemy do tego wlasnego kontrolera.

### `GET/POST /connect/logout`

To jest end-session endpoint OIDC.

W kroku 2:
- endpoint jest skonfigurowany i publikowany w discovery,
- ale sensowny browser logout zalezy od UI i cookie flow z kroku 3.

Czyli tak samo jak przy `authorize`:
- protokol jest skonfigurowany,
- pelna interakcja usera jeszcze nie.

## 3. Co z endpointami `/auth/*`

Po poprawce kroku 2 nie ma jeszcze endpointow:
- `/auth/login`
- `/auth/register`
- `/auth/logout`
- `/auth/forbidden`

To jest celowe.

Powod:
- w `.ai/Auth.md` te endpointy naleza do kroku 3: `Account UI`.
- krok 2 nie powinien budowac prowizorycznego UI ani wciskac logiki do kontrolerow.

## 4. Co robi `/.well-known/openid-configuration`, skoro nie ma go w kontrolerze

To pytanie bylo trafne: ten endpoint nie powinien byc w kontrolerze.

Odpowiedz:
- publikuje go sam OpenIddict,
- dzieje sie to automatycznie po `AddOpenIddict().AddServer(...).UseAspNetCore()`,
- zawartosc bierze z konfiguracji serwera.

Dlatego:
- endpoint jest testowany,
- ale nie ma dla niego naszego pliku kontrolera.

## 5. Czemu wczesniej byly trasy z `~`

W ASP.NET Core `~/...` oznacza trase absolutna od root aplikacji.

Przyklad:
- `[HttpGet("~/auth/login")]`

Znaczenie:
- nie doklejaj nazwy kontrolera,
- wystaw endpoint dokladnie pod wskazana sciezka.

Po wyczyszczeniu kroku 2 ten temat przestaje byc istotny, bo usuniete zostaly kontrolery z takim routingiem.

## 6. Czemu nie powinno sie budowac HTML w kontrolerze

Budowanie HTML jako stringa w kontrolerze bylo zla forma dla tego projektu.

Dlaczego:
- dokument przewiduje `Razor/MVC UI` w kroku 3,
- backend stringujacy HTML mieszal warstwe protokolu z warstwa prezentacji,
- utrudnial czytelnosc i dalszy rozwoj.

Poprawny kierunek:
- krok 2: konfiguracja auth,
- krok 3: kontrolery + modele widokow + Razor Views.

## 7. Skad jest certyfikat

W dev certyfikat bierze sie z:
- `AddDevelopmentEncryptionCertificate()`
- `AddDevelopmentSigningCertificate()`

To oznacza:
- certyfikat developerski nie siedzi w repo,
- OpenIddict generuje go lokalnie, jesli trzeba,
- jest przeznaczony tylko do developmentu.

W prod:
- kod oczekuje certyfikatow z konfiguracji,
- z pliku `.pfx` albo z certificate store po thumbprincie.

## 8. Jak dziala tutaj cookie

Cookie jest przygotowane juz w kroku 2 przez Identity:
- ustawiony jest application cookie scheme,
- skonfigurowane sa `LoginPath`, `LogoutPath`, `AccessDeniedPath`.

Ale w kroku 2 cookie nie jest jeszcze aktywnie uzywane przez UI, bo same endpointy `/auth/*` pojawia sie dopiero w kroku 3.

Rola cookie w docelowym flow:
- sluzy do interaktywnego loginu przegladarkowego,
- pozwala utrzymac sesje usera podczas authorize flow,
- nie sluzy do autoryzacji API.

API ma uzywac:
- `Authorization: Bearer <access_token>`

Przechowywanie:
- cookie jest przechowywane po stronie klienta, czyli w przegladarce,
- auth ticket jest chroniony przez ASP.NET Core Data Protection.

## 9. Jak to wszystko dziala jako calosc po kroku 2

Po kroku 2 mamy:
- Identity jako store userow, hasel, rol i cookie auth,
- OpenIddict Server jako serwer OIDC/OAuth2,
- OpenIddict Validation jako walidacje bearer tokenow dla API,
- discovery endpoint i token endpoint wystawiane przez OpenIddict,
- konfiguracje certyfikatow, issuera, scopes, lifetimes i endpoint URIs.

Po kroku 2 jeszcze nie mamy:
- UI logowania/rejestracji,
- finalnego browser flow z `/auth/login`,
- dopietego authorize/logout UX.

To jest zgodne z dokumentem:
- krok 2 konfiguruje protokol,
- krok 3 dopiero dodaje interaktywne account UI.
