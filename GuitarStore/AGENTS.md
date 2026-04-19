# GuitarStore – Konwencje Projektu

> Dokument opisuje architekturę i standardy kodowania obowiązujące w całym projekcie.
> Jest źródłem prawdy dla każdego, kto dodaje lub zmienia kod – człowieka i agenta AI.

---

## 1. Przegląd projektu

**Modularny monolit** napisany w **.NET 8**, hostowany w jednym procesie `GuitarStore.ApiGateway`.
Jedna baza **MSSQL**, osobny schemat per moduł.
Frontend: React SPA (OIDC Authorization Code + PKCE).

---

## 2. Struktura solucji

```
GuitarStore.ApiGateway/          – host; thin API controllers + MVC Razor views (auth UI)
  Modules/
    Catalog/                     – thin API controllers dla modułu Catalog
    Orders/                      – thin API controllers dla modułu Orders
    Auth/                        – MVC Razor controllers + OIDC controllers
    ...

<Modul>.Application/             – handlery CQRS, walidatory, abstrakcje repozytoriów
<Modul>.Domain/                  – encje, value objects, agregaty, reguły domenowe
<Modul>.Infrastructure/          – DbContext (EF Core), repozytoria, query services
<Modul>.Shared/                  – publiczny kontrakt modułu (interfaces + DTOs)
<Modul>.Core/                    – używane dla mniejszych modułów zamiast Application+Domain+Infrastructure

Common.*/                        – building blocki bez logiki domenowej (patrz sekcja 9)
Tests.Unit/                      – unit testy (xUnit + NSubstitute)
Tests.EndToEnd/                  – testy E2E (xUnit + WebApplicationFactory + Testcontainers)
```

### Architektura modułów

| Typ modułu | Projekty | Przykłady |
|---|---|---|
| Duży (Clean Architecture) | Application + Domain + Infrastructure + Shared | Catalog, Orders, Customers, Warehouse |
| Mały | Core + Shared | Auth, Payments, Delivery |

---

## 3. Cienkie kontrolery (Thin Controller Rule)

**API kontrolery muszą być cienkie.** Jedyną odpowiedzialnością jest: przyjąć request HTTP → wywołać executor → zwrócić response.

API kontrolery wstrzykują wyłącznie `ICommandHandlerExecutor` i `IQueryHandlerExecutor`.
**Nigdy** nie wstrzykuj serwisów domenowych ani aplikacyjnych bezpośrednio do API kontrolerów.

```csharp
// PRAWIDŁOWO
[HttpPost]
public async Task<IActionResult> Create(AddProductCommand request, CancellationToken ct)
{
    await _commandHandlerExecutor.Execute(request, ct);
    return Ok();
}

[HttpGet("{orderId}")]
public async Task<ActionResult<OrderDetailsResponse>> GetDetails([FromRoute] OrderId orderId, CancellationToken ct)
{
    var response = await _queryHandlerExecutor.Execute<GetOrderDetailsQuery, OrderDetailsResponse>(
        new GetOrderDetailsQuery(orderId), ct);
    return Ok(response);
}

// BŁĄD – nie wstrzykuj serwisów domenowych do API kontrolera
public ProductsController(IProductService productService) { ... }
```

**Wyjątek:** Kontrolery MVC (Razor) w module Auth (`AccountController`) mogą wstrzykiwać serwisy aplikacyjne bezpośrednio, ponieważ zarządzają ModelState formularzy. Nie ma tu `ICommandHandlerExecutor`.

---

## 4. Wzorzec CQRS

Każda operacja biznesowa przechodzi przez handler komendy lub zapytania.

### Command (zapis, bez odpowiedzi)
```csharp
public sealed record AddProductCommand(string Name, decimal Price) : ICommand;

internal sealed class AddProductCommandHandler : ICommandHandler<AddProductCommand>
{
    public async Task Handle(AddProductCommand command, CancellationToken ct) { ... }
}
```

### Command z odpowiedzią
```csharp
public sealed record PlaceOrderCommand(CustomerId CustomerId) : ICommand;

internal sealed class PlaceOrderCommandHandler : ICommandHandler<PlaceOrderResponse, PlaceOrderCommand>
{
    public async Task<PlaceOrderResponse> Handle(PlaceOrderCommand command, CancellationToken ct) { ... }
}
```

### Query
```csharp
public sealed record GetOrderDetailsQuery(OrderId OrderId) : IQuery;

internal sealed class GetOrderDetailsQueryHandler : IQueryHandler<GetOrderDetailsQuery, OrderDetailsResponse>
{
    public Task<OrderDetailsResponse> Handle(GetOrderDetailsQuery query, CancellationToken ct) { ... }
}
```

Handlery są **`internal sealed`**. Komendy i zapytania są definiowane w projekcie `Application`.

---

## 5. Obsługa błędów – wyłącznie wyjątki

Błędy biznesowe są komunikowane przez **wyjątki**, a nie przez obiekty Result, OneOf, czy statusy enum.
Middleware w ApiGateway automatycznie mapuje wyjątki na problem responses HTTP.

```csharp
// Naruszenie reguły domenowej
throw new DomainException("Product with this name already exists.");

// Zasób nie istnieje
throw new NotFoundException(productId);
```

**NIE używaj** `Result<T>`, `OneOf<Success, Failure>`, statusów enum ani nullable jako sygnałów błędów biznesowych w warstwie Application/Domain.

**Wyjątek od reguły:** Moduł Auth (`AccountController`) używa obiektu rezultatu (`AuthLoginResult`, `AuthRegisterResult`) dla akcji Razor, ponieważ MVC wymaga szczegółowego sterowania ModelState i przekierowaniami. Ta decyzja jest lokalna dla warstwy UI Auth i **nie powinna** być wzorcem dla innych modułów.

---

## 6. Kody błędów

Zdefiniowane centralnie w `Common.Errors/ApplicationErrorCode.cs`.
Korzystaj z istniejących kodów lub dodaj nowe w tym samym pliku.

---

## 7. Komunikacja między modułami

### Asynchroniczna (RabbitMQ)

Moduł publikuje event → inny moduł obsługuje go handlerem.

```csharp
// Publikacja (w handlerze komendy lub serwisie aplikacyjnym)
await integrationEventPublisher.Publish(new UserRegisteredEvent(...), ct);

// Handler w innym module
public class UserRegisteredEventHandler : IIntegrationEventHandler<UserRegisteredEvent> { ... }
```

**Dla zdarzeń wymagających gwarancji dostarczenia użyj wzorca Outbox** (`Common.Outbox`) zamiast bezpośredniej publikacji. Outbox gwarantuje at-least-once delivery nawet przy awarii procesu.

### Synchroniczna

Moduł wystawia publiczny kontrakt w projekcie `.Shared`:

```csharp
// Customers.Shared
public interface ICartService
{
    Task<CheckoutCartDto> GetCheckoutCart(CustomerId customerId, CancellationToken ct);
}
```

Inne moduły referencjonują wyłącznie projekt `.Shared` – **nigdy** `.Application` ani `.Infrastructure` innego modułu. Brak bezpośrednich zależności między modułami.

---

## 8. Rejestracja modułów (DI)

Każdy moduł rejestruje się przez metodę rozszerzającą `AddXxxModule(services, configuration)`.
Wszystkie moduły są wplecione w `GuitarStore.ApiGateway/Configuration/ModulesInitializator.cs`.

Wewnątrz modułów używamy **Scrutor** do automatycznego skanowania przez konwencję:
- Repozytoria: `*Repository`
- Query services: `*QueryService`
- Handlery CQRS: przez implementowany interfejs `ICommandHandler<>` / `IQueryHandler<,>`

---

## 9. Common – building blocki

| Projekt | Zawartość |
|---|---|
| `Common.Errors` | `DomainException`, `NotFoundException`, `GuitarStoreApplicationException`, `ApplicationErrorCode` |
| `Common.EfCore` | `IUnitOfWork`, `ITransactionExecutor<T>`, `ICrossModuleTransactionExecutor` |
| `Common.Outbox` | Outbox pattern dla niezawodnej publikacji eventów |
| `Common.RabbitMq` | Infrastruktura RabbitMQ |
| `Common.RabbitMq.Abstractions` | `IIntegrationEventPublisher`, `IIntegrationEventHandler<T>` |
| `Common.StronglyTypedIds` | Strongly-typed ID types per moduł |

Building blocki **nie zawierają logiki domenowej** – to czysta infrastruktura wielokrotnego użytku.

---

## 10. Strongly Typed IDs

Wszystkie klucze encji domenowych są strongly typed, zdefiniowane w `Common.StronglyTypedIds`.
Przykłady: `AuthId`, `ProductId`, `OrderId`, `CustomerId`.
Nigdy nie używaj surowego `Guid` jako klucza domenowego.

---

## 11. Konfiguracja

Pełna dokumentacja wdrożeniowa: `.ai/Configuration.md`.

### Lokalizacja plików

Wszystkie pliki konfiguracyjne leżą w `GuitarStore.ApiGateway/`. Projekty modułów nie posiadają własnych plików konfiguracyjnych – bindują wyłącznie `IOptions<T>` do sekcji po nazwie.

### Schemat nazewnictwa

```
appsettings.json                        – infrastruktura wspólna (Logging, ConnectionStrings placeholder)
appsettings.{Env}.json                  – overrides infrastruktury per środowisko

appsettings.{Modul}.json               – konfiguracja modułu, production-safe defaults
appsettings.{Modul}.{Env}.json         – overrides modułu per środowisko
```

Przykłady: `appsettings.Auth.json`, `appsettings.Auth.Development.json`, `appsettings.Auth.Test.json`.

### Środowiska

| Nazwa | Kiedy używane |
|---|---|
| `Development` | lokalne środowisko developera |
| `Test` | testy E2E (`WebApplicationFactory`) |
| `Eod` | środowisko on-demand per Pull Request |
| `QA` / `Production` | *(przyszłe)* |

### Zasady

- `appsettings.{Modul}.json` zawiera **production-safe defaults** (bez localhost URL, bez dev certs, `SeedAdmin.Enabled: false`).
- `appsettings.{Modul}.Development.json` zawiera lokalne overrides (localhost, dev certs, seed admin).
- `appsettings.{Modul}.Test.json` zawiera statyczne wartości testowe; dynamiczne overrides (connection stringi z Testcontainers) zostają w `MemoryConfigurationTestSource`.
- **Sekrety nigdy w plikach** – connection stringi, klucze API, hasła certyfikatów trafiają wyłącznie do User Secrets (dev) lub zmiennych środowiskowych (prod).
- **Zmienne środowiskowe wygrywają z plikami** – pipeline konfiguracji ładuje pliki przed `AddEnvironmentVariables()`.
- Konfiguracja jest **walidowana przy starcie** (fail-fast). Brakująca lub błędna konfiguracja = wyjątek przy uruchomieniu, nie w runtime.

---

## 12. Testy

Pełny standard opisany w `.ai/Tests.md`. Poniżej skrót:

| Kategoria | Projekt | Narzędzia |
|---|---|---|
| Unit testy | `Tests.Unit` | xUnit + NSubstitute |
| E2E / integracyjne | `Tests.EndToEnd` | xUnit + WebApplicationFactory + Testcontainers |

### Kluczowe zasady
- **Unit testy nie uruchamiają** `ServiceProvider`, EF Core, ani real bazy. Wszystkie zależności mockowane przez NSubstitute.
- **Handlery CQRS i serwisy aplikacyjne** mają dedykowane unit testy.
- **E2E** testuje przez `GuitarStoreClient` dla API. Surowy `HttpClient` tylko dla HTML/OIDC/cookie flows.
- **Test data** przygotowywana przez dedykowane seedery (np. `CatalogDbSeeder`, `AuthTestDataSeeder`).
- **Eventy** testowane przez realną publikację do RabbitMQ + `Waiter.WaitForCondition(...)`.
- **Nazewnictwo**: `WhenX_ShouldY`.

---

## 13. Wzorce, których NIE stosujemy

| Wzorzec | Powód |
|---|---|
| Result<T> / OneOf w Application/Domain | Projekt używa wyjątków; patrz sekcja 5 |
| Bezpośrednie referencje między modułami | Komunikacja tylko przez `.Shared` (sync) lub events (async) |
| Serwisy domenowe wstrzykiwane bezpośrednio do API kontrolerów | Tylko przez CQRS executors |
| `Task.Delay()` w testach jako strategia oczekiwania | Używaj `Waiter.WaitForCondition(...)` |
| Bezpośrednia publikacja eventów bez Outbox (dla krytycznych eventów) | Ryzyko utraty eventu przy awarii procesu |

---

## 14. Referencje do plików wzorcowych

| Wzorzec | Plik |
|---|---|
| Thin API controller | `GuitarStore.ApiGateway/Modules/Catalog/Products/ProductsController.cs` |
| Command handler | `Customers.Application/Customers/Commands/AddCustomerCommand.cs` |
| Command handler z odpowiedzią | `Orders.Application/Orders/Commands/PlaceOrderCommand.cs` |
| Query handler | `Catalog.Application/Products/Queries/ListProductsQuery.cs` |
| DomainException | `Common.Errors/Exceptions/DomainException.cs` |
| NotFoundException | `Common.Errors/Exceptions/NotFoundException.cs` |
| Shared contract | `Customers.Shared/ICartService.cs` |
| E2E test | `Tests.EndToEnd/E2E_Orders/Endpoints/PlaceOrderTest.cs` |
| Unit test (domenowy) | `Tests.Unit/Customers/Domain/CartTest.cs` |
| Unit test (aplikacyjny) | `Tests.Unit/Auth/AdminInitializerTest.cs` |
