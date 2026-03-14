# GuitarStore - Order Flow Backlog

Data aktualizacji: 2026-03-14
Rola dokumentu:
- backlog otwartych tematow dla order flow,
- uzupelnienie dla `.ai/Orders_Architecture.md`,
- nie jest to juz pelny plan implementacji od zera ani dokument historyczny.

## 1. Source of Truth

Za logike i semantyke flow zamowien za glowny dokument nalezy uznac:
- `.ai/Orders_Architecture.md`

Ten plik zostawia tylko:
- rzeczy nadal otwarte,
- rzeczy czesciowo wdrozone,
- dalsze ulepszenia i backlog techniczny.

## 2. What Is Already Considered Done

Nastepujace obszary traktujemy jako wdrozone na poziomie bazowym:
- outbox w `Payments`
- optimistic concurrency dla `Order`
- TTL z konfiguracji
- poprawiona semantyka eventow platnosci
- `OrderPaymentFailedEvent`
- business `OrderCancelledEvent`
- `OrderExpiredEvent`
- integracja `Warehouse` z eventami `OrderPaid`, `OrderCancelled`, `OrderExpired`
- expiration flow dla zamowien
- user cancellation endpoint i podstawowy flow anulowania
- refactor architektury transakcji do explicit transaction executors

Jesli ktorys z tych obszarow wymaga korekty, powinien trafic do backlogu jako nowy punkt, a nie wracac do formy starego epica wdrozeniowego.

## 3. Open Items - High Priority

### 3.1 API ownership cleanup after Auth integration

Status:
- nadal otwarte

Problem:
- czesc endpointow nadal nie pobiera tozsamosci usera z auth claims,
- w kodzie sa placeholdery typu `CustomerId.New()` albo TODO o odczycie z JWT.

Najwazniejsze miejsca:
- cart endpoints w `GuitarStore.ApiGateway/Modules/Customers/Carts/CartsController.cs`
- cancel/history order endpoints w `GuitarStore.ApiGateway/Modules/Orders/Orders/OrdersController.cs`

Target:
- customer-owned operacje maja korzystac z identity/auth context,
- klient nie przekazuje w request body lub route wartosci, ktora powinna wynikac z zalogowanego usera.

Definition of done:
- [ ] Cart endpoints korzystaja z identity claims zamiast placeholderow
- [ ] Orders cancel/history korzystaja z identity claims
- [ ] Ownership rules sa egzekwowane konsekwentnie na API boundary

### 3.2 Consumer resilience and dead-letter handling

Status:
- czesciowo otwarte

Co juz jest:
- `Payments` outbox ma retry count i dead-letter style cutoff na poziomie dispatchera

Czego nadal brakuje:
- spójnej retry policy dla consumerow integration events,
- jasnego dead-letter / poison-message handling dla RabbitMQ consumers.

Target:
- transient failures sa retryowane,
- poison messages nie blokuja kolejek,
- blad handlera jest dobrze widoczny operacyjnie.

Definition of done:
- [ ] Retry policy dla consumerow jest zdefiniowana i wdrozona
- [ ] Dead-letter path jest skonfigurowany lub jasno obslugiwany
- [ ] Sa testy dla transient failure i poison message scenario

### 3.3 Correlation and observability through the order flow

Status:
- czesciowo otwarte

Co juz jest:
- `IntegrationEvent` ma `CorrelationId`
- publisher przekazuje `CorrelationId` do message properties

Czego nadal brakuje:
- propagacji correlation ID od HTTP request przez command, eventy i webhook flow,
- spójnego structured logging dla order flow,
- metrics/traces dla kluczowych etapow.

Target:
- da sie przesledzic jedno zamowienie przez `Orders`, `Payments`, `Warehouse` i webhooki,
- flow jest diagnosable bez recznego slepienia logow.

Definition of done:
- [ ] Correlation ID jest propagowane end-to-end
- [ ] Kluczowe handlery i joby maja structured logs
- [ ] Sa podstawowe metrics dla order flow
- [ ] Jest plan albo wdrozenie traces / OpenTelemetry

## 4. Open Items - Medium Priority

### 4.1 Health checks

Status:
- otwarte

Target:
- health endpoint dla glownego hosta,
- weryfikacja polaczenia do bazy, RabbitMQ i krytycznych zaleznosci.

Definition of done:
- [ ] `/health` istnieje
- [ ] obejmuje DB i RabbitMQ
- [ ] mozna szybko rozpoznac stan aplikacji i infra

### 4.2 Payment failure persistence on Order

Status:
- otwarte

Problem:
- `OrderPaymentFailedEventHandler` loguje failure, ale nie zapisuje informacji o ostatniej nieudanej probie platnosci.

Target:
- opcjonalne pole lub read model z informacja o ostatnim payment failure,
- lepszy support dla troubleshooting i ewentualnego UX.

Definition of done:
- [ ] decyzja czy to ma byc przechowywane w aggregate czy read modelu
- [ ] implementacja wybranego wariantu

### 4.3 Order completion / fulfillment flow

Status:
- otwarte

Problem:
- `OrderCompletionJob` nadal jest pustym szkieletem,
- lifecycle ma statusy `Sent` i `Realized`, ale flow fulfillment nie jest domkniety.

Target:
- jasno zdecydowac, czy:
  - usuwamy `OrderCompletionJob` jako martwy kod,
  - czy implementujemy rzeczywisty flow `Paid -> Sent -> Realized`.

Definition of done:
- [ ] decyzja: remove albo implement
- [ ] brak pustych jobow z TODO jako pseudoplanem
- [ ] jesli implementacja: statusy `Sent` i `Realized` maja realny trigger

### 4.4 Distributed execution strategy for expiration jobs

Status:
- otwarte

Problem:
- obecne jobs zakladaja pojedyncza instancje hosta,
- w kodzie istnieje TODO dotyczace distributed job mechanism / locking.

Target:
- jesli aplikacja ma byc uruchamiana w wielu instancjach, trzeba uniknac duplikacji pracy jobow.

Definition of done:
- [ ] decyzja architektoniczna dla multi-instance scheduling
- [ ] dokumentacja lub implementacja lock/distributed scheduler

## 5. Open Items - Nice to Have

### 5.1 Extra warehouse events

Opcjonalne eventy:
- `ReservationConfirmedEvent`
- `ReservationReleasedEvent`

Przydatnosc:
- analytics
- audit
- notifications

### 5.2 Notifications

Scenariusze:
- order placed
- payment confirmed
- order expired
- order cancelled
- order shipped

### 5.3 Admin operational flows

Mozliwe rozszerzenia:
- manual order cancellation by admin
- manual expiration / release actions
- audit log dla akcji operatorskich

### 5.4 Fulfillment enhancements

Mozliwe rozszerzenia:
- real integration z `Delivery`
- manual or event-driven `MarkSent`
- manual or event-driven `MarkRealized`

### 5.5 Rate limiting and extra security hardening

Mozliwe rozszerzenia:
- rate limiting dla wrazliwych endpointow,
- dodatkowe zabezpieczenia dla webhookow,
- dalsze policy-based authorization dla endpointow operacyjnych.

## 6. Success Criteria For Remaining Work

### Functional
- [ ] customer-owned endpoints sa powiazane z auth claims
- [ ] order flow ma domknieta obsluge bledow i ownership
- [ ] fulfillment path jest albo wdrozony, albo martwy kod usuniety

### Operational
- [ ] consumer failures nie blokuja systemu
- [ ] correlation i logs pozwalaja sledzic zamowienie end-to-end
- [ ] health checks daja szybki sygnal o stanie aplikacji

## 7. Maintenance Rule For This File

Ten plik powinien zawierac tylko:
- rzeczy otwarte,
- rzeczy czesciowo wdrozone,
- dalszy backlog.

Nie powinien znowu stac sie:
- historycznym dumpem wdrozen,
- duplikatem dokumentu architektonicznego,
- lista rzeczy juz zakonczonych.
