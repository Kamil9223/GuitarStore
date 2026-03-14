# GuitarStore (Backend) - PRD / Roadmap

> Product and technical direction document for an educational e-commerce backend. The scope is intentionally focused on a working end-to-end purchase flow plus architecture, reliability, and security concerns.

## 1. Product Goal

GuitarStore is an e-commerce backend for a guitar store, similar in business shape to stores such as Guitar Center.

The goals of the project are:
- deliver a **minimal working purchase flow** from catalog browsing to payment and order completion,
- practice **architecture and system design** in a modular monolith built on .NET 8,
- evolve the solution toward **DDD, event-driven integration, observability, security, and reliability**.

The system is educational in purpose, but it should still be deployable to a test environment and treated as a serious engineering project.

## 2. Product Scope and Current Direction

GuitarStore is a backend-only system organized as a modular monolith.

The current modules are:
- `Catalog`
- `Customers`
- `Orders`
- `Payments`
- `Warehouse`
- `Delivery`
- `Auth`
- `ApiGateway`
- `Common`

The current technical direction is:
- `.NET 8`
- `EF Core`
- `MSSQL` with one database and separate schemas per module
- `RabbitMQ` for asynchronous integration between modules
- `Stripe` for payments
- `ASP.NET Core Identity + OpenIddict` for authentication and authorization
- end-to-end testing with `TestContainers`

## 3. Key Personas

- **Anonymous customer**
  Browses the product catalog and explores the offer.
- **Authenticated customer**
  Manages a cart, performs checkout, pays for an order, tracks order status, and accesses only their own data.
- **Admin / operator**
  Manages the catalog and, in later stages, supports operational flows such as stock or order handling.

## 4. MVP Scope

The MVP is defined as a minimal purchase flow that works end-to-end.

### 4.1 User Stories

1. As a customer, I can browse a list of guitars and product details.
2. As a customer, I can add products to my cart and change quantities.
3. As a customer, I can prepare checkout by providing delivery information and selecting a delivery option.
4. As a customer, I can pay through Stripe and receive confirmation that the order has been accepted.
5. As a system, I reserve stock during payment to prevent overselling.
6. As a customer, I can view the status and history of my own orders.
7. As a system, I handle failure cases such as payment failure, abandoned payment, reservation expiration, and repeated webhook delivery.

### 4.2 Functional Areas

**Catalog**
- Product listing and product details.
- Product management capabilities for administrative use.
- Product data includes brand, model, type, price, currency, description, and SKU.

**Customers**
- Customer-related data and customer-owned cart.
- Cart management and checkout preparation.

**Orders**
- Order creation from checkout data.
- Order lifecycle tracking.
- Ownership rules so customers only access their own orders.
- Idempotent handling of payment-driven state transitions.

**Warehouse**
- Stock availability management.
- Soft reservations with TTL during payment.
- Reservation confirmation after successful payment.
- Reservation release after cancellation or expiration.

**Payments**
- Stripe checkout/session creation.
- Stripe webhook handling.
- Mapping external payment outcomes into internal events and order state changes.

**Delivery**
- Delivery data persistence.
- Basic delivery-cost and delivery-option support for MVP.
- Fulfillment can remain simple or mocked in the MVP stage.

**Auth / Security**
- Authentication based on `ASP.NET Core Identity + OpenIddict`.
- SPA-oriented authentication flow based on `Authorization Code + PKCE`.
- Server-hosted auth UI in `ApiGateway`.
- Role- and policy-based authorization as the target direction.
- Customers should only access their own orders and customer-owned resources.

## 5. Core Business Rules

### 5.1 Stock Reservation

The central business rule is prevention of overselling.

The intended behavior is:
- when an order is created, products are reserved for a limited time,
- the order enters `PendingPayment`,
- when payment succeeds, the reservation is confirmed,
- when payment fails permanently, the order is cancelled by business decision or remains retryable depending on the flow,
- when payment is not completed before TTL expiry, the order expires and stock is released.

Reservation expiration must be actively enforced by background processing.

### 5.2 Order Lifecycle

The current order lifecycle is:
- `PendingPayment`
- `Paid`
- `Sent`
- `Realized`
- `Canceled`
- `Expired`

Interpretation:
- `PendingPayment`: stock reserved, waiting for payment result
- `Paid`: payment confirmed
- `Sent`: fulfillment or shipment started
- `Realized`: order completed
- `Canceled`: order cancelled before fulfillment
- `Expired`: payment was not completed before reservation TTL expired

### 5.3 Idempotency

Idempotency is required for:
- payment webhooks,
- payment-result event processing,
- repeated or retried message handling,
- commands that may be repeated by clients or infrastructure.

The project direction includes inbox/outbox style reliability where needed.

## 6. Events and Integration Direction

The system uses both in-module and cross-module events.

**Domain events**
- Used inside a module to represent business state changes.

**Integration events**
- Used between modules through RabbitMQ.

The target direction includes:
- payment outcome events,
- warehouse reservation events,
- order lifecycle events,
- reliable message publication with outbox where required.

## 7. Non-Functional Requirements

### Reliability
- Idempotent webhook handling.
- Reliable event publication and consumption.
- Retry policies and dead-letter handling where appropriate.
- Consistent reservation and order state handling.

### Observability
- Correlation across requests and background processing.
- Logs, metrics, and traces suitable for end-to-end flow analysis.

### Security
- OpenIddict- and Identity-based auth flow.
- Role- and policy-based access control.
- Ownership-based access to customer data.
- Protection of sensitive endpoints and webhook verification.

### Testability
- Automated unit and end-to-end tests for core purchase scenarios.
- End-to-end verification for success, failure, and expiration scenarios.

### Performance
- Database indexing for critical entities such as orders, stock reservations, and products.
- Room for later optimization such as read-model tuning or caching.

## 8. Current Architecture Decisions That Matter for the Product

- The system is a modular monolith, not a microservice system.
- Cart functionality currently belongs to the `Customers` module.
- Orders, payments, and warehouse coordination are core to the MVP.
- Auth is no longer treated as a stub; the project direction is a real OIDC/OAuth2-based auth module.
- The order flow is evolving toward stronger reliability guarantees and clearer event semantics.

## 9. Roadmap

### M0 - Structural cleanup
- Keep module boundaries explicit.
- Reduce accidental coupling between modules.
- Keep `Common` focused on cross-cutting concerns.

### M1 - Working purchase MVP
- Complete the flow from cart to payment to order state.
- Make warehouse reservations with TTL deterministic.
- Cover happy path, failure path, and expiration path with E2E tests.

### M2 - Reliability and eventing
- Strengthen event taxonomy and integration semantics.
- Add outbox/inbox patterns where needed.
- Improve consistency around retries and failures.

### M3 - Observability
- Add traces, metrics, structured logs, and correlation.
- Make the purchase flow diagnosable across module boundaries.

### M4 - Authentication and authorization
- Complete the auth module based on Identity and OpenIddict.
- Add hosted login/registration UI.
- Add roles, policies, and ownership enforcement.

### M5 - System design maturity
- ADRs, sequence diagrams, architecture diagrams.
- Load and failure analysis.
- Thoughtful discussion of future decomposition and scaling options.

## 10. Out of Scope for MVP

The following are outside MVP:
- returns and refunds,
- coupons and promotions,
- recommendation systems,
- multi-currency support,
- advanced fulfillment integrations,
- full operational back office,
- production-grade enterprise hardening in every area.

## 11. Definition of Done for the MVP

The MVP is considered done when:
- the purchase flow works end-to-end,
- stock reservation and release behave deterministically,
- payment processing and webhook handling are idempotent,
- order status transitions are correct,
- customers can access their own order information,
- the main scenarios are covered with automated end-to-end tests,
- the system is sufficiently observable for debugging and learning.
