# Auth Implementation 9

Data: 2026-04-13
Status: Draft

## 0. Czy krok 9 jest w ogole potrzebny

Tak, ale nie jako warunek MVP.

Po kroku 8 Auth jest funkcjonalnie domkniety na poziomie MVP:
- Identity + OpenIddict dzialaja,
- account UI dziala,
- auth code + PKCE + refresh token rotation sa przetestowane,
- role i policies dzialaja,
- Customers integration dziala,
- seed admin i bootstrap admina dzialaja.

Czyli:
- dla lokalnego developmentu i podstawowego rolloutu krok 8 moze byc uznany za sensowny punkt stop,
- dla produkcyjnego hardeningu Auth nadal zostaje kilka otwartych tematow.

Najbardziej oczywisty debt po kroku 8:
- bootstrap admina w prod nadal nie wymusza zmiany hasla przy pierwszym logowaniu.

To bylo jawnie odnotowane w kroku 7 jako swiadomy kompromis MVP.

## 1. Rekomendacja: czym powinien byc krok 9

Krok 9 powinien byc krokiem `password lifecycle hardening`, a nie kolejnym krokiem infrastrukturalnym.

Zakres rekomendowany:
- `MustChangePassword` dla bootstrap admina,
- wymuszenie zmiany hasla przy pierwszym logowaniu,
- UI i backend do zmiany hasla przed dopuszczeniem do zwyklego flow,
- testy integracyjne dla first-login password change.

Powod:
- to jest najblizszy, juz rozpoznany debt po kroku 7,
- jest bezposrednio zwiazany z bezpieczeństwem uprzywilejowanego konta,
- ma wiekszy priorytet niz MFA czy outbox,
- nie miesza kilku duzych tematow naraz.

## 2. Cel kroku 9

Po tym kroku system ma miec:
- mozliwosc oznaczenia usera, ze musi zmienic haslo,
- bootstrap admina w prod tworzacego konto z flaga `MustChangePassword = true`,
- blokade zwyklego login/authorize flow dla usera z taka flaga,
- dedykowany flow zmiany hasla po pierwszym logowaniu,
- zdjecie flagi po udanej zmianie hasla,
- testy potwierdzajace caly scenariusz.

Krok 9 nie powinien jeszcze obejmowac:
- MFA,
- email confirmation,
- password reset by email,
- outbox w Auth,
- rate limiting.

## 3. Wejscie do kroku 9

Stan po kroku 8:
- `AdminInitializer` wspiera dev seed i prod bootstrap,
- prod bootstrap tworzy admina na podstawie `SeedAdmin.Email` i `SeedAdmin.Password`,
- nie ma jeszcze pola typu `MustChangePassword`,
- login i authorize flow wpuszczaja usera bez dodatkowej kontroli pierwszego logowania.

Istotna obserwacja:
- `Auth.Core.Entities.User` nie ma jeszcze flagi, ktora rozroznia konto tymczasowe od zwyklego,
- `AccountController` i `OpenIddictController.Authorize()` nie maja jeszcze logiki wymuszajacej interstitial typu "zmien haslo zanim pojdziesz dalej".

## 4. Kluczowe decyzje

### 4.1 Flaga powinna byc na `User`

Rekomendacja:
- dodac do `Auth.Core.Entities.User` pole typu:
  - `bool MustChangePassword`

Powod:
- to jest prosty, jawny stan biznesowy usera,
- nie trzeba wnioskowac go z historii logowan,
- da sie go latwo ustawic w bootstrapie i wyczyscic po sukcesie.

### 4.2 Wymuszenie ma dzialac po uwierzytelnieniu, ale przed normalnym dostepem

Rekomendacja:
- poprawne haslo moze tworzyc sesje,
- ale user z `MustChangePassword = true` nie powinien byc wpuszczony do:
  - standardowego `returnUrl`,
  - `/connect/authorize`,
  - zwyklego obszaru aplikacji.

Zamiast tego:
- powinien byc przekierowany na ekran zmiany hasla.

Powod:
- to najprostszy i najbardziej czytelny flow dla operatora/admina.

### 4.3 Flaga ma byc ustawiana tylko dla prod bootstrap admina

Rekomendacja:
- dev/local seed admina nadal tworzy zwyklego admina bez `MustChangePassword`,
- prod bootstrap tworzy tymczasowego admina z `MustChangePassword = true`.

Powod:
- dev/local ma byc wygodne,
- prod bootstrap ma byc bezpieczniejszy.

## 5. Zakres implementacji

Krok 9 obejmuje:
- rozszerzenie encji `User`,
- migracje Auth,
- aktualizacje `AdminInitializer`,
- zmiany w login/authorize flow,
- dedykowany ekran i POST do zmiany hasla,
- testy integracyjne / E2E nowego flow.

Krok 9 nie obejmuje:
- zapomnialem hasla / reset link,
- wymogu potwierdzenia email,
- MFA challenge,
- polityk opartych o risk signals,
- rotacji sekretow bootstrap.

## 6. Plan implementacji technicznej

### Etap 9.1: Rozszerzyc `User`

Dodac do `Auth.Core.Entities.User`:
- `bool MustChangePassword { get; private set; }`

Dodatkowo warto dodac male metody domenowo-techniczne:
- `RequirePasswordChange()`
- `MarkPasswordChanged()`

Jesli obecny model encji jest anemiczny, dopuszczalna jest tez prostsza wersja:
- public/internal setter + ostrozne uzycie po stronie serwisow.

### Etap 9.2: Dodac migracje Auth

Dodac migracje rozszerzajaca tabele `Auth.Users` o kolumne:
- `MustChangePassword bit not null default 0`

### Etap 9.3: Zmienic `AdminInitializer`

Scenariusz docelowy:
- dev/local seed:
  - tworzy admina jak dotychczas,
  - `MustChangePassword = false`
- prod bootstrap:
  - tworzy pierwszego admina,
  - ustawia `MustChangePassword = true`

Rekomendacja:
- nie zmieniac istniejacego zachowania dla juz istniejacego admina w dev/local,
- dla prod bootstrap nowy user ma byc jawnie oznaczony jako tymczasowy.

### Etap 9.4: Dodac serwis do zmiany hasla przy pierwszym logowaniu

Najlepiej nie wpychac calej logiki do kontrolera.

Dodac np.:
- `IForcedPasswordChangeService`
- albo rozszerzyc obecny `IAuthService`

Odpowiedzialnosc:
- zweryfikowac stare haslo,
- ustawic nowe haslo przez `UserManager`,
- wyczyscic `MustChangePassword`,
- odswiezyc cookie/sign-in state.

### Etap 9.5: Dodac UI i endpointy

Dodac widok i endpointy, np.:
- `GET /auth/change-password-required`
- `POST /auth/change-password-required`

Widok powinien:
- przyjac stare haslo,
- przyjac nowe haslo + potwierdzenie,
- pokazac walidacje zgodne z password policy.

### Etap 9.6: Zablokowac zwykly flow dla usera z flaga

Punkty kontrolne:
- po poprawnym loginie w `AccountController`,
- w `OpenIddictController.Authorize()`,
- opcjonalnie przez middleware lub dedykowany filtr dla UI auth.

Minimalna bezpieczna wersja:
- po loginie, jesli user ma `MustChangePassword`, zawsze redirect na ekran zmiany hasla,
- w `Authorize()` dodatkowa blokada, jesli cookie usera nadal ma taka flage.

Powod:
- sam redirect po loginie to za malo, bo user moze juz miec aktywna cookie session.

### Etap 9.7: Testy

Minimalny zestaw testow:

1. `ProdBootstrap_ShouldMarkAdminAsMustChangePassword`
- bootstrap prod tworzy admina z flaga.

2. `Login_WhenUserMustChangePassword_ShouldRedirectToForcedChangeScreen`
- poprawny login nie wraca do zwyklego `ReturnUrl`,
- tylko prowadzi do zmiany hasla.

3. `Authorize_WhenUserMustChangePassword_ShouldNotIssueAuthorizationCode`
- authorize flow nie wydaje `code`,
- tylko przekierowuje do flow zmiany hasla.

4. `ForcedPasswordChange_WhenNewPasswordIsValid_ShouldClearFlag`
- po udanej zmianie hasla flaga znika.

5. `ForcedPasswordChange_AfterSuccess_ShouldAllowNormalAuthorizeFlow`
- po zmianie hasla user moze przejsc standardowy auth code flow.

## 7. Ryzyka i decyzje otwarte

### 7.1 Gdzie trzymac redirect po zmianie hasla

Trzeba zdecydowac, czy po wymuszonej zmianie hasla:
- wracamy do pierwotnego `ReturnUrl`,
- czy po prostu przekierowujemy na `/`.

Rekomendacja:
- jesli technicznie proste, zachowac `ReturnUrl`,
- jesli ma to skomplikowac krok 9, na MVP przekierowac na `/`.

### 7.2 Cookie claims vs odczyt z bazy

Nie nalezy polegac tylko na claimie w cookie.

Rekomendacja:
- dla krytycznej decyzji `MustChangePassword` sprawdzac aktualny stan usera z bazy,
- bo flaga moze zmienic sie po issuance cookie.

### 7.3 Czy obejmowac zwyklych userow czy tylko bootstrap admina

Na start wystarczy:
- infrastruktura ogolna,
- ale realne ustawianie flagi tylko dla prod bootstrap admina.

To daje przyszla rozszerzalnosc bez rozlewania zakresu kroku 9.

## 8. Czy po kroku 9 Auth bedzie juz "gotowy"

Po kroku 9 Auth bedzie sensownie zahardeningowany dla pierwszego konta admina, ale nadal zostana tematy opcjonalne / kolejnych iteracji:
- email confirmation,
- MFA,
- outbox w Auth,
- rate limiting na auth endpoints.

Czyli:
- po kroku 9 mozna uznac Auth za mocny MVP+,
- ale nie za finalny security-complete auth stack.

## 9. Ile jeszcze krokow przewidujemy

Rekomendacja planistyczna:
- minimum 2 dodatkowe kroki po kroku 8,
- realistycznie 3 dodatkowe kroki, jesli chcemy domknac Auth sensownie produkcyjnie.

Proponowana mapa:

### Krok 9
- `MustChangePassword` + first-login password change dla bootstrap admina.

### Krok 10
- email confirmation + ewentualnie forgot/reset password flow.

### Krok 11
- reliability/security hardening:
  - outbox w Auth dla `UserRegisteredEvent`,
  - rate limiting dla `/connect/token` i krytycznych endpointow auth,
  - ewentualnie decyzja o MFA jako osobny pod-etap albo krok 12.

MFA:
- nie wciskalbym go do kroku 9,
- nie wciskalbym go tez na sile do kroku 10, jesli email confirmation i password recovery beda duze,
- praktycznie MFA moze byc:
  - czescia kroku 11,
  - albo osobnym krokiem 12, jesli chcemy to zrobic porzadnie.

## 10. Definition of Done dla kroku 9

Krok 9 uznajemy za zamkniety, gdy:
- `User` ma stan `MustChangePassword`,
- prod bootstrap admina ustawia ten stan,
- user z taka flaga nie przechodzi normalnie przez login/authorize flow,
- istnieje wymuszony ekran zmiany hasla,
- udana zmiana hasla czyści flage,
- po sukcesie user moze przejsc normalny auth flow,
- testy integracyjne/E2E pokrywaja ten scenariusz.
