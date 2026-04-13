# Auth Implementation 8

Data: 2026-04-07
Status: Draft

## 0. Cel kroku 8

Krok 8 domyka sekcje `szersze testy auth code flow i refresh rotation` z `.ai/Auth_Flow.md`.

Po tym kroku system ma miec:
- pelne E2E potwierdzenie happy path `authorization code + PKCE`,
- test wydania tokenow z poprawnego `authorization_code`,
- test odswiezenia tokenow przez `refresh_token`,
- test potwierdzajacy rotation refresh tokena,
- testy negatywne dla ponownego uzycia starego refresh tokena i dla blednych parametrow token endpointu.

Krok 8 nie powinien jeszcze obejmowac:
- MFA,
- email confirmation,
- hardening typu `must change password`,
- dodatkowego UI klienta OIDC,
- niestandardowych grant types.

## 1. Wejscie do kroku 8

Zakladany stan po kroku 7:
- Auth ma Identity + OpenIddict,
- login/register/logout dzialaja,
- role i permission claims sa seedowane,
- klient SPA jest seedowany przy starcie,
- seed/bootstrap admina jest gotowy,
- istnieja podstawowe testy E2E dla discovery, klienta, UI login i prostych walidacji token endpointu.

Aktualny stan repo istotny dla kroku 8:
- `OpenIddictController.Authorize()` obsluguje authorize flow i challenge do loginu,
- `/connect/token` jest obslugiwany przez OpenIddict server middleware,
- klient testowy `guitarstore-spa` ma permission na `authorization_code`, `refresh_token` i PKCE,
- istnieja testy:
  - `AuthorizeEndpointTest` tylko dla redirectu na login,
  - `TokenEndpointValidationTest` tylko dla braku `grant_type`,
  - `AccountControllerTest` tylko dla UI login/register,
  - `OidcPrincipalFactoryTest` dla claims principal.

Wniosek:
- fundament auth flow jest juz gotowy,
- brakuje testow spinajacych caly przebieg OIDC end-to-end,
- krok 8 to glownie rozbudowa testow i pomocniczych helperow testowych.

## 2. Zakres

Krok 8 obejmuje:
- rozszerzenie testow E2E w `Tests.EndToEnd/E2E_Auth`,
- dodanie helperow do wykonania loginu UI i przechwycenia `authorization_code`,
- dodanie helperow do exchange `code -> tokens`,
- dodanie helperow do `refresh_token -> new tokens`,
- weryfikacje odpowiedzi JSON i podstawowych claimow / tokenow,
- sprawdzenie refresh token rotation oraz odrzucenia reuse starego refresh tokena.

Krok 8 nie obejmuje:
- zmian w kontraktach API biznesowego,
- budowy frontendowego klienta SPA,
- zmian w modelu domenowym Auth,
- przebudowy konfiguracji OpenIddict bez wyraznej potrzeby.

## 3. Kluczowe decyzje

### 3.1 Krok 8 powinien pozostac na poziomie E2E

Rekomendacja:
- nie robic z tego unit testow,
- testowac realny przebieg przez HTTP, cookies, redirecty i middleware OpenIddict.

Powod:
- wartosc kroku 8 lezy w weryfikacji integracji Identity, cookie auth, authorize endpointu i token endpointu,
- unit testy nie pokaza, czy realny flow OIDC dziala od poczatku do konca.

### 3.2 Testowac realny PKCE zamiast "udawanego" code challenge

Rekomendacja:
- w testach generowac:
  - `code_verifier`,
  - `code_challenge` dla `S256`,
- wymiane kodu na token wykonywac z realnym `code_verifier`.

Powod:
- klient SPA jest skonfigurowany jako public client z wymaganym PKCE,
- test powinien potwierdzic, ze flow dziala tak, jak bedzie uzywany produkcyjnie.

### 3.3 Nie parsowac samych redirect URL-i ad hoc w kazdym tescie

Rekomendacja:
- dodac jeden lub dwa helpery testowe:
  - `OidcAuthorizeFlowTestHelper`,
  - opcjonalnie `OidcTokenResponse`.

Powod:
- bez helpera testy beda pelne powtarzalnego kodu:
  - GET authorize,
  - redirect do login,
  - GET login,
  - anti-forgery token,
  - POST login,
  - follow redirect,
  - wyciagniecie `code` z callback URL.

### 3.4 Rotation refresh tokena trzeba zweryfikowac kontraktowo, nie implementacyjnie

Rekomendacja:
- nie testowac wewnetrznych tabel OpenIddict, o ile nie bedzie to konieczne do diagnostyki,
- testowac obserwowalne zachowanie HTTP:
  - pierwszy refresh zwraca nowy refresh token,
  - ponowne uzycie starego refresh tokena konczy sie bledem,
  - nowy refresh token pozostaje poprawny.

Powod:
- to jest najstabilniejszy kontrakt biznesowo-techniczny,
- szczegoly storage OpenIddict sa detalem implementacyjnym.

## 4. Docelowy zestaw scenariuszy testowych

### 4.1 Authorization code happy path

Scenariusz:
- utworz usera testowego,
- wejdz na `/connect/authorize` z poprawnym:
  - `client_id`,
  - `redirect_uri`,
  - `response_type=code`,
  - `scope=openid profile offline_access`,
  - `state`,
  - `nonce`,
  - `code_challenge`,
  - `code_challenge_method=S256`,
- aplikacja przekierowuje na login,
- po poprawnym loginie browser wraca redirectem na `redirect_uri`,
- callback URL zawiera:
  - `code`,
  - ten sam `state`.

Weryfikacje:
- `code` nie jest pusty,
- redirect host zgadza sie z klientem testowym,
- `state` jest zachowany.

### 4.2 Code exchange returns tokens

Scenariusz:
- wykorzystaj `authorization_code` z poprzedniego flow,
- wykonaj POST `/connect/token` z:
  - `grant_type=authorization_code`,
  - `client_id`,
  - `code`,
  - `redirect_uri`,
  - `code_verifier`.

Weryfikacje:
- status `200 OK`,
- odpowiedz zawiera:
  - `token_type = Bearer`,
  - `access_token`,
  - `refresh_token`,
  - opcjonalnie `id_token`,
  - `expires_in`,
  - `scope`.

### 4.3 Refresh token returns rotated tokens

Scenariusz:
- wykonaj refresh z otrzymanym `refresh_token`,
- POST `/connect/token` z:
  - `grant_type=refresh_token`,
  - `client_id`,
  - `refresh_token`.

Weryfikacje:
- status `200 OK`,
- nowy `access_token` jest wydany,
- nowy `refresh_token` jest wydany,
- nowy `refresh_token` rozni sie od poprzedniego.

### 4.4 Reuse old refresh token is rejected

Scenariusz:
- po udanym refreshu sproboj ponownie uzyc starego refresh tokena.

Weryfikacje:
- odpowiedz to `400 BadRequest`,
- payload zawiera `error`,
- kod bledu wskazuje odmowe invalid/used token.

Uwaga:
- konkretna wartosc `error_description` moze zalezec od OpenIddict,
- dlatego test powinien byc rygorystyczny wobec statusu i `error`,
- ale ostrozny wobec pelnego tekstu opisu.

### 4.5 Invalid PKCE verifier is rejected

Scenariusz:
- sproboj wymienic poprawny `authorization_code` z blednym `code_verifier`.

Weryfikacje:
- `400 BadRequest`,
- `error = invalid_grant` albo rownowazny kontrakt OpenIddict.

### 4.6 Authorization code is single-use

Scenariusz:
- po poprawnym exchange sproboj drugi raz uzyc tego samego `authorization_code`.

Weryfikacje:
- druga proba jest odrzucona,
- endpoint zwraca blad `invalid_grant` albo rownowazny.

## 5. Plan implementacji technicznej

### Etap 8.1: Dodac helper do testowego authorize flow

Dodac nowy helper w `Tests.EndToEnd/E2E_Auth`, np.:
- `OidcAuthorizationCodeFlowTestHelper.cs`

Odpowiedzialnosc:
- zbudowac authorize URL,
- przejsc przez login UI,
- pobrac anti-forgery token,
- zalogowac usera,
- przechwycic redirect na SPA callback,
- zwrocic:
  - `authorization_code`,
  - `state`,
  - `redirect_uri`,
  - `code_verifier`.

Rekomendacja:
- helper ma byc maly i ukryc tylko mechanike HTTP,
- same asercje maja pozostac w testach.

### Etap 8.2: Dodac model odpowiedzi token endpointu

Dodac prosty DTO testowy, np.:
- `OidcTokenResponse`

Pola:
- `access_token`,
- `refresh_token`,
- `id_token`,
- `token_type`,
- `expires_in`,
- `scope`,
- `error`,
- `error_description`.

Powod:
- uproszczenie odczytu JSON i czytelniejsze asercje.

### Etap 8.3: Dodac helper do wywolan `/connect/token`

Dodac helper lub prywatne metody testowe do:
- exchange authorization code,
- refresh token.

Powinny:
- ustawic `Accept: application/json`,
- wysylac `FormUrlEncodedContent`,
- zwracac:
  - `HttpResponseMessage`,
  - sparsowany `OidcTokenResponse`.

### Etap 8.4: Rozszerzyc pakiet testow E2E_Auth

Dodac nowa klase, np.:
- `AuthorizationCodeFlowTest.cs`

Minimalny zestaw testow:
1. `AuthorizeAndTokenExchange_ShouldReturnTokens`
2. `RefreshToken_ShouldRotateRefreshToken`
3. `RefreshTokenReuse_ShouldBeRejected`
4. `AuthorizationCode_WithInvalidVerifier_ShouldBeRejected`
5. `AuthorizationCode_ReusedSecondTime_ShouldBeRejected`

Rekomendacja:
- trzymac obecny `AuthorizeEndpointTest` jako test prostego redirectu dla niezalogowanego usera,
- pelny flow umiescic w nowej klasie, zamiast rozbudowywac obecny test jednym ogromnym plikiem.

### Etap 8.5: Dodac jawne test data setup dla usera OIDC

Wykorzystac istniejacy:
- `AuthTestDataSeeder.EnsureUserAsync(...)`

Jesli bedzie potrzebne, dodac cienki helper specjalizowany do auth flow, ale bez duplikowania logiki seeda.

### Etap 8.6: Ewentualne doprecyzowanie konfiguracji testowej

Na start nie zmieniac konfiguracji OpenIddict.

Jesli testy pokaza niestabilnosc:
- dopiero wtedy rozważyć override konfiguracji testowej,
- np. jawne skrocenie TTL albo doprecyzowanie opcji refresh tokenow.

Rekomendacja:
- najpierw sprawdzic domyslne zachowanie OpenIddict na obecnej konfiguracji.

## 6. Proponowana struktura plikow

Nowe lub zmieniane miejsca:
- `Tests.EndToEnd/E2E_Auth/AuthorizationCodeFlowTest.cs`
- `Tests.EndToEnd/E2E_Auth/OidcAuthorizationCodeFlowTestHelper.cs`
- opcjonalnie `Tests.EndToEnd/E2E_Auth/OidcTokenResponse.cs`
- ewentualnie drobne dopiski w `Tests.EndToEnd/E2E_Auth/AuthUiTestHelpers.cs`

Bez zmian funkcjonalnych po stronie produkcyjnej, chyba ze testy ujawnia realny bug.

## 7. Kryteria asercji

Testy powinny asercyjnie potwierdzac:
- poprawne redirecty i zachowanie `state`,
- wydanie `authorization_code`,
- wydanie `access_token` i `refresh_token`,
- dzialanie `refresh_token` grant,
- rotation refresh tokena,
- odrzucenie reuse starego refresh tokena,
- odrzucenie zlego `code_verifier`,
- odrzucenie ponownego uzycia tego samego `authorization_code`.

Testy nie powinny:
- porownywac pelnych token stringow poza roznica starego i nowego refresh tokena,
- zalezec od niestabilnych opisow bledu 1:1,
- wymuszac dokladnego skladu wszystkich claimow w zakodowanym JWT, jesli nie jest to cel kroku.

## 8. Ryzyka i otwarte decyzje

### 8.1 Domyslne zachowanie refresh rotation musi byc potwierdzone testem

Istnieje ryzyko, ze obecna konfiguracja OpenIddict:
- juz robi rotation poprawnie,
- albo wymaga dodatkowej opcji.

Rekomendacja:
- najpierw napisac test,
- jesli test ujawni brak rotation, dopiero wtedy dopisac minimalna zmiane produkcyjna i udokumentowac ja jako czesc kroku 8.

### 8.2 Testy beda bardziej zlozone od obecnych E2E Auth

Najwiekszy koszt techniczny:
- obsluga cookies i redirectow,
- przejscie przez anty-forgery login UI,
- parsowanie callback URL.

Dlatego helper testowy nie jest opcjonalnym luksusem, tylko warunkiem utrzymania czytelnosci.

### 8.3 Token payload moze sie roznic miedzy wersjami OpenIddict

Rekomendacja:
- asercje utrzymac na poziomie kontraktu HTTP/OIDC,
- nie przywiazywac testow do niestabilnych szczegolow formatu odpowiedzi bardziej niz to konieczne.

## 9. Definition of Done

Krok 8 uznajemy za zamkniety, gdy:
- istnieje E2E test happy path dla `authorization_code + PKCE`,
- istnieje E2E test poprawnej wymiany `code -> tokens`,
- istnieje E2E test `refresh_token` flow,
- istnieje E2E test potwierdzajacy refresh token rotation,
- istnieje E2E test odrzucenia reuse starego refresh tokena,
- istnieje E2E test odrzucenia blednego `code_verifier`,
- helpery testowe utrzymuja testy czytelne i bez nadmiernej duplikacji,
- wszystkie testy `Tests.EndToEnd/E2E_Auth` przechodza stabilnie.

## 10. Rekomendowana kolejnosc implementacji

1. Dodac helper do authorize flow z loginem i PKCE.
2. Dodac DTO i helper do `/connect/token`.
3. Napisac happy path `authorize -> code -> token`.
4. Napisac test `refresh_token` flow.
5. Napisac test rotation i odrzucenia reuse starego refresh tokena.
6. Napisac test blednego `code_verifier`.
7. Napisac test single-use dla `authorization_code`.
8. Dopiero jesli testy wykryja bug, wprowadzic minimalna poprawke produkcyjna w konfiguracji lub endpointach.
