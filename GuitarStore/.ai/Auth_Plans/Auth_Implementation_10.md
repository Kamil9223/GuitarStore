# Auth Implementation 10

Data: 2026-04-18
Status: Draft

## 0. Po co jest krok 10

Tak, krok 10 jest sensowny i wynika bezposrednio z obecnego stanu repo.

Po kroku 9 Auth ma juz:
- Identity + OpenIddict,
- UI login/register/logout,
- auth code + PKCE + refresh token rotation,
- role i policies,
- Customers integration,
- seed/bootstrap admina,
- `MustChangePassword` z wymuszona zmiana hasla przy pierwszym logowaniu.

Ale nadal brakuje dwoch waznych flow zwiazanych z cyklem zycia konta:
- potwierdzenia emaila,
- odzyskiwania dostepu przez `forgot password` / `reset password`.

To nie jest kosmetyka. W aktualnym kodzie:
- `RequireEmailConfirmed` istnieje w konfiguracji i jest podpiete do Identity,
- ale nie ma endpointow, linkow ani wysylki maili do realnego potwierdzenia adresu,
- rejestracja nadal od razu robi `SignInAsync(...)`,
- nie ma flow resetu hasla dla usera, ktory utracil dostep.

Wniosek:
- krok 10 powinien byc krokiem `account recovery and email verification`,
- i powinien domknac brakujace flow wokol `EmailConfirmed`.

## 1. Stan wejsciowy po kroku 9

Aktualny stan kodu istotny dla kroku 10:
- `AuthModuleInitializator` ustawia `options.SignIn.RequireConfirmedEmail = authOptions.RequireEmailConfirmed`,
- `appsettings.Auth.json` ma `RequireEmailConfirmed = false`,
- `AccountController` obsluguje `login`, `register`, `logout` i `change-password-required`,
- `AuthService.RegisterAsync(...)` tworzy usera, przypisuje role `user`, publikuje `UserRegisteredEvent`, a potem od razu loguje usera,
- `OpenIddictController.Authorize()` wywoluje `CanSignInAsync(user)`,
- w repo nie ma jeszcze zadnej infrastruktury typu `IEmailSender`,
- w repo nie ma endpointow:
  - `/auth/confirm-email`,
  - `/auth/forgot-password`,
  - `/auth/reset-password`,
- w E2E nie ma jeszcze testowego mail sinka.

Najwazniejsza luka:
- sama flaga `RequireEmailConfirmed` nie wystarcza,
- bo bez potwierdzenia emaila i bez maili nie da sie jej realnie wlaczyc.

## 2. Rekomendacja: czym powinien byc krok 10

Krok 10 powinien obejmowac dwa powiazane obszary:

1. `email confirmation`
2. `forgot/reset password`

To powinien byc jeden krok, a nie dwa osobne, bo:
- oba flow opieraja sie na tokenach Identity i linkach wysylanych emailem,
- oba wymagaja tej samej minimalnej infrastruktury mailowej,
- oba sa elementem pelnego lifecycle konta,
- oba logicznie uzupelniaja `RequireEmailConfirmed`.

Krok 10 nie powinien jeszcze obejmowac:
- MFA,
- outbox w Auth,
- rate limiting,
- zmiany modelu uprawnien,
- social login,
- email change flow.

## 3. Cel kroku 10

Po tym kroku system ma miec:
- mozliwosc wyslania maila potwierdzajacego po rejestracji,
- endpoint do potwierdzenia emaila tokenem Identity,
- mozliwosc uruchomienia flow `forgot password`,
- endpoint i UI do ustawienia nowego hasla przez token resetu,
- zgodne zachowanie rejestracji i logowania przy `RequireEmailConfirmed = true`,
- testy E2E potwierdzajace oba flow.

Po kroku 10 powinno byc mozliwe wlaczenie:
- `Auth:RequireEmailConfirmed = true`

bez psucia podstawowego UX i bez dead-endow dla usera.

## 4. Kluczowe decyzje

### 4.1 Nie dodawac nowej tabeli pod tokeny

Rekomendacja:
- uzyc standardowych token providerow ASP.NET Core Identity,
- nie wprowadzac osobnej tabeli ani wlasnego storage dla tokenow confirm/reset.

Powod:
- to jest wbudowany i wystarczajacy mechanizm dla tego etapu,
- ogranicza zakres,
- zmniejsza ryzyko dorobienia niepotrzebnej infrastruktury.

### 4.2 Dodac minimalna abstrakcje wysylki maili

Rekomendacja:
- dodac prosty kontrakt w stylu `IAuthEmailSender` albo `IAuthAccountEmailSender`,
- bez wiazania kroku 10 z konkretnym dostawca SMTP/API.

Kontrakt powinien obsluzyc przynajmniej:
- wysylke maila potwierdzajacego,
- wysylke maila resetu hasla.

Powod:
- produkcja i testy potrzebuja roznych implementacji,
- E2E beda potrzebowaly testowego sinka do przechwycenia linku,
- repo nie ma jeszcze wspolnej infrastruktury mailowej.

### 4.3 Linki w mailach musza byc absolutne

Rekomendacja:
- generowac absolutne linki do:
  - `/auth/confirm-email`,
  - `/auth/reset-password`,
- jako baze wykorzystac publiczny origin hosta auth.

Najprostsza wersja na ten etap:
- oprzec sie o `Auth:Issuer`, bo auth UI i endpointy OIDC sa hostowane w tym samym `ApiGateway`.

Jesli w implementacji okaze sie to zbyt kruche:
- dodac osobna konfiguracje typu `Auth:PublicOrigin`.

### 4.4 Tokeny trzeba kodowac URL-safe

Rekomendacja:
- tokeny z Identity kodowac do `Base64Url`,
- w endpointach odtwarzac oryginalny token przed wywolaniem `ConfirmEmailAsync` / `ResetPasswordAsync`.

Powod:
- surowe tokeny Identity zawieraja znaki problematyczne dla query stringa,
- to klasyczny punkt awarii tych flow.

### 4.5 Przy `RequireEmailConfirmed = true` rejestracja nie moze auto-logowac

Rekomendacja:
- gdy `RequireEmailConfirmed = false`:
  - zachowac obecne zachowanie, czyli auto-sign-in po rejestracji,
- gdy `RequireEmailConfirmed = true`:
  - nie logowac usera po rejestracji,
  - wyslac mail potwierdzajacy,
  - przekierowac na ekran typu `check your inbox`.

Powod:
- obecne `SignInAsync(...)` po rejestracji jest semantycznie sprzeczne z wlaczonym `RequireConfirmedEmail`,
- trzeba miec jeden spójny kontrakt.

### 4.6 `Forgot password` nie moze ujawniac, czy konto istnieje

Rekomendacja:
- `POST /auth/forgot-password` zawsze zwraca ten sam UX/result,
- niezaleznie od tego, czy email istnieje, czy jest potwierdzony.

Mail resetu wysylamy tylko gdy:
- user istnieje,
- i jego email jest juz potwierdzony.

Powod:
- to standardowe ograniczenie user enumeration,
- nie ma sensu resetowac hasla dla niepotwierdzonego konta.

## 5. Zakres kroku 10

Krok 10 obejmuje:
- rozszerzenie warstwy Auth o wysylke maili accountowych,
- flow potwierdzenia emaila po rejestracji,
- flow `forgot password`,
- flow `reset password`,
- odpowiednie UI Razor,
- aktualizacje zachowania register/login pod `RequireEmailConfirmed`,
- testy jednostkowe i E2E.

Krok 10 nie obejmuje:
- zmiany emaila zalogowanego usera,
- resend confirmation jako osobnego ekranu administracyjnego,
- rate limiting endpointow auth,
- MFA,
- audytu zdarzen bezpieczenstwa,
- integracji z prawdziwym providerem email w prod.

## 6. Plan implementacji technicznej

### Etap 10.1: Dodac minimalna infrastrukture maili Auth

Dodac kontrakt, np.:
- `IAuthEmailSender`

oraz proste modele:
- `EmailConfirmationMessage`
- `PasswordResetMessage`

Implementacja produkcyjna na tym etapie moze byc minimalna:
- `NoOp` albo `Logging` sender dla lokalnego developmentu,
- z wyraznym miejscem na przyszla integracje SMTP/provider API.

W E2E:
- dodac `TestAuthEmailSender` jako singleton,
- przechowujacy ostatnio wyslane maile w pamieci do odczytu przez testy.

Realne miejsca podpieta:
- rejestracja serwisu w `AuthModuleInitializator`,
- override w `Tests.EndToEnd/Setup/Modules/Common/OverrideServicesSetup.cs`.

### Etap 10.2: Dodac serwis do budowy linkow accountowych

Nie skladac linkow recznie w kontrolerze.

Dodac np.:
- `IAuthEmailLinkFactory`

Odpowiedzialnosc:
- wygenerowac link confirm email,
- wygenerowac link reset password,
- zakodowac token URL-safe,
- zbudowac URI na bazie publicznego originu.

### Etap 10.3: Rozszerzyc `IAuthService` o flow email confirmation

Dodac operacje typu:
- wygenerowanie maila potwierdzajacego po rejestracji,
- `ConfirmEmailAsync(userId, encodedToken)`,
- opcjonalnie `ResendConfirmationEmailAsync(email)`.

Najwazniejsza zmiana w `RegisterAsync(...)`:
- gdy `RequireEmailConfirmed = false`:
  - pozostawic obecny auto-login,
- gdy `RequireEmailConfirmed = true`:
  - utworzyc konto,
  - przypisac role,
  - opublikowac `UserRegisteredEvent`,
  - wygenerowac token confirm email,
  - wyslac mail,
  - nie logowac usera.

Wynik rejestracji musi odroznic przynajmniej:
- `SucceededAndSignedIn`,
- `SucceededPendingEmailConfirmation`,
- `DuplicateEmail`,
- `Failed`.

### Etap 10.4: Dodac endpoint i UI `confirm email`

Dodac endpoint:
- `GET /auth/confirm-email`

Parametry:
- `userId`,
- `token`

Zachowanie:
- odczyt usera,
- dekodowanie tokenu,
- `UserManager.ConfirmEmailAsync(...)`,
- widok sukcesu albo widok bledu.

Rekomendacja:
- nie robic POST-a, bo link z maila naturalnie prowadzi do GET confirm.

### Etap 10.5: Dodac ekran po rejestracji oczekujacej na confirm

Dodac prosty widok typu:
- `GET /auth/register-confirmation`

Po co:
- zeby user po rejestracji przy `RequireEmailConfirmed = true` nie ladowal na martwym koncu,
- zeby jasno wiedzial, ze musi sprawdzic skrzynke.

Widok moze zawierac:
- komunikat o wyslaniu linku,
- informacje, ze login bedzie mozliwy po confirm,
- opcjonalny link do ponownego wyslania w przyszlym kroku.

### Etap 10.6: Doprecyzowac zachowanie logowania dla niepotwierdzonego konta

Obecnie `PasswordSignInAsync(...)` przy wlaczonym `RequireConfirmedEmail` zwroci `IsNotAllowed`.

Trzeba jawnie zdecydowac UX:
- zamiast ogolnego `This account is not allowed to sign in.`,
- pokazac komunikat w stylu `Please confirm your email before signing in.`

To mozna zrobic:
- przez rozroznienie przyczyny w `AuthService.LoginAsync(...)`,
- albo przez dodatkowe sprawdzenie `IsEmailConfirmedAsync(user)` po wyniku `IsNotAllowed`.

To jest wazne, bo bez tego user dostanie techniczny, malo czytelny komunikat.

### Etap 10.7: Dodac flow `forgot password`

Dodac endpointy:
- `GET /auth/forgot-password`
- `POST /auth/forgot-password`
- `GET /auth/forgot-password-confirmation`

`POST` powinien:
- przyjac email,
- zawsze zakonczyc sie tym samym redirectem/komunikatem,
- dla istniejacego i potwierdzonego usera wygenerowac token resetu,
- wyslac mail z linkiem do `/auth/reset-password`.

### Etap 10.8: Dodac flow `reset password`

Dodac endpointy:
- `GET /auth/reset-password`
- `POST /auth/reset-password`

GET:
- przyjmuje `userId` i `token`,
- pre-populuje model widoku.

POST:
- dekoduje token,
- wywoluje `UserManager.ResetPasswordAsync(...)`,
- po sukcesie przekierowuje na ekran potwierdzenia albo login,
- po bledzie pokazuje walidacje.

Rekomendacja:
- po sukcesie nie logowac usera automatycznie,
- przekierowac na login z jasnym komunikatem.

### Etap 10.9: Dodac ViewModel-e i widoki Razor

Nowe modele widokow:
- `ForgotPasswordViewModel`
- `ResetPasswordViewModel`

Nowe widoki:
- `ForgotPassword`
- `ForgotPasswordConfirmation`
- `ResetPassword`
- `ResetPasswordConfirmation`
- `RegisterConfirmation`
- `ConfirmEmailResult`

Zakres UI ma byc celowo prosty:
- formularz,
- komunikat sukcesu,
- komunikat bledu,
- bez rozbudowanej nawigacji.

### Etap 10.10: Dopisac testy

#### Testy jednostkowe

Dodac lekkie testy orchestration, najlepiej z `NSubstitute`, dla:
- register flow z `RequireEmailConfirmed = true`,
- generowania i wysylki maila confirm,
- confirm email,
- forgot password bez wycieku istnienia usera,
- reset password happy path.

Jesli obecny `AuthService` okaże sie za ciezki do sensownych unit testow:
- wydzielic mniejsze abstrakcje zamiast budowac ciezki `ServiceProvider`.

#### Testy E2E

Dodac testy w `Tests.EndToEnd/E2E_Auth`, korzystajace z `HttpClient` i `TestAuthEmailSender`.

Minimalny zestaw:

1. `Register_WhenEmailConfirmationRequired_ShouldNotSignInAndShouldSendConfirmationEmail`
- rejestracja nie ustawia cookie sesyjnego dla normalnego flow,
- user trafia na ekran potwierdzenia,
- testowy sender przechwytuje link confirm.

2. `ConfirmEmail_WithValidToken_ShouldMarkUserAsConfirmed`
- wejscie w link z maila ustawia `EmailConfirmed = true`.

3. `Login_WhenEmailIsNotConfirmed_ShouldShowConfirmationMessage`
- przy wlaczonym `RequireEmailConfirmed` login nie przechodzi,
- UI pokazuje komunikat o koniecznosci potwierdzenia maila.

4. `ForgotPassword_ForExistingConfirmedUser_ShouldSendResetEmail`
- POST konczy sie generycznym potwierdzeniem,
- sender przechwytuje mail resetu.

5. `ForgotPassword_ForUnknownEmail_ShouldReturnSameConfirmationWithoutSendingSensitiveSignal`
- identyczny UX jak dla prawidlowego emaila,
- bez ujawnienia, czy konto istnieje.

6. `ResetPassword_WithValidToken_ShouldAllowLoginWithNewPassword`
- po wejsciu w link resetu i ustawieniu nowego hasla,
- login nowym haslem dziala.

7. `ResetPassword_WithInvalidToken_ShouldShowValidationError`
- flow nie konczy sie sukcesem dla zlego tokenu.

## 7. Wplyw na endpointy i flow Auth

Po kroku 10 dokumentacja Auth powinna zostac rozszerzona o:
- `GET /auth/confirm-email`
- `GET /auth/register-confirmation`
- `GET /auth/forgot-password`
- `POST /auth/forgot-password`
- `GET /auth/forgot-password-confirmation`
- `GET /auth/reset-password`
- `POST /auth/reset-password`

Zmiana flow rejestracji:
- przy `RequireEmailConfirmed = false` pozostaje auto-login,
- przy `RequireEmailConfirmed = true` rejestracja konczy sie ekranem `check inbox`.

Zmiana flow logowania:
- niepotwierdzony user nie dostaje tylko generycznego `NotAllowed`,
- dostaje zrozumialy komunikat i wskazowke co dalej.

## 8. Ryzyka i otwarte decyzje

### 8.1 Czy event `UserRegisteredEvent` ma byc publikowany przed confirm email

Obecnie rejestracja publikuje event od razu po utworzeniu konta.

To oznacza:
- Customers moze dostac usera jeszcze przed potwierdzeniem emaila.

Na ten krok rekomenduje:
- nie zmieniac tego zachowania,
- zostawic semantyke `konto zostalo utworzone`, a nie `konto zostalo aktywowane`.

Powod:
- zmiana semantyki eventu rozleje zakres na inne moduly.

Jesli kiedys bedzie potrzebne rozroznienie:
- dodac osobny event typu `UserEmailConfirmed`.

### 8.2 Czy wysylac reset password dla niepotwierdzonego konta

Rekomendacja:
- nie,
- reset hasla tylko dla potwierdzonych emaili.

Powod:
- upraszcza model bezpieczenstwa,
- unika wspierania kont, ktore nie przeszly jeszcze podstawowej weryfikacji adresu.

### 8.3 Czy potrzebny jest resend confirmation juz w kroku 10

Opcjonalnie przydatne, ale nie wymagane.

Rekomendacja:
- nie robic osobnego endpointu resend w kroku 10,
- chyba ze implementacja confirm email ujawni, ze bez tego UX jest nieakceptowalny.

### 8.4 Jak traktowac juz zalogowana sesje po confirm email

W podstawowej wersji:
- confirm email tylko oznacza konto jako potwierdzone,
- nie tworzy automatycznie sesji.

To jest prostsze i mniej ryzykowne niz auto-sign-in po kliknieciu linku z maila.

## 9. Definition of Done dla kroku 10

Krok 10 uznajemy za zamkniety, gdy:
- istnieje minimalna infrastruktura wysylki maili auth,
- rejestracja przy `RequireEmailConfirmed = true` wysyla link confirm i nie loguje usera,
- istnieje endpoint i widok potwierdzenia emaila,
- login niepotwierdzonego usera pokazuje jasny komunikat,
- istnieje flow `forgot password`,
- istnieje flow `reset password`,
- linki confirm/reset sa generowane jako poprawne URI z bezpiecznie zakodowanym tokenem,
- istnieje testowy mail sink dla E2E,
- testy E2E pokrywaja register confirm i reset password,
- wlaczenie `Auth:RequireEmailConfirmed = true` nie powoduje dead-endu w UI.

## 10. Rekomendowana kolejnosc implementacji

1. Dodac kontrakt wysylki maili i testowy mail sink.
2. Dodac factory do budowy linkow confirm/reset z URL-safe tokenem.
3. Rozszerzyc `AuthService.RegisterAsync(...)` o wariant `pending confirmation`.
4. Dodac `GET /auth/confirm-email` i ekran `register-confirmation`.
5. Doprecyzowac komunikat loginu dla niepotwierdzonego konta.
6. Dodac `forgot password` flow.
7. Dodac `reset password` flow.
8. Dodac E2E dla confirm email.
9. Dodac E2E dla forgot/reset password.
10. Na koniec sprawdzic pelny scenariusz z `RequireEmailConfirmed = true`.

## 11. Co proponowalbym jako krok 11

Po kroku 10 najbardziej logiczny kolejny etap to:
- reliability/security hardening Auth,

czyli:
- outbox dla eventow wychodzacych z Auth,
- rate limiting dla endpointow auth i token endpointu,
- ewentualnie osobna decyzja o MFA.

MFA nadal nie wciskalbym do kroku 10.
To juz osobny temat architektoniczny i UX-owy.
