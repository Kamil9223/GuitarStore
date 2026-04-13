# Tests

Data: 2026-04-12
Status: Working agreement

## 1. Cel dokumentu

Ten plik zbiera praktyczne zasady pisania testow w repo `GuitarStore` na podstawie aktualnego kodu testowego i uzgodnionych kierunkow.

To nie jest opis idealnego stanu 1:1. To jest:
- opis obecnych konwencji, ktore juz wystepuja w repo,
- rekomendowany standard dla nowych testow,
- lista znanych wyjatkow i odchylen.

## 2. Stos testowy

Aktualnie w repo:
- framework testowy: `xUnit`,
- asercje w E2E: `Shouldly`,
- klient biznesowego API w E2E: autogenerowany `GuitarStoreClient`,
- infrastruktura E2E: `WebApplicationFactory`,
- kontenery testowe: `Testcontainers`.

Docelowa konwencja dla nowych testow:
- `Tests.Unit` dla testow jednostkowych,
- `Tests.EndToEnd` dla testow integracyjnych / E2E przez realny host aplikacji,
- testy warstwy aplikacyjnej w `Tests.Unit` powinny uzywac `NSubstitute`,
- testy domenowe moga byc pisane bez mock frameworka, jesli wystarcza czyste obiekty domenowe.

Uwaga:
- w aktualnym stanie repo nie ma jeszcze referencji do `NSubstitute`,
- nalezy to traktowac jako standard dla nowych testow jednostkowych warstwy aplikacyjnej, a nie jako cos juz wszedzie wdrozonego.

## 3. Ogolne zasady

- Nazwa testu powinna opisywac scenariusz i oczekiwany efekt, np. `WhenX_ShouldY`.
- Test powinien sprawdzac jedno glowne zachowanie.
- Preferowany uklad to `Arrange / Act / Assert`.
- Dane testowe powinny byc czytelne i lokalne dla testu.
- Dla unikalnych danych zewnetrznych preferujemy `Guid.NewGuid()` zamiast twardych wspoldzielonych wartosci.
- Test ma sprawdzac kontrakt i obserwowalne zachowanie, nie szczegoly implementacyjne.
- Jezeli scenariusz jest asynchroniczny lub eventual-consistent, trzeba to nazwac w tescie i uzyc helpera oczekiwania zamiast `Task.Delay()` jako glownej strategii.

## 4. Testy jednostkowe

### 4.1 Co jest testem jednostkowym

Test jednostkowy:
- nie powinien uruchamiac calego `ServiceProvider`,
- nie powinien stawiac realnej bazy,
- nie powinien polegac na `WebApplicationFactory`,
- nie powinien testowac wiring-u DI.

Jezeli test potrzebuje:
- realnego `ServiceProvider`,
- EF Core,
- hosta HTTP,
- middleware,
- zewnetrznej infrastruktury,
to najczesciej nie jest to juz unit test.

### 4.2 Standard dla warstwy aplikacyjnej

Nowe testy jednostkowe warstwy aplikacyjnej powinny:
- testowac pojedynczy handler / serwis / dekorator,
- stubowac zaleznosci przez `NSubstitute`,
- skupiac sie na logice orchestration, walidacji i wywolywaniu zaleznosci,
- nie uzywac prawdziwych implementacji infra.

Typowy wzorzec:
- `Substitute.For<...>()` dla repozytoriow, serwisow, publisherow, clockow itp.,
- `Received()` / `DidNotReceive()` dla weryfikacji wywolan,
- asercje na rezultacie i skutkach ubocznych.

### 4.3 Standard dla testow domenowych

Testy domenowe:
- powinny operowac na realnych obiektach domenowych,
- nie potrzebuja mock frameworka, jesli logika miesci sie w agregacie / VO / encji,
- powinny sprawdzac reguly biznesowe i invariants.

Dobry przyklad obecnego stylu:
- [CartTest.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.Unit\Customers\Domain\CartTest.cs)

### 4.4 Lekcje z Auth

Auth pokazal wazna zasade:
- jezeli test jednostkowy zaczyna budowac `Identity + EF + ServiceProvider`, to zwykle jest za ciezki jak na unit test.

Dlatego preferujemy:
- wydzielenie malej abstrakcji,
- test logiki na fake/substitute,
- a wiring pozostawienie do testu integracyjnego.

Dobry przyklad obecnego kierunku:
- [AdminInitializerTest.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.Unit\Auth\AdminInitializerTest.cs)

## 5. Testy EndToEnd

### 5.1 Infrastruktura

Testy E2E uruchamiaja realna aplikacje przez `WebApplicationFactory` w srodowisku `TestContainers`.

W E2E korzystamy z `Testcontainers` dla:
- SQL Server,
- RabbitMQ,
- Stripe mock.

Zrodla:
- [TestsContainers.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\Setup\TestsContainers.cs)
- [Application.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\Setup\Application.cs)
- [MemoryConfigurationTestSource.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\Setup\MemoryConfigurationTestSource.cs)

Konfiguracja testowa:
- podmienia connection string do SQL,
- podmienia RabbitMQ,
- ustawia URL stripe-mock,
- ustawia config auth pod testy,
- wylacza `SeedAdmin` w E2E.

### 5.2 Dostep do API

Dla biznesowych endpointow API preferujemy:
- `TestContext.GuitarStoreClient`

Powod:
- klient jest typowany,
- obsluguje kontrakty request/response,
- upraszcza asercje i obsluge `ApiException`.

Przyklady:
- [AddProductTest.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\E2E_Catalog\Endpoints\AddProductTest.cs)
- [PlaceOrderTest.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\E2E_Orders\Endpoints\PlaceOrderTest.cs)
- [IncreaseStockQuantityTest.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\E2E_Warehouse\Endpoints\IncreaseStockQuantityTest.cs)

Surowy `HttpClient` jest uzasadniony tylko wtedy, gdy testujemy:
- HTML,
- formularze,
- cookies,
- redirecty,
- anty-forgery,
- OIDC endpoints typu `/connect/*`,
- discovery document,
- inne scenariusze protokolarne niewystawione przez OpenAPI.

Przyklady:
- [AccountControllerTest.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\E2E_Auth\AccountControllerTest.cs)
- [AuthorizationCodeFlowTest.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\E2E_Auth\AuthorizationCodeFlowTest.cs)

### 5.3 Seedowanie danych

W E2E preferujemy seedowanie przez dedykowane seedery / extension methods, a nie reczne tworzenie wszystkiego inline.

Obecne wzorce:
- `CatalogDbSeeder`
- `CustomersDbSeeder`
- `OrdersDbSeeder`
- `WarehouseDbSeeder`
- `AuthTestDataSeeder`

Zrodla:
- [CatalogDbSeeder.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\Setup\Modules\Catalog\CatalogDbSeeder.cs)
- [CustomersDbSeeder.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\Setup\Modules\Customers\CustomersDbSeeder.cs)
- [OrdersDbSeeder.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\Setup\Modules\Orders\OrdersDbSeeder.cs)
- [WarehouseDbSeeder.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\Setup\Modules\Warehouse\WarehouseDbSeeder.cs)
- [AuthTestDataSeeder.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\Setup\Modules\Auth\AuthTestDataSeeder.cs)

Zasada:
- jesli nowy test potrzebuje powtarzalnego setupu DB, dopisujemy lub rozszerzamy seeder,
- nie duplikujemy duzych blokow konstrukcji danych pomiedzy testami.

Wyjatek:
- pojedyncze, male obiekty eventow lub requestow moga byc tworzone inline w tescie.

### 5.4 Dostep do DB i weryfikacja skutkow

E2E zwykle:
- przygotowuje dane przez seeder,
- wywoluje endpoint lub publikuje event,
- weryfikuje finalny stan przez odpowiedni `DbContext`.

Do dostepu do kontekstow sluzy:
- [DbsAccessor.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\Setup\Modules\Common\DbsAccessor.cs)

Zasady:
- po zmianach asynchronicznych lub po odczycie tego samego rekordu wielokrotnie czyscimy `ChangeTracker`,
- asercje robimy na stanie finalnym, nie na przypuszczeniu, ze handler skonczyl natychmiast.

## 6. Testy event handlerow

Testy event handlerow w tym repo sa testami integracyjnymi / E2E, nie unit testami.

Typowy wzorzec:
- zseedowac stan poczatkowy w bazie,
- opublikowac event przez `RabbitMqChannel.PublishTestEvent(...)`,
- poczekac na efekt przez `Waiter.WaitForCondition(...)`,
- wyczyscic `ChangeTracker`,
- sprawdzic finalny stan bazy.

Zrodla:
- [UserRegisteredEventTest.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\E2E_Customers\EventHandlers\UserRegisteredEventTest.cs)
- [ProductAddedEventHandler.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\E2E_Customers\EventHandlers\ProductAddedEventHandler.cs)
- [WarehouseEventHandlersTests.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\E2E_Warehouse\EventHandlers\WarehouseEventHandlersTests.cs)
- [RabbitMqExtensions.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\Setup\TestsHelpers\RabbitMqExtensions.cs)
- [Waiter.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\Setup\TestsHelpers\Waiter.cs)

Warto utrzymac nastepujace zasady:
- event handler testuje sie przez realna publikacje eventu, a nie przez bezposrednie wywolanie metody handlera,
- dla scenariuszy idempotencji dostarczamy ten sam event wiecej niz raz,
- asercje opieramy o finalny stan danych,
- timeouty powinny byc krotkie, ale realistyczne dla async processing.

## 7. Obsluga zewnetrznych zaleznosci w E2E

RabbitMQ:
- uzywany do realnego testowania przeplywu eventow miedzy modulami.

Stripe:
- w E2E nie wolamy prawdziwego Stripe,
- serwis jest nadpisywany przez `TestStripeService`.

Zrodla:
- [OverrideServicesSetup.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\Setup\Modules\Common\OverrideServicesSetup.cs)
- [TestStripeService.cs](C:\Moje\Projekty IT\GuitarStore\GuitarStore\Tests.EndToEnd\Setup\Modules\Payments\TestStripeService.cs)

Zasada:
- zewnetrzne uslugi, ktore nie sa celem testu, stubujemy lub podmieniamy w setupie E2E,
- infrastrukture transportowa i persistence testujemy realnie, jesli jest to czesc kontraktu systemu.

## 8. Asercje i obsluga bledow

W E2E preferujemy:
- `Shouldly` do czytelnych asercji,
- `ApiException` z klienta do sprawdzania statusow i odpowiedzi bledow.

W testach biznesowych:
- dla endpointow sukcesu sprawdzamy najpierw wynik API,
- potem stan danych lub skutki uboczne,
- dla endpointow blednych sprawdzamy kod HTTP i, gdy ma to sens, tresc odpowiedzi.

## 9. Znane odchylenia i dlug techniczny

Auth:
- testy Auth nie zawsze przestrzegaja wszystkich zasad z tego dokumentu,
- maja wiecej helperow niz pozostale obszary,
- nie zawsze korzystaja z `DbSeedera`,
- czesc z nich testuje HTML/OIDC i z definicji musi isc surowym `HttpClient`.

Starsze testy:
- niektore testy maja jeszcze nazwy lub komentarze o charakterze roboczym,
- nie wszystkie sa idealnie konsekwentne w `Arrange / Act / Assert`,
- zdarzaja sie testy, ktore moglyby lepiej wykorzystywac seedery.

To nie blokuje nowych zmian, ale:
- nowe testy powinny juz trzymac sie zasad z tego pliku,
- przy dotykaniu starszych testow warto je przy okazji doprowadzac do tego standardu.

## 10. Praktyczne reguly dla nowych testow

- Dla endpointow biznesowych używaj `GuitarStoreClient`.
- Dla HTML/OIDC/UI używaj surowego `HttpClient`.
- Dla E2E przygotowuj dane przez seedery modułów.
- Dla event handlerow publikuj realny event do RabbitMQ i czekaj na stan koncowy.
- Dla warstwy aplikacyjnej pisz lekkie unit testy i mockuj zaleznosci przez `NSubstitute`.
- Nie buduj `ServiceProvider` w testach jednostkowych, jesli testujesz logike a nie wiring.
- Jezeli test potrzebuje prawdziwej bazy, hosta albo kontenerow, to powinien trafic do `Tests.EndToEnd`, nie do `Tests.Unit`.
