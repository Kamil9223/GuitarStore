# Auth Implementation 7

Data: 2026-04-05
Status: Draft

## 0. Cel kroku 7

Krok 7 realizuje sekcje `Seed admin` z `.ai/Auth.md`.

Po tym kroku system ma miec:
- automatyczny seed konta admina w dev/local na podstawie `SeedAdmin` z konfiguracji,
- bezpieczny bootstrap admina w prod uruchamiany tylko raz,
- jawne rozroznienie scenariusza developerskiego od produkcyjnego,
- testy potwierdzajace, ze seed jest idempotentny i nie tworzy duplikatow.

Krok 7 nie powinien jeszcze domykac:
- email confirmation,
- MFA,
- outbox w Auth,
- pelnych testow auth code flow i refresh rotation z kroku 8.

## 1. Wejscie do kroku 7

Zakladany stan po kroku 6:
- Auth ma Identity + OpenIddict,
- UI login/register/logout dziala,
- role `user`, `support`, `admin` sa seedowane,
- rejestracja publikuje `Auth.Core.Events.Outgoing.UserRegisteredEvent`,
- Customers tworzy `Customer` i `Cart`,
- logika account flow jest juz wyniesiona z kontrolera do `Auth.Core.Services.IAuthService`.

Aktualny stan repo istotny dla kroku 7:
- istnieje `SeedAdminOptions` w `Auth.Core/Configuration/SeedAdminOptions.cs`,
- istnieje sekcja `SeedAdmin` w `GuitarStore.ApiGateway/appsettings.Auth.json`,
- `InitializeAuthModuleAsync(...)` wykonuje startup init dla Auth,
- testy E2E ustawiaja `SeedAdmin:Enabled = false`,
- `Auth.Core.Entities.User` nie ma jeszcze pola typu `MustChangePassword`,
- nie ma jeszcze zadnego `AdminInitializer` ani mechanizmu bootstrap-once.

Wniosek:
- konfiguracja pod seed juz istnieje,
- ale brak jeszcze warstwy wykonawczej i zasad bezpieczenstwa dla produkcji.

## 2. Zakres

Krok 7 obejmuje:
- initializer/seeder admina po stronie `Auth.Core`,
- dev/local seed admina z `SeedAdmin:Enabled`,
- produkcyjny bootstrap admina odpalany tylko raz,
- walidacje konfiguracji `SeedAdmin`,
- testy integracyjne/idempotency dla seeda i bootstrapu.

Krok 7 nie obejmuje:
- UI do bootstrapu admina,
- flow zmiany hasla po pierwszym logowaniu,
- resetu hasla admina przez panel,
- zarzadzania wieloma adminami jako osobnego scenariusza biznesowego.

## 3. Kluczowe decyzje

### 3.1 Dev/local i prod musza byc rozdzielone

Rekomendacja:
- `SeedAdmin.Enabled = true` ma dzialac tylko w dev/local,
- w prod standardowy seed z haslem z pliku konfiguracyjnego ma byc zablokowany,
- produkcja uzywa osobnego bootstrap-once.

Powod:
- haslo admina w appsettings jest akceptowalne lokalnie,
- ale nie powinno byc docelowym mechanizmem provisioning w prod.

### 3.2 Bootstrap prod tylko raz

Rekomendacja:
- bootstrap jest odpalany tylko wtedy, gdy:
  - aplikacja pracuje poza dev/local,
  - `ADMIN_BOOTSTRAP_SECRET` jest ustawiony,
  - nie istnieje jeszcze zaden admin.

Po skutecznym utworzeniu pierwszego admina:
- kolejne starty aplikacji nie powinny tworzyc nastepnych adminow z bootstrapu,
- nawet jesli env var nadal jest ustawiony.

Najprostsza regula jednokrotnosci na MVP:
- `if any user in role admin exists => no-op`.

To wystarcza, jesli celem jest:
- zapewnienie pierwszego admina,
- a nie rozbudowany provisioning tozsamosci uprzywilejowanych.

### 3.3 Haslo jednorazowe vs haslo docelowe

`.ai/Auth.md` sugeruje:
- prod bootstrap tworzy admina z haslem jednorazowym,
- i wymusza zmiane hasla przy pierwszym logowaniu.

Aktualny model repo nie ma jeszcze pola ani flow do `MustChangePassword`.

Dlatego sa dwa warianty:

Wariant A: MVP w kroku 7
- bootstrap tworzy admina z haslem z env/config,
- bez wymuszenia zmiany hasla,
- a requirement `must change password` zostaje zapisany jako follow-up.

Wariant B: pelniejszy bootstrap
- rozszerzyc `User` o flage typu `MustChangePassword`,
- dodac migracje Auth,
- zablokowac zwykly flow po pierwszym logowaniu i wymusic zmiane hasla.

Rekomendacja dla kroku 7:
- wdrozyc Wariant A jako MVP,
- jawnie zapisac brak wymuszenia zmiany hasla jako debt do kolejnego kroku hardening.

Powod:
- glowny plan kroku 7 koncentruje sie na seed/bootstrap,
- a nie na pelnym password lifecycle management.

## 4. Docelowy model konfiguracji

### 4.1 Dev/local

Istniejaca sekcja:

```json
"SeedAdmin": {
  "Enabled": true,
  "Email": "admin@guitarstore.local",
  "Password": "ChangeMe!123"
}
```

Dla dev/local to pozostaje poprawne.

Zasady:
- `Enabled = true` tworzy admina przy starcie, jesli nie istnieje,
- role musza byc juz zainicjalizowane przed seedem admina,
- seed powinien przypisywac role `admin` oraz opcjonalnie `user` tylko jesli to jest zgodne z aktualnym modelem roli bazowej.

Rekomendacja:
- przypisac tylko role `admin`,
- nie dokladac `user`, jesli nie jest to wymagane przez logike systemu.

Powod:
- `admin` i tak niesie pelny zestaw elevated permissions,
- nie ma potrzeby mieszac semantyki roli bazowej i uprzywilejowanej bez wyraznej potrzeby.

### 4.2 Prod

Bootstrap powinien korzystac z:
- `ADMIN_BOOTSTRAP_SECRET` jako warunku wlaczenia,
- opcjonalnie:
  - `ADMIN_BOOTSTRAP_EMAIL`
  - `ADMIN_BOOTSTRAP_PASSWORD`

Jesli nie chcemy mnozyc env vars, mozna przyjac prostszy MVP:
- `ADMIN_BOOTSTRAP_SECRET` tylko odblokowuje bootstrap,
- email/haslo dalej sa czytane z `SeedAdmin`,
- ale `SeedAdmin.Enabled` musi byc `false` w prod.

Rekomendacja:
- na MVP trzymac:
  - `SeedAdmin.Enabled = false` w prod,
  - `SeedAdmin.Email`
  - `SeedAdmin.Password`
  - `ADMIN_BOOTSTRAP_SECRET` jako bezpiecznik uruchomienia.

Powod:
- najmniejszy zakres zmian,
- bez przebudowy calego modelu konfiguracji.

## 5. Plan implementacji technicznej

### Etap 7.1: Dodac initializer admina w `Auth.Core`

Dodac nowy komponent, np.:
- `Auth.Core/Configuration/AdminInitializer.cs`

Odpowiedzialnosc:
- odczyt konfiguracji `SeedAdminOptions`,
- odczyt environment gate dla prod bootstrap,
- sprawdzenie istnienia admina,
- utworzenie usera,
- przypisanie roli `admin`,
- logowanie decyzji startupowych.

Powinien korzystac z:
- `UserManager<User>`
- `RoleManager<Role>` tylko jesli potrzebna jest dodatkowa walidacja istnienia roli,
- `IHostEnvironment` albo innego sygnalu srodowiska,
- `IConfiguration` dla env var / gate,
- loggera.

### Etap 7.2: Ustalic kolejnosc init

`InitializeAuthModuleAsync(...)` powinno wykonywac:
1. seed rol i permission claims,
2. seed OpenIddict applications,
3. seed/bootstrap admina.

Powod:
- admin nie moze byc przypisany do roli `admin`, jesli role nie istnieja,
- aplikacje OIDC sa niezalezne od admina, ale logicznie nadal naleza do startup init auth.

### Etap 7.3: Dev/local seed

Scenariusz:
- jesli `SeedAdmin.Enabled == true`,
- i user o skonfigurowanym emailu nie istnieje,
- utworzyc usera i przypisac role `admin`.

Jesli user istnieje:
- nie tworzyc duplikatu,
- opcjonalnie dopilnowac, ze ma role `admin`.

Rekomendacja:
- initializer powinien byc idempotentny i synchronizowac minimalny oczekiwany stan:
  - user istnieje,
  - user jest w roli `admin`.

Nie rekomenduje na tym etapie:
- automatycznego resetowania hasla istniejacego admina przy kazdym starcie,
- bo to byloby zbyt agresywne i niebezpieczne.

### Etap 7.4: Prod bootstrap once

Scenariusz:
- jesli aplikacja nie jest w dev/local,
- i `SeedAdmin.Enabled == false`,
- i `ADMIN_BOOTSTRAP_SECRET` jest ustawiony,
- i nie istnieje zaden user z rola `admin`,
- wtedy utworzyc pierwszego admina.

Jesli admin juz istnieje:
- no-op.

Jesli gate env var nie jest ustawiony:
- no-op.

Jesli konfiguracja email/hasla jest pusta:
- fail startup z jasnym komunikatem.

### Etap 7.5: Walidacja konfiguracji

Dodac jawne reguly walidacji:
- jesli `SeedAdmin.Enabled == true`, to `Email` i `Password` musza byc ustawione,
- haslo musi spelniac aktualna password policy Identity,
- w prod nie wolno miec jednoczesnie:
  - `SeedAdmin.Enabled == true`
  - i aktywnego bootstrap gate.

Rekomendacja:
- fail-fast przy niespojnej konfiguracji.

### Etap 7.6: Logging i observability

Initializer powinien logowac:
- czy dev seed byl wlaczony,
- czy admin juz istnial,
- czy bootstrap prod zostal wykonany,
- czy bootstrap zostal pominiety z powodu braku gate env var.

Nie logowac:
- hasla,
- sekretu bootstrap,
- pelnych danych wrazliwych.

## 6. Proponowany kontrakt zachowania

### 6.1 Dev/local

Przy starcie:
- jesli brak admina o skonfigurowanym emailu -> tworz admina,
- jesli admin istnieje -> nie tworz nowego,
- jesli user istnieje bez roli `admin` -> dopnij role `admin`.

### 6.2 Prod

Przy starcie:
- jesli brak `ADMIN_BOOTSTRAP_SECRET` -> nie rob nic,
- jesli istnieje juz jakikolwiek admin -> nie rob nic,
- jesli warunki bootstrapu sa spelnione -> utworz jednego admina,
- po utworzeniu kolejny start ma byc no-op.

## 7. Testy

Minimalny zestaw testow dla kroku 7:

### 7.1 Dev seed creates admin
- przy `SeedAdmin.Enabled = true`,
- brak admina przed startem,
- po init istnieje user z emailem z configu,
- user ma role `admin`.

### 7.2 Dev seed is idempotent
- dwukrotne uruchomienie init nie tworzy duplikatu usera,
- liczba adminow nie rosnie,
- rola `admin` pozostaje przypisana.

### 7.3 Prod bootstrap requires gate
- przy `SeedAdmin.Enabled = false`,
- bez `ADMIN_BOOTSTRAP_SECRET`,
- bootstrap nic nie robi.

### 7.4 Prod bootstrap creates first admin only once
- przy ustawionym gate,
- i braku admina,
- pierwszy init tworzy admina,
- drugi init nic juz nie robi.

### 7.5 Invalid config fails fast
- `SeedAdmin.Enabled = true` bez emaila lub hasla -> startup/init fail,
- analogicznie dla bootstrapu, jesli wybrany wariant wymaga skonfigurowanych danych.

## 8. Ryzyka i otwarte decyzje

### 8.1 Brak `MustChangePassword`

To najwazniejsza luka wzgledem pierwotnej ambicji planu.

Aktualnie repo nie ma:
- flagi na userze,
- flow UI do wymuszenia zmiany hasla,
- dodatkowej walidacji po logowaniu.

Rekomendacja:
- zapisac to jako follow-up po kroku 7,
- nie mieszac tego z samym bootstrapem admina, jesli celem jest szybkie domkniecie provisioning MVP.

### 8.2 Jeden admin vs wielu adminow

Bootstrap-once daje:
- pierwszego admina.

Nie rozwiazuje:
- dalszego provisioning kolejnych adminow.

To jest akceptowalne na tym etapie, jesli:
- dalsze nadawanie roli `admin` bedzie robione recznie lub osobnym narzedziem operacyjnym.

### 8.3 Email istniejacy bez roli admin

Mozliwy scenariusz:
- user o skonfigurowanym emailu juz istnieje,
- ale nie ma roli `admin`.

Rekomendacja:
- w dev/local dopiac role `admin`,
- w prod bootstrap rozpatrywac ostrozniej:
  - jesli istnieje user o bootstrap emailu, dopiac role `admin` tylko wtedy, gdy to jest jawnie akceptowany scenariusz,
  - w przeciwnym razie fail-fast z jasnym logiem operacyjnym.

Najbezpieczniejsze MVP:
- dev/local: dopinaj role,
- prod: fail-fast przy konflikcie emaila.

## 9. Definition of Done

Krok 7 uznajemy za zamkniety, gdy:
- Auth seeduje admina w dev/local na podstawie `SeedAdmin`,
- seed jest idempotentny,
- produkcyjny bootstrap tworzy pierwszego admina tylko raz,
- bootstrap jest chroniony env gate,
- niespojna konfiguracja failuje jawnie,
- istnieja testy pokrywajace dev seed i prod bootstrap,
- `AccountController` i pozostale flow Auth nie musza byc zmieniane, aby seed dzialal.

## 10. Rekomendowana kolejnosc implementacji

1. Dodac `AdminInitializer` w `Auth.Core`.
2. Dopiacz go do `InitializeAuthModuleAsync(...)` po seedzie rol.
3. Zaimplementowac dev/local seed z `SeedAdminOptions`.
4. Zaimplementowac prod bootstrap gate oparty o `ADMIN_BOOTSTRAP_SECRET`.
5. Dodac fail-fast walidacje konfiguracji.
6. Dodac testy idempotencji i bootstrap-once.
7. Zapisac follow-up dla `MustChangePassword` i first-login password change.
