# Zarządzanie konfiguracją

Data: 2026-04-19
Status: Standard projektowy

---

## 1. Cel dokumentu

Ten plik opisuje implementacyjny standard zarządzania konfiguracją w GuitarStore.
Skrócone konwencje są w `AGENTS.md` sekcja 11. Ten dokument zawiera pełny opis z przykładami.

---

## 2. Struktura plików

Wszystkie pliki konfiguracyjne leżą w `GuitarStore.ApiGateway/`.

```
GuitarStore.ApiGateway/
  appsettings.json                       ← infrastruktura wspólna
    appsettings.Development.json         ← lokalne overrides infrastruktury
    appsettings.Test.json                ← overrides dla E2E testów
    appsettings.Eod.json                 ← overrides dla środowiska on-demand (PR)

  appsettings.Auth.json                  ← Auth: production-safe defaults
    appsettings.Auth.Development.json    ← Auth: lokalne overrides
    appsettings.Auth.Test.json           ← Auth: overrides dla E2E testów

  appsettings.Orders.json                ← Orders: production-safe defaults
    appsettings.Orders.Development.json  ← (opcjonalne, gdy moduł ma lokalne wartości)

  appsettings.Catalog.json
  appsettings.Customers.json
  appsettings.Payments.json
  appsettings.Warehouse.json
  appsettings.Delivery.json
```

Wcięcia pokazują zagnieżdżenie w Rider/VS (przez `<DependentUpon>` w `.csproj`).

---

## 3. Priorytety ładowania (najniższy → najwyższy)

```
appsettings.json
appsettings.{Env}.json
appsettings.{Modul}.json          (per moduł)
appsettings.{Modul}.{Env}.json    (per moduł, per środowisko)
User Secrets                       (tylko Development)
Environment Variables              ← WYGRYWAJĄ Z PLIKAMI
Command Line args                  ← NAJWYŻSZY PRIORYTET
```

Zmienne środowiskowe zawsze nadpisują pliki. Na produkcji sekrety są wstrzykiwane jako env vars przez Azure App Configuration lub Terraform.

---

## 4. Implementacja – `Program.cs`

Standardowy `WebApplication.CreateBuilder` ładuje pliki modułów **po** zmiennych środowiskowych, co odwraca priorytety. Należy przebudować pipeline:

```csharp
var builder = WebApplication.CreateBuilder(args);
var env = builder.Environment;

// Czyścimy domyślny pipeline i budujemy z prawidłową kolejnością
builder.Configuration.Sources.Clear();

// Warstwa 1: Globalna infrastruktura (najniższy priorytet)
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

// Warstwa 2: Konfiguracje modułów (base + env override)
string[] modules = ["Auth", "Catalog", "Customers", "Delivery", "Orders", "Payments", "Warehouse"];
foreach (var module in modules)
{
    builder.Configuration
        .AddJsonFile($"appsettings.{module}.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{module}.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
}

// Warstwa 3: Overrides (najwyższy priorytet – zawsze wygrywają z plikami)
if (env.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>(optional: true);
    DotNetEnv.Env.Load(options: new DotNetEnv.LoadOptions());
}
builder.Configuration
    .AddEnvironmentVariables()
    .AddCommandLine(args);
```

---

## 5. Zagnieżdżanie plików w Rider / Visual Studio

Pliki modułów zagnieżdżają się pod plikiem bazowym przez `<DependentUpon>` w `GuitarStore.ApiGateway.csproj`:

```xml
<ItemGroup>
  <!-- appsettings.Test.json pod appsettings.json -->
  <Content Update="appsettings.Test.json">
    <DependentUpon>appsettings.json</DependentUpon>
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>

  <!-- Auth -->
  <Content Update="appsettings.Auth.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
  <Content Update="appsettings.Auth.Development.json">
    <DependentUpon>appsettings.Auth.json</DependentUpon>
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
  <Content Update="appsettings.Auth.Test.json">
    <DependentUpon>appsettings.Auth.json</DependentUpon>
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>

  <!-- Orders -->
  <Content Update="appsettings.Orders.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
  <Content Update="appsettings.Orders.Development.json">
    <DependentUpon>appsettings.Orders.json</DependentUpon>
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>

  <!-- Catalog -->
  <Content Update="appsettings.Catalog.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
  <Content Update="appsettings.Catalog.Development.json">
    <DependentUpon>appsettings.Catalog.json</DependentUpon>
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>

  <!-- Customers -->
  <Content Update="appsettings.Customers.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
  <Content Update="appsettings.Customers.Development.json">
    <DependentUpon>appsettings.Customers.json</DependentUpon>
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>

  <!-- Payments -->
  <Content Update="appsettings.Payments.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
  <Content Update="appsettings.Payments.Development.json">
    <DependentUpon>appsettings.Payments.json</DependentUpon>
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>

  <!-- Warehouse -->
  <Content Update="appsettings.Warehouse.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
  <Content Update="appsettings.Warehouse.Development.json">
    <DependentUpon>appsettings.Warehouse.json</DependentUpon>
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>

  <!-- Delivery -->
  <Content Update="appsettings.Delivery.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
  <Content Update="appsettings.Delivery.Development.json">
    <DependentUpon>appsettings.Delivery.json</DependentUpon>
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

Uwaga: `appsettings.Development.json` jest już automatycznie zagnieżdżone pod `appsettings.json` przez `Microsoft.NET.Sdk.Web` – nie trzeba tego deklarować ręcznie.

---

## 6. Zawartość plików – przykłady

### `appsettings.json`
Wyłącznie infrastruktura współdzielona. Puste placeholdery dla sekretów (uzupełniane przez env vars na produkcji).

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "GuitarStore": "",
    "RabbitMq": ""
  }
}
```

### `appsettings.Development.json`
Lokalne connection stringi i narzędzia deweloperskie.

```json
{
  "ConnectionStrings": {
    "GuitarStore": "Server=localhost,1433;Database=GuitarStore;User Id=sa;Password=Secret@Passw0rd;TrustServerCertificate=True;",
    "RabbitMq": "amqp://guest:guest@localhost:5672"
  },
  "Stripe": {
    "Url": "https://api.stripe.com",
    "SecretKey": "",
    "WebhookSecret": ""
  }
}
```

### `appsettings.Test.json`
Statyczne overrides dla E2E. Connection stringi i inne dynamiczne wartości zostają w `MemoryConfigurationTestSource`.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  }
}
```

### `appsettings.Auth.json`
Production-safe defaults modułu Auth. Bez localhost URL, bez dev certów, seed admin wyłączony.

```json
{
  "Auth": {
    "Issuer": "",
    "AccessTokenMinutes": 15,
    "RefreshTokenDays": 30,
    "RequireEmailConfirmed": true,
    "Scopes": {
      "IncludeProfileScope": true
    },
    "Clients": [],
    "Certificates": {
      "UseDevelopmentCertificates": false
    },
    "Password": {
      "RequiredLength": 8,
      "RequireDigit": true,
      "RequireLowercase": true,
      "RequireUppercase": true,
      "RequireNonAlphanumeric": true
    },
    "Lockout": {
      "MaxFailedAccessAttempts": 5,
      "DefaultLockoutMinutes": 10
    }
  },
  "SeedAdmin": {
    "Enabled": false
  }
}
```

### `appsettings.Auth.Development.json`
Lokalne overrides dla Auth.

```json
{
  "Auth": {
    "Issuer": "https://localhost:7028",
    "RequireEmailConfirmed": false,
    "Certificates": {
      "UseDevelopmentCertificates": true
    },
    "Clients": [
      {
        "ClientId": "guitarstore-spa",
        "DisplayName": "GuitarStore SPA",
        "RedirectUris": ["http://localhost:3000/auth/callback"],
        "PostLogoutRedirectUris": ["http://localhost:3000/auth/logout-callback"]
      }
    ]
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000"]
  },
  "SeedAdmin": {
    "Enabled": true,
    "Email": "admin@guitarstore.local",
    "Password": "ChangeMe!123"
  }
}
```

### `appsettings.Auth.Test.json`
Statyczne wartości dla E2E testów Auth.

```json
{
  "Auth": {
    "Issuer": "https://localhost:5001",
    "AccessTokenMinutes": 60,
    "RequireEmailConfirmed": false,
    "Certificates": {
      "UseDevelopmentCertificates": true
    },
    "Clients": [
      {
        "ClientId": "guitarstore-spa",
        "DisplayName": "GuitarStore SPA",
        "RedirectUris": ["https://localhost:5001/auth/callback"],
        "PostLogoutRedirectUris": ["https://localhost:5001/"]
      }
    ]
  },
  "Cors": {
    "AllowedOrigins": ["https://localhost:5001"]
  },
  "SeedAdmin": {
    "Enabled": false
  }
}
```

### `appsettings.Orders.json`
Ustawienia modułu Orders wyciągnięte z pliku globalnego.

```json
{
  "Orders": {
    "ReservationTtlMinutes": 10
  }
}
```

---

## 7. Sekrety na produkcji

Sekrety **nigdy nie trafiają do commitowanych plików**. Zastępowane przez:

| Mechanizm | Zastosowanie |
|---|---|
| User Secrets (`dotnet user-secrets`) | lokalne dev, nie commitowane |
| Zmienne środowiskowe | produkcja, CI/CD |
| Azure App Configuration / App Settings | ręczny override per środowisko |
| Terraform | globalne zarządzanie per środowisko |

Format zmiennej środowiskowej dla zagnieżdżonych sekcji:
```
Auth__Issuer=https://prod.guitarstore.com
ConnectionStrings__GuitarStore=Server=prod-server;...
```

Podwójny underscore (`__`) jest separatorem hierarchii w .NET configuration.

---

## 8. Środowisko testów a `MemoryConfigurationTestSource`

Aktualna implementacja E2E (`Tests.EndToEnd/Setup/Application.cs`) używa środowiska `"TestContainers"` i **czyści cały pipeline konfiguracji** przez `config.Sources.Clear()`, zastępując go wyłącznie `MemoryConfigurationTestSource`. Żadne pliki `appsettings.*.json` nie są ładowane podczas testów.

Konsekwencje:
- Pliki `appsettings.TestContainers.json` / `appsettings.Auth.TestContainers.json` **nie są** aktualnie używane.
- Zmiany w `Program.cs` (pipeline) nie wpływają na testy – testy mają własną izolowaną konfigurację.

Podział odpowiedzialności w `MemoryConfigurationTestSource`:

| Co | Gdzie |
|---|---|
| Connection stringi z Testcontainers | `MemoryConfigurationTestSource` (dynamiczne, runtime) |
| Auth config dla testów (Issuer, Clients, Certs) | `MemoryConfigurationTestSource` (statyczne, ale powiązane z env) |
| Flagi i overrides testowe | `MemoryConfigurationTestSource` |

Jeśli w przyszłości testy zostaną zrefaktorowane do ładowania plików JSON, należy:
1. Zmienić `UseEnvironment("TestContainers")` na `UseEnvironment("Test")`.
2. Usunąć `config.Sources.Clear()` lub zawęzić czyszczenie do dynamicznych wartości.
3. Dodać `appsettings.Test.json` i `appsettings.Auth.Test.json` ze statycznymi wartościami.

---

## 9. Dodawanie nowego środowiska

Nowe środowisko (np. `QA`) nie wymaga zmian w kodzie – wystarczy:

1. Dodać pliki `appsettings.QA.json` i `appsettings.{Modul}.QA.json`.
2. Ustawić `ASPNETCORE_ENVIRONMENT=QA` na docelowym serwerze.
3. Opcjonalnie dodać wpisy `<DependentUpon>` w `.csproj` dla porządku w IDE.

---

## 10. Historia zmian

### 2026-04-19 – refaktoryzacja wdrożona

- [x] `Program.cs` – przebudowano pipeline: `Sources.Clear()` + prawidłowa kolejność (pliki → env vars → CLI)
- [x] `appsettings.Auth.json` – rozbity na production-safe base + `appsettings.Auth.Development.json`
- [x] `appsettings.json` – usunięto `Orders.ReservationTtlMinutes` i lokalne connection stringi
- [x] `appsettings.Development.json` – dodano lokalne connection stringi (SQL, RabbitMQ)
- [x] `appsettings.Orders.json` – utworzono z `Orders.ReservationTtlMinutes`
- [x] `<DependentUpon>` w `.csproj` – dodano zagnieżdżenie dla plików modułów
