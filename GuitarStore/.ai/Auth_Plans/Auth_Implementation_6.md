# Auth Implementation 6

Data: 2026-03-29
Status: Draft

## 0. Cel kroku 6

Krok 6 realizuje sekcje `Customers integration` z `.ai/Auth.md`.

Po tym kroku:
- rejestracja w Auth tworzy nie tylko konto loginowe, ale tez klienta w module `Customers`,
- integracja jest event-driven,
- handler po stronie `Customers` jest idempotentny,
- po poprawnej rejestracji powstaje rowniez pusty koszyk klienta.

Krok 6 nie powinien jeszcze domykac:
- seed admin,
- pelnych testow auth code flow i refresh rotation,
- produkcyjnego bootstrapu,
- pelnego outboxa w Auth, jesli zdecydujemy sie na MVP direct publish.

## 1. Wejscie do kroku 6

Zakladany stan po kroku 5:
- Auth ma Identity + OpenIddict,
- UI logowania i rejestracji dziala,
- klient SPA jest rejestrowany przy starcie,
- role i policies sa skonfigurowane,
- nowy user w Auth dostaje role `user`.

Aktualny stan `Customers` w repo:
- istnieje `SignedUpEvent` w `Customers.Application/Customers/Events/Incoming/SignedUpEvent.cs`,
- handler tworzy `Customer` i od razu `Cart`,
- deduplikacja jest dzis po `Email`,
- event jest `internal`,
- `Customer.Create(...)` wymaga `Name`, `LastName`, `Email`,
- model `Customer` nie przechowuje jeszcze `UserId/AuthUserId`,
- `EventBusSubscriptionManager` w `Customers.Application` subskrybuje na razie tylko `ProductAddedEvent`.

To oznacza, ze krok 6 nie jest tylko "dopisaniem publish event":
- trzeba doprecyzowac kontrakt eventu,
- trzeba zdecydowac, gdzie zyje payload integracyjny,
- trzeba rozwiazac temat `Name/LastName`,
- trzeba domknac idempotencje.

## 2. Kluczowa decyzja dla kroku 6

W `.ai/Auth.md` wybrana opcja dla danych klienta to:
- rozszerzyc rejestracje w Auth UI o `Name` i `LastName`.

Ta decyzja jest konieczna, bo obecny `Customers`:
- nie pozwala utworzyc `Customer` bez `Name` i `LastName`,
- tworzy klienta juz na etapie signup,
- nie ma jeszcze modelu `CustomerDraft`.

Wniosek:
- krok 6 musi rozszerzyc `RegisterViewModel`, widok i `POST /auth/register`,
- Auth publikuje event dopiero po udanym utworzeniu usera.

## 3. Docelowy kontrakt integracyjny

### 3.1 Rekomendowany event

Rekomendacja:
- wprowadzic nowy publiczny kontrakt integracyjny, np. `UserRegisteredIntegrationEvent`.

Payload:
- `UserId` (`Guid`)
- `Email`
- `Name`
- `LastName`
- `OccurredAtUtc`

Powody:
- obecny `SignedUpEvent` jest `internal` i siedzi w `Customers.Application`, wiec Auth nie powinien od niego zalezec,
- `UserId` jest potrzebny do docelowej idempotencji i przyszlych powiazan Auth -> Customers,
- publiczny kontrakt powinien zyc w warstwie shared/contracts, a nie wewnatrz implementacji konsumenta.

### 3.2 Gdzie powinien zyc kontrakt

Najbardziej naturalne opcje:
- `Customers.Shared/Events/*`
- albo osobny projekt kontraktow integracyjnych dla Auth/Customers.

Rekomendacja dla obecnego repo:
- wykorzystac `Customers.Shared`, bo projekt juz istnieje i sluzy do wspoldzielonych kontraktow.

Nie rekomenduje:
- publikowania typu z `Customers.Application`,
- referencji `Auth -> Customers.Application`,
- zostawienia eventu jako `internal`.

## 4. Model danych po stronie Customers

W glownym planie idempotencja byla zalozona po `UserId`, ale obecny model `Customer` go nie ma.

Dlatego krok 6 powinien rozszerzyc `Customers` o:
- `UserId` lub `AuthUserId` typu `Guid`,
- unikalny indeks po tym polu,
- migracje dla `Customers`.

Decyzja modelowa:
- nie przenosic typu `AuthId` do `Customers`,
- na granicy modulow uzyc zwyklego `Guid`.

Rekomendowana nazwa:
- `AuthUserId`

Powody:
- jasno komunikuje zrodlo identyfikatora,
- nie miesza biznesowego `CustomerId` z identyfikatorem konta auth,
- upraszcza integracje miedzy modulami.

## 5. Idempotencja

Obecny handler `SignedUpEvent`:
- sprawdza `Exists(x => x.Email == @event.Email, ct)`,
- przy duplikacie rzuca `DomainException`.

To nie jest dobra idempotencja dla integration eventu.

Rekomendacja:
- docelowo deduplikowac po `AuthUserId`,
- przy duplikacie zrobic `no-op`, a nie exception,
- opcjonalnie sprawdzic tez email jako dodatkowa walidacje ochronna.

Konsekwencja:
- ponowne dostarczenie tego samego eventu nie powinno psuc systemu,
- logika konsumenta staje sie odporna na retry i re-delivery.

## 6. Publish po stronie Auth

Krok 6 po stronie Auth powinien:
- po udanym `CreateAsync(user, password)` i przypisaniu roli `user`,
- opublikowac event `UserRegisteredIntegrationEvent`,
- dopiero potem finalizowac redirect/sign-in flow.

Wazna kolejnosc:
1. utworzenie usera Auth,
2. przypisanie roli `user`,
3. publish eventu integracyjnego,
4. sign-in cookie i redirect.

### 6.1 MVP vs niezawodnosc

Sa dwa warianty:

Wariant A: MVP direct publish
- `IIntegrationEventPublisher` od razu publikuje event po udanym zapisie usera.

Zalety:
- najmniejszy zakres zmian,
- dobrze pasuje do obecnej infrastruktury RabbitMQ.

Wada:
- brak gwarancji publish przy awarii procesu miedzy DB commit a publish.

Wariant B: Auth outbox
- Auth zapisuje event do outboxa w tej samej transakcji co user,
- background dispatcher publikuje go asynchronicznie.

Zalety:
- lepsza niezawodnosc.

Wady:
- wiekszy zakres: tabela outbox, publisher, dispatcher, migracje, wiring.

Rekomendacja dla kroku 6:
- wdrozyc Wariant A jako MVP,
- jawnie odnotowac outbox w Auth jako follow-up hardening.

Powod:
- glowny plan traktuje outbox jako opcjonalny,
- najpierw trzeba domknac funkcjonalny przeplyw signup -> customer.

## 7. Scope implementacyjny

Krok 6 obejmuje:
- rozszerzenie `RegisterViewModel` i widoku o `Name` i `LastName`,
- publikacje integration eventu z Auth po rejestracji,
- nowy publiczny kontrakt eventu w shared layer,
- subskrypcje eventu po stronie `Customers`,
- utworzenie `Customer` + `Cart`,
- migracje `Customers` pod `AuthUserId`,
- idempotentny handler,
- testy integracyjne Auth -> Customers.

Krok 6 nie obejmuje:
- email confirmation,
- update profilu klienta po rejestracji,
- outbox w Auth,
- panel admina do zarzadzania klientami/profilami.

## 8. Plan implementacji technicznej

### Etap 6.1: Publiczny kontrakt eventu

Dodac nowy event integracyjny do shared projectu, np.:
- `Customers.Shared/Events/UserRegisteredIntegrationEvent.cs`

Event powinien:
- dziedziczyc po `IntegrationEvent`,
- implementowac `IIntegrationPublishEvent` i/lub `IIntegrationConsumeEvent` zgodnie z obecnym wzorcem,
- byc publiczny.

### Etap 6.2: Rozszerzenie rejestracji Auth UI

Zmiany po stronie `GuitarStore.ApiGateway`:
- `RegisterViewModel`:
  - `Name`
  - `LastName`
  - `Email`
  - `Password`
  - `ConfirmPassword`
- `Views/Account/Register.cshtml`
- `AccountController.Register(...)`

Walidacja:
- `Name` i `LastName` wymagane,
- zachowanie dotychczasowych zasad dla hasla i emaila.

### Etap 6.3: Publish eventu po stronie Auth

Po udanym utworzeniu usera i przypisaniu roli:
- zbudowac `UserRegisteredIntegrationEvent`,
- opublikowac przez `IIntegrationEventPublisher`.

W razie bledu publish w MVP:
- traktowac to jako blad operacji rejestracji,
- nie robic silent failure.

To oznacza, ze krok 6 wymaga jasnej decyzji, czy przy publish failure:
- rollbackujemy operacje,
- czy zostawiamy usera Auth i pokazujemy blad.

Rekomendacja MVP:
- jesli nie ma outboxa, blad publish powinien failowac request i byc jawny,
- ewentualny cleanup usera Auth trzeba rozwazyc osobno.

### Etap 6.4: Rozszerzenie modelu Customers

Dodac `AuthUserId` do:
- `Customers.Domain/Customers/Customer.cs`
- konfiguracji EF w `CustomerDbConfiguration`
- migracji `Customers`

Dodatkowo:
- indeks unikalny po `AuthUserId`,
- ewentualnie indeks unikalny po `Email`, jesli chcemy zachowac obecna ochrone.

### Etap 6.5: Nowy handler eventu w Customers

Zastapic lub przepiac obecny `SignedUpEventHandler` na nowy kontrakt.

Handler powinien:
- sprawdzac istnienie klienta po `AuthUserId`,
- przy duplikacie robic `return`,
- przy braku klienta utworzyc `Customer`,
- utworzyc `Cart`,
- zapisac wszystko w jednym `SaveChangesAsync(ct)`.

### Etap 6.6: Subskrypcja eventu

Rozszerzyc `Customers.Application/EventBusSubscriptionManager.cs` o subskrypcje nowego eventu.

Po kroku 6 `Customers` powinien nasluchiwac co najmniej:
- `ProductAddedEvent`
- `UserRegisteredIntegrationEvent`

### Etap 6.7: Porzadek po starym `SignedUpEvent`

Po wdrozeniu nowego kontraktu trzeba zdecydowac:
- czy stary `SignedUpEvent` usuwamy,
- czy zostawiamy chwilowo jako adapter kompatybilnosci.

Rekomendacja:
- usunac stary internal event po przepieciu handlera,
- zostawic jeden kanoniczny kontrakt integracyjny.

## 9. Testy

Minimalny zestaw testow dla kroku 6:

### 9.1 Register -> event publish -> customer created
- po `POST /auth/register` powstaje user Auth,
- po stronie `Customers` pojawia sie `Customer`,
- tworzony jest rowniez pusty `Cart`.

### 9.2 Idempotencja handlera
- dwa razy dostarczony ten sam event nie tworzy duplikatu klienta,
- nie leci exception domenowy przy duplikacie.

### 9.3 Mapping danych
- `Name`
- `LastName`
- `Email`
- `AuthUserId`

wszystkie sa poprawnie przepisane do `Customers`.

### 9.4 UI register validation
- brak `Name` albo `LastName` zwraca blad walidacji,
- poprawne dane przechodza przez caly flow.

## 10. Ryzyka i otwarte decyzje

### 10.1 Brak outboxa w Auth

Jesli zostajemy przy direct publish:
- system jest funkcjonalny,
- ale nie ma pelnej gwarancji dostarczenia przy awarii procesu.

To trzeba jawnie zapisac jako follow-up.

### 10.2 Co z istniejacymi userami Auth bez klienta

Po wdrozeniu kroku 6 moga istniec konta Auth utworzone wczesniej, dla ktorych nie ma jeszcze rekordu `Customer`.

Trzeba zdecydowac:
- czy ignorujemy to jako dane dev/test,
- czy dodajemy jednorazowy backfill script.

Rekomendacja:
- na MVP potraktowac to jako backfill administracyjny poza zakresem kroku 6,
- chyba ze w danych developerskich to realny blocker.

### 10.3 Cleanup przy publish failure

Bez outboxa trzeba podjac decyzje:
- czy po nieudanym publish usuwamy dopiero co utworzonego usera Auth,
- czy zwracamy blad i zostawiamy konto.

Rekomendacja:
- opisac to jawnie w implementacji i testach,
- nie zostawiac tego jako przypadkowego efektu ubocznego.

## 11. Definition of Done

Krok 6 uznajemy za zamkniety, gdy:
- rejestracja w Auth zbiera `Name` i `LastName`,
- Auth publikuje publiczny event integracyjny po signup,
- `Customers` konsumuje ten event,
- powstaje `Customer` z `AuthUserId`, `Name`, `LastName`, `Email`,
- powstaje `Cart` dla nowego klienta,
- handler jest idempotentny,
- istnieja testy integracyjne potwierdzajace caly flow.

## 12. Rekomendowana kolejnosc implementacji

1. Dodac publiczny kontrakt eventu w shared layer.
2. Rozszerzyc register UI o `Name` i `LastName`.
3. Rozszerzyc model `Customer` o `AuthUserId` i dodac migracje.
4. Zaimplementowac handler nowego eventu po stronie `Customers`.
5. Dodac subskrypcje eventu w `Customers`.
6. Podpiac publish eventu po stronie Auth po udanej rejestracji.
7. Dodac testy end-to-end i test idempotencji.
