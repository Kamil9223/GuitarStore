# Auth Implementation 5

Data: 2026-03-29
Status: Draft

## 0. Cel kroku 5

Krok 5 realizuje sekcje `Roles i policies` z `.ai/Auth.md`.

Po tym kroku system ma miec:
- seed podstawowych rol `user`, `support`, `admin`,
- jawny model permission claims,
- policy-based authorization zarejestrowane w DI,
- pierwsze miejsca w API gotowe do uzycia `[Authorize(Policy = ...)]`.

Krok 5 nie powinien jeszcze domykac:
- Customers integration,
- seed admin z konfiguracji dev/prod bootstrap,
- pelnych testow auth code flow z realnym token exchange,
- wymuszenia przypisywania roli `user` w kazdym scenariuszu biznesowym poza tym, co jest potrzebne dla MVP auth.

## 1. Wejscie do kroku 5

Zakladany stan po kroku 4:
- Identity i OpenIddict sa skonfigurowane,
- UI login/register/logout dziala,
- klient SPA jest rejestrowany przy starcie,
- `OidcClaimsPrincipalFactory` umie juz przenosic claim `role` do tokenu, jesli principal go posiada,
- nie ma jeszcze seeda rol i nie ma formalnego rejestru policies.

To oznacza, ze fundament techniczny pod role juz istnieje, ale system nie ma jeszcze:
- zdefiniowanych permission names,
- mapowania role -> permissions,
- `AddAuthorization(...)` z konkretnymi policies,
- testow potwierdzajacych kontrakt authorization.

## 2. Zakres

Krok 5 obejmuje:
- wprowadzenie stalego modelu permission names,
- seed rol `user`, `support`, `admin`,
- seed claimow-permission przypisanych do rol,
- konfiguracje policies w Auth/API,
- minimalne zastosowanie policies tam, gdzie juz istnieja odpowiednie endpointy,
- testy potwierdzajace seed i rejestracje policies.

Krok 5 nie obejmuje:
- seedowania admin usera,
- resource-based authorization dla konkretnych encji zamowien,
- kompletnego rolloutu atrybutow `[Authorize]` po wszystkich modulach,
- przebudowy calego API pod nowe permissions w jednym kroku.

## 3. Docelowy model uprawnien

Zgodnie z `.ai/Auth.md` role pozostaja:
- `user`
- `support`
- `admin`

Proponowane permission claims:
- `Catalog.Manage`
- `Orders.ViewAny`
- `Orders.CancelAny`
- `Customers.ViewAny`

Rekomendowane mapowanie:
- `user`
  - brak elevated permissions
- `support`
  - `Orders.ViewAny`
  - `Orders.CancelAny`
  - `Customers.ViewAny`
- `admin`
  - `Catalog.Manage`
  - `Orders.ViewAny`
  - `Orders.CancelAny`
  - `Customers.ViewAny`

Wazne doprecyzowanie:
- rola jest prostym sposobem grupowania uprawnien,
- policy powinny opierac sie na permission claims, nie na samych nazwach rol,
- dzieki temu przyszle zmiany mapowania nie beda wymuszaly zmian w kontrolerach.

## 4. Decyzje implementacyjne

### 4.1 Permission jako claim

Rekomendacja:
- uzyc osobnego typu claimu, np. `permission`,
- nie kodowac permissions jako pseudo-rol.

Powod:
- role opisuja grupy uzytkownikow,
- permissions opisuja konkretne zdolnosci,
- policy-based authorization lepiej sklada sie z claims niz z rozbudowanej logiki `RequireRole(...)`.

### 4.2 Miejsce definicji stalych

Dodac jawne stale:
- nazwy rol,
- nazwy permissions,
- opcjonalnie nazwy policies.

Przyklad struktury:
- `Auth.Core/Authorization/AuthRoles.cs`
- `Auth.Core/Authorization/AuthPermissions.cs`
- `Auth.Core/Authorization/AuthPolicies.cs`

Cel:
- uniknac stringly-typed authorization w wielu projektach,
- miec jedno zrodlo prawdy dla nazw.

### 4.3 Seed rol i claimow

Seed powinien:
- tworzyc role, jesli nie istnieja,
- aktualizowac claims roli do oczekiwanego stanu,
- byc idempotentny.

To oznacza:
- brak duplikowania claimow,
- mozliwosc bezpiecznego uruchamiania przy kazdym starcie,
- jawne usuwanie nieaktualnych permission claims z roli, jesli mapa w kodzie sie zmieni.

### 4.4 Miejsce uruchamiania seeda

Krok 4 pokazal juz, ze startup tasks w Auth musza byc odporne na design-time tooling.

Dlatego w kroku 5 seed rol powinien:
- korzystac z tego samego modelu startup/init co pozostale krytyczne init tasks,
- nie byc "best effort",
- failowac startup, jesli konfiguracja auth jest niespojna,
- byc pomijany w scenariuszach build tooling, jesli host jest uruchamiany tylko dla OpenAPI generation.

### 4.5 Rejestracja policies

`AddAuthorization()` trzeba rozszerzyc o jawne policies:
- `Catalog.Manage`
- `Orders.ViewAny`
- `Orders.CancelAny`
- `Customers.ViewAny`

Kazda policy powinna:
- wymagac authenticated usera,
- wymagac odpowiedniego `permission` claim.

Nie rekomenduje sie na tym etapie:
- mieszania jednej policy z rolami i claimami jednoczesnie,
- ukrytego mapowania w wielu miejscach DI.

## 5. Plan implementacji technicznej

### Etap 5.1: Wprowadzenie kontraktu authorization

Dodac stale i pomocnicze typy dla:
- rol,
- permissions,
- policies,
- typu claimu `permission`.

Opcjonalnie:
- dodac helper budujacy mape `role -> permissions`, zeby seed i policy config korzystaly z tych samych danych.

### Etap 5.2: Seed rol Identity

Dodac initializer/seeder dla rol, oparty o `RoleManager<Role>`.

Zakres:
- `user`
- `support`
- `admin`

Seeder powinien:
- sprawdzic istnienie roli,
- utworzyc role brakujace,
- nie zakladac, ze seed admin usera juz istnieje.

### Etap 5.3: Seed permission claims dla rol

Rozszerzyc seeder o claims przypisane do rol.

Zakres:
- odczyt aktualnych claims roli,
- porownanie z oczekiwanym zestawem,
- dodanie brakujacych permissions,
- usuniecie permissions, ktore nie sa juz czescia mapowania.

To jest istotne, bo bez synchronizacji:
- rola moze zachowac stare permission claims po zmianie kodu,
- testy i runtime moga sie rozjechac.

### Etap 5.4: Rejestracja policies w DI

Rozszerzyc `ConfigureAuthorization(...)` w `AuthModuleInitializator`.

Polityki powinny byc zdefiniowane jawnie i centralnie, np.:
- `options.AddPolicy(AuthPolicies.CatalogManage, policy => ...)`
- `options.AddPolicy(AuthPolicies.OrdersViewAny, policy => ...)`
- analogicznie dla pozostalych.

### Etap 5.5: Spiecie z principal i tokenami

Zweryfikowac, czy principal budowany przez Identity i `OidcClaimsPrincipalFactory` dostaje:
- role claims,
- ewentualnie permission claims, jesli maja trafiaac do tokenu.

Decyzja do podjecia przed implementacja:
- czy permission claims maja trafic tylko do `HttpContext.User` po cookie/Identity,
- czy rowniez do access tokena dla klienta SPA.

Rekomendacja MVP:
- jesli API ma opierac sie na policies wymagajacych permission claims, to permission claims musza byc obecne w bearer principal,
- to w praktyce oznacza, ze principal OIDC powinien zawierac permissions wynikajace z roli usera.

Wniosek:
- sam seed claims na roli nie wystarczy,
- trzeba jawnie sprawdzic, czy `UserClaimsPrincipalFactory` / Identity doklada role claims,
- i czy trzeba dopisac permission claims do principal przy issuance tokena.

### Etap 5.6: Minimalne uzycie policies w API

Na tym kroku warto wdrozyc co najmniej 1-2 realne punkty uzycia, zeby policy nie pozostaly "martwe".

Rekomendacja:
- wybrac endpoint administracyjny katalogu dla `Catalog.Manage`,
- wybrac endpoint wsparcia lub podgladu zamowien dla `Orders.ViewAny` albo `Customers.ViewAny`.

Jesli pelny rollout po wszystkich modulach jest za duzy na ten krok:
- wdrozyc reprezentatywne miejsca,
- reszte odnotowac jako follow-up.

### Etap 5.7: Domyslna rola `user`

Przy okazji kroku 5 trzeba podjac decyzje, czy:
- nowo rejestrowany user dostaje od razu role `user`,
- czy rola `user` jest tylko logiczna i nie musi byc przypisywana explicite.

Rekomendacja:
- przypisywac role `user` przy rejestracji.

Powod:
- upraszcza audyt i przyszle rozszerzenia,
- ujednolica model danych Identity,
- usuwa niejednoznacznosc "czy zwykly user ma role czy tylko brak roli".

Jesli ta decyzja wejdzie do implementacji kroku 5, trzeba dopisac to do `POST /auth/register`.

## 6. Wplyw na tokeny i authorize flow

Krok 5 dotyka nie tylko `[Authorize]`, ale tez zawartosci principal tokenowego.

Po kroku 5 trzeba miec jasnosc:
- czy access token niesie tylko `role`,
- czy niesie tez `permission`,
- jakie claim destinations ustawiamy dla permission claims.

Rekomendacja:
- `role` i `permission` powinny trafic do access tokena,
- do id tokena tylko wtedy, gdy swiadomie tego potrzebujemy.

To ogranicza nadmiar danych w id tokenie i utrzymuje spojnosc z API opartym o bearer auth.

## 7. Testy

Minimalny zestaw testow po kroku 5:

### 7.1 Test seeda rol
- po starcie aplikacji istnieja role `user`, `support`, `admin`.

### 7.2 Test mapowania permission claims
- `support` ma oczekiwane permissions,
- `admin` ma oczekiwane permissions,
- `user` nie ma elevated permissions.

### 7.3 Test rejestracji policies
- application startup rejestruje wszystkie oczekiwane policies,
- policy wymaga odpowiedniego claimu `permission`.

### 7.4 Test principal/token preparation
- principal dla usera z rola `support` lub `admin` zawiera claims potrzebne do policy evaluation.

### 7.5 Test integracyjny reprezentatywnego endpointu
- user bez permission dostaje `403`,
- user z odpowiednia rola/permission dostaje sukces.

## 8. Ryzyka i decyzje do podjecia

### 8.1 Permissions tylko na roli vs permissions na userze

Rekomendacja na MVP:
- permissions przez role claims,
- bez per-user overrides.

Powod:
- mniejsza zlozonosc seedingu i testow,
- wystarcza do obecnego modelu `user/support/admin`.

### 8.2 Resource-based authorization

`Orders.ViewOwn` i podobne przypadki nie powinny byc wciskane do tego samego mechanizmu co elevated permissions.

To powinno zostac osobno:
- jako resource handler,
- w kolejnym kroku lub osobnym podkroku.

### 8.3 Zakres rolloutu po modulach

Najwieksze ryzyko kroku 5 to probowanie zabezpieczenia calego API naraz.

Lepsze podejscie:
- najpierw domknac infrastrukture authz,
- potem wdrozyc kilka reprezentatywnych endpointow,
- dopiero potem rozszerzac coverage.

## 9. Definition of Done

Krok 5 uznajemy za zamkniety, gdy:
- role `user`, `support`, `admin` sa seedowane przy starcie,
- role maja oczekiwane permission claims,
- policies sa jawnie zarejestrowane w DI,
- co najmniej wybrane endpointy korzystaja z nowych policies,
- testy potwierdzaja seed, mapowanie i dzialanie policy-based authorization,
- principal/token zawiera claims potrzebne do egzekwowania policies w API.

## 10. Rekomendowana kolejnosc implementacji

1. Dodac stale dla rol, permissions i policies.
2. Dodac mape `role -> permissions`.
3. Zaimplementowac idempotentny seed rol i role claims.
4. Zarejestrowac policies w `AddAuthorization(...)`.
5. Zweryfikowac, jak permissions trafiaja do principal/tokena i dopiac brakujace elementy.
6. Podpiac 1-2 reprezentatywne endpointy pod nowe policies.
7. Dodac testy seed/policy/integration.
