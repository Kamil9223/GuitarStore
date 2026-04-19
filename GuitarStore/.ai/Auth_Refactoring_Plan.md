# Plan refaktoryzacji modułu Auth

Data: 2026-04-19
Status: Do zatwierdzenia

---

## 0. Ocena stanu obecnego

Moduł Auth jest **funkcjonalnie kompletny** (kroki 1–7 zrealizowane, krok 8 w toku).
Jednak w porównaniu do standardów projektu opisanych w `CLAUDE.md` zawiera kilka istotnych odchyleń.

### Co działa zgodnie ze standardami

- `AccountController` i `OpenIddictController` są cienkie (delegują do serwisu/fabryki).
- `AuthModuleInitializator` rejestruje moduł zgodnie z konwencją.
- Strongly-typed ID (`AuthId`), osobny schemat bazy, konfiguracja per moduł.
- Walidacja konfiguracji przy starcie (fail-fast).
- Roles, permissions i policies w `AuthAuthorization.cs` są czytelne i zwarte.
- `OidcClaimsPrincipalFactory` poprawnie deleguje budowę principal poza kontroler.
- `UserRegisteredEvent` jest integration eventem; `AuthTestDataSeeder` istnieje.

### Lista odchyleń

| # | Obszar | Opis | Priorytet |
|---|---|---|---|
| 1 | **Brak Outbox** | `UserRegisteredEvent` publikowany bezpośrednio bez Outbox. Przy awarii po zapisaniu usera event może zostać utracony. | Krytyczny |
| 2 | **Brak CQRS** | `IAuthService` z 8 metodami zamiast command/query handlerów. | Wysoki |
| 3 | **Result pattern zamiast wyjątków (guard conditions)** | `CurrentUserNotFound`, `PasswordChangeNotRequired` to wyjątkowe sytuacje, nie statusy UI – powinny być wyjątkami. | Wysoki |
| 4 | **Brak `Auth.Shared`** | Moduł nie wystawia publicznego kontraktu. Jeśli inny moduł będzie potrzebował danych z Auth, nie ma gdzie ich pobrać bez bezpośredniej referencji. | Średni |
| 5 | **`OpenIddictController` wstrzykuje `UserManager<User>` i `SignInManager<User>` bezpośrednio** | Logika sprawdzania użytkownika powinna być za serwisem, nie bezpośrednio w kontrolerze. | Średni |
| 6 | **Testy jednostkowe warstwy aplikacyjnej** | Brak unit testów dla `AuthService`. `AdminInitializerTest.cs` buduje realny `ServiceProvider` z Identity + EF (znany dług opisany w `Tests.md`). | Średni |
| 7 | **`try/catch/delete` w `RegisterAsync`** | Rollback przez `userManager.DeleteAsync(user)` po nieudanej publikacji eventu jest kruchy i nieatomowy. Outbox eliminuje ten problem. | Rozwiązany przez #1 |
| 8 | **`IAuthService` jest publiczny** | Interfejs jest `public` i dostępny poza modułem. Kontrolery w ApiGateway potrzebują go, ale warto ocenić zakres widoczności po refaktorze. | Niski |

---

## 1. Krok 1 – Outbox dla `UserRegisteredEvent` (Krytyczny)

### Problem

`AuthService.RegisterAsync` publikuje event bezpośrednio przez `IIntegrationEventPublisher.Publish(...)`.
Jeśli publish rzuci wyjątek po `userManager.CreateAsync`, obecny kod próbuje usunąć użytkownika – ale ta operacja też może się nie powieść, co prowadzi do orphaned user bez eventu.
Brakuje gwarancji at-least-once delivery.

### Rozwiązanie

Użyć wzorca **Outbox** z `Common.Outbox`.
Zamiast bezpośredniego publish, event jest zapisywany do tabeli Outbox w tej samej transakcji co utworzenie użytkownika.
Osobny background job odczytuje Outbox i wysyła eventy.

### Kroki implementacji

1. **Dodaj `Common.Outbox` jako referencję do `Auth.Core.csproj`** (jeśli jeszcze nie ma).

2. **Zarejestruj Outbox w `AuthModuleInitializator.AddAuthModule`**:
   ```csharp
   services.AddOutbox<AuthDbContext>();
   ```

3. **Zamień bezpośrednią publikację w `AuthService.RegisterAsync`** na zapis do Outbox:
   ```csharp
   // PRZED
   await integrationEventPublisher.Publish(userRegisteredEvent, ct);

   // PO
   await outboxPublisher.PublishAsync(userRegisteredEvent, ct);  // w tej samej transakcji
   ```

4. **Usuń cały blok `try/catch` z `RegisterAsync`** — Outbox eliminuje potrzebę rollbacku przez delete.
   Jeśli `userManager.CreateAsync` się uda, a Outbox write się nie uda, transakcja jest rollbackowana atomowo przez EF.

5. **Dodaj migrację** dla tabeli Outbox w schemacie `Auth`.

### Wynik

`RegisterAsync` staje się:
```csharp
var user = new User { ... };
await userManager.CreateAsync(user, request.Password);
await userManager.AddToRoleAsync(user, AuthRoles.User);
await outboxPublisher.PublishAsync(new UserRegisteredEvent(...), ct);

if (_requireEmailConfirmed)
    return AuthRegisterResult.PendingEmailConfirmation();

await signInManager.SignInAsync(user, isPersistent: false);
return AuthRegisterResult.Success();
```

---

## 2. Krok 2 – Wprowadzenie CQRS command handlerów (Wysoki)

### Problem

`IAuthService` z 8 metodami narusza zasadę Single Responsibility i jest niespójny z resztą projektu.

### Decyzja architektoniczna

Moduł Auth jest szczególny: `AccountController` to kontroler **Razor MVC**, a nie API.
Wzorzec `ICommandHandlerExecutor` / `IQueryHandlerExecutor` jest stosowany dla API kontrolerów.
Dla Razor MVC (gdzie zarządzamy ModelState i przekierowaniami) pośrednia warstwa IAuthService jest uzasadniona.

**Dlatego:**
- NIE wprowadzamy `ICommandHandlerExecutor` do `AccountController` (Razor MVC).
- **Wprowadzamy** handlery CQRS jako wewnętrzną implementację wewnątrz Auth.Core.
- `IAuthService` staje się cienką fasadą, która orchestruje handlery.

### Alternatywna droga (pełna)

Jeśli w przyszłości Auth będzie miał **API endpoints** (nie tylko Razor UI), wtedy:
1. Handler komendy powinien być wywoływany bezpośrednio przez `ICommandHandlerExecutor`.
2. `AccountController` może pozostać z fasadą `IAuthService` dla Razor flow.

### Kroki implementacji

1. **Utwórz strukturę komend w `Auth.Core`** (lub przyszłym `Auth.Application`):

```
Auth.Core/
  Commands/
    RegisterUserCommand.cs         + RegisterUserCommandHandler.cs
    ConfirmEmailCommand.cs         + ConfirmEmailCommandHandler.cs
    RequestPasswordResetCommand.cs + RequestPasswordResetCommandHandler.cs
    ResetPasswordCommand.cs        + ResetPasswordCommandHandler.cs
    ChangePasswordCommand.cs       + ChangePasswordCommandHandler.cs
  Queries/
    RequiresPasswordChangeQuery.cs + RequiresPasswordChangeQueryHandler.cs
```

2. **Przenieś logikę z `AuthService` do handlerów** (1:1 per metoda).

3. **`AuthService` staje się fasadą orchestrującą handlery** (lub jest usuwany, jeśli AccountController wywoła handlery bezpośrednio):

```csharp
// Opcja A – AuthService jako fasada (rekomendowana dla zachowania MVC flow)
public async Task<AuthRegisterResult> RegisterAsync(AuthRegisterRequest request, CancellationToken ct)
{
    return await registerUserCommandHandler.Handle(
        new RegisterUserCommand(request.Name, request.LastName, request.Email, request.Password), ct);
}
```

4. **Zarejestruj handlery przez konwencję** (Scrutor scan) lub ręcznie w `AuthModuleInitializator`.

### Korzyści

- Każdy handler jest jednostką testowalną z NSubstitute (bez pełnego ServiceProvider).
- Zgodność z wzorcem reszty projektu.
- Ułatwia przyszłe API endpoints w Auth.

---

## 3. Krok 3 – Wyjątki dla guard conditions (Wysoki)

### Problem

`AuthChangePasswordResult.CurrentUserNotFoundResult()` i `AuthChangePasswordResult.PasswordChangeNotRequiredResult()` są guard conditions – nie są oczekiwanym UI state'em, tylko błędem logicznym.

### Rozwiązanie

Zastąpić statusy wyspecjalizowanymi wyjątkami:

```csharp
// W ChangePasswordCommandHandler (lub AuthService)

var user = await userManager.GetUserAsync(principal)
    ?? throw new InvalidOperationException("Current user was not found during password change.");

if (!user.MustChangePassword)
    throw new DomainException("Password change is not required for this account.");
```

`AccountController` nie powinien obsługiwać tych statusów – powinien je obsłużyć middleware (zwracając 500 lub przekierowując na login).

### Zmiany w `AccountController.ChangePasswordRequired`

```csharp
// PRZED
if (result.Status == AuthChangePasswordStatus.CurrentUserNotFound)
    return RedirectToAction(nameof(Login), new { returnUrl = model.ReturnUrl });

// PO – zniknie, bo wyjątek jest rzucany w handlerze
```

### Statusy, które POZOSTAJĄ jako Result (uzasadnione dla UI)

| Status | Powód zachowania |
|---|---|
| `InvalidCredentials` | Celowe ukrycie informacji czy email istnieje |
| `LockedOut` | Komunikat UI do użytkownika |
| `RequiresEmailConfirmation` | Przekierowanie na inną stronę |
| `RequiresPasswordChange` | Przekierowanie na forced change |
| `DuplicateEmail` | Błąd formularza na konkretnym polu |
| `PendingEmailConfirmation` | Przekierowanie na confirmation notice |

---

## 4. Krok 4 – Projekt `Auth.Shared` (Średni)

### Problem

Brak publicznego kontraktu modułu Auth. Jeśli inny moduł będzie potrzebował informacji o użytkowniku (np. weryfikacji roli, EmailAddress), musi albo tworzyć bezpośrednią referencję do `Auth.Core`, albo powielać kod.

### Rozwiązanie

Utwórz projekt `Auth.Shared` i przenieś do niego typy, które mogą być potrzebne innym modułom.

### Kandydaci na `Auth.Shared`

```
Auth.Shared/
  AuthId.cs          – jeśli AuthId jest używany przez inne moduły (np. jako klucz w Customers)
```

> Uwaga: `AuthId` jest już w `Common.StronglyTypedIds`. Sprawdź czy przeniesienie jest uzasadnione.

Inne kandydaty tylko gdy pojawi się konkretna potrzeba:
- `IAuthUserReader` – interfejs do odczytu podstawowych danych o użytkowniku (jeśli moduł X musi zweryfikować, że userId istnieje w Auth).

**Zasada:** Nie tworzyć `Auth.Shared` jako pustego projektu "na zapas". Dodaj do niego tylko typy, których konkretny inny moduł **aktualnie potrzebuje**.

---

## 5. Krok 5 – Refaktor `OpenIddictController` (Średni)

### Problem

`OpenIddictController` bezpośrednio wstrzykuje `UserManager<User>` i `SignInManager<User>`.
Choć kontroler jest relatywnie cienki, operacje na użytkowniku mogą trafić do serwisu.

### Rozwiązanie

Wydziel `IAuthSessionService` (lub podobnie):

```csharp
public interface IAuthSessionService
{
    Task<User?> GetUserFromPrincipalAsync(ClaimsPrincipal principal);
    Task<bool> CanSignInAsync(User user);
    Task SignOutAsync();
}
```

`OpenIddictController` wstrzykuje `IAuthSessionService` zamiast `UserManager` i `SignInManager`.

### Priorytet

Niższy niż #1–#3. Obecna wersja jest funkcjonalna. Refaktor warto przeprowadzić **przy okazji** pracy w tym kontrolerze lub w ramach CQRS refaktoru.

---

## 6. Krok 6 – Testy jednostkowe warstwy aplikacyjnej (Średni)

### Problem

`AdminInitializerTest.cs` buduje pełny `ServiceProvider` z Identity + EF Core – jest to test integracyjny udający unit test.
`AuthService` nie ma żadnych unit testów.

### Rozwiązanie

Po refaktorze CQRS (krok 2) każdy handler będzie łatwo testowalny:

```csharp
// Przykład unit testu po refaktorze
public class RegisterUserCommandHandlerTest
{
    private readonly IUserRepository _userRepository = Substitute.For<IUserRepository>();
    private readonly IIntegrationEventPublisher _publisher = Substitute.For<IIntegrationEventPublisher>();

    [Fact]
    public async Task WhenEmailAlreadyExists_ShouldReturnDuplicateEmail()
    {
        // Arrange
        _userRepository.ExistsByEmailAsync("test@test.com").Returns(true);
        var handler = new RegisterUserCommandHandler(_userRepository, _publisher, ...);

        // Act
        var result = await handler.Handle(new RegisterUserCommand("test@test.com", ...), CancellationToken.None);

        // Assert
        result.Status.ShouldBe(AuthRegisterStatus.DuplicateEmail);
        await _publisher.DidNotReceive().Publish(Arg.Any<UserRegisteredEvent>(), Arg.Any<CancellationToken>());
    }
}
```

Istniejący `AdminInitializerTest.cs` należy zrefaktorować: wydzielić interfejs dla Identity i mockować go przez NSubstitute zamiast budować ServiceProvider.

---

## 7. Kolejność wdrożenia (sugerowana)

```
Krok 1 – Outbox dla UserRegisteredEvent          ← zaczynamy, największe ryzyko biznesowe
Krok 3 – Wyjątki dla guard conditions            ← proste, mały zakres zmian
Krok 2 – CQRS command handlery                   ← większy refaktor, po kroce 1
Krok 6 – Unit testy dla nowych handlerów         ← przy okazji kroku 2
Krok 4 – Auth.Shared                             ← gdy pojawi się konkretna potrzeba
Krok 5 – Refaktor OpenIddictController           ← przy okazji pracy w tym kontrolerze
```

---

## 8. Co NIE wymaga zmiany

| Element | Uzasadnienie |
|---|---|
| `AccountController` wywoływanie `IAuthService` | Razor MVC – fasada jest uzasadniona; thin controller rule spełniona dla MVC |
| `AuthModuleInitializator` – walidacja konfiguracji | Wzorcowa implementacja fail-fast |
| `AuthRolesInitializer`, `OpenIddictApplicationsInitializer`, `AdminInitializer` | Poprawny wzorzec inicjalizatorów modułu |
| `OidcClaimsPrincipalFactory` | Poprawna delegacja poza kontroler |
| `AuthOptions`, `SeedAdminOptions` | Spójne z konwencją konfiguracji |
| Testy E2E w `E2E_Auth/` | Pokrywają flow poprawnie (z zastrzeżeniem: stopniowo wyrównywać do `Tests.md`) |
| Result pattern dla `AuthLoginResult`, `AuthRegisterResult` | Uzasadniony dla Razor MVC UI; patrz krok 3 co zostaje |

---

## 9. Definicja ukończenia (DoD)

- [ ] `UserRegisteredEvent` idzie przez Outbox (at-least-once delivery)
- [ ] `try/catch/delete` w `RegisterAsync` usunięty
- [ ] `CurrentUserNotFound` i `PasswordChangeNotRequired` rzucają wyjątki
- [ ] Logika biznesowa Auth podzielona na handlery CQRS (w warstwie wewnętrznej)
- [ ] Unit testy dla każdego nowego handlera (NSubstitute, bez ServiceProvider)
- [ ] E2E testy przechodzą bez zmian (kontrakt zewnętrzny nie zmieniony)
