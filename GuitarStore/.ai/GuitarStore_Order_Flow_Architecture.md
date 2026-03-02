# GuitarStore -- Order Flow Architecture (Deep Analysis / Production Playbook)

**Generated:** 2026-02-28T21:36:48.401854 UTC\
**Context:** Modular monolith (.NET), single MSSQL DB (schemas per
module), RabbitMQ integration events, Stripe Checkout + Webhooks.\
**Goal:** Provide a *complete* description of the order flow, including
edge cases, retry semantics, failure modes, and recommended hardening
steps.

------------------------------------------------------------------------

## 0. What is true today (as observed from code + your clarification)

### 0.1 Place Order (POST /orders) current behavior

-   `PlaceOrderCommandHandler`:
    1.  Reads `CheckoutCart` from Customers (sync, in-process).
    2.  Validates delivery address.
    3.  Creates `Order` (currently `New`).
    4.  Reserves products in Warehouse (sync, in-process) with TTL
        (currently hardcoded 10 min).
    5.  Creates Stripe Checkout Session via Payments (sync, in-process).
    6.  Persists `Order` and commits.
-   You clarified: **the whole handler runs in a single DB transaction**
    (single MSSQL), so if Stripe session creation fails, **order and
    reservations are not persisted**.

### 0.2 Payments webhook current behavior

-   ApiGateway controller reads raw body + signature, sends a command to
    Payments.
-   Payments handler:
    -   verifies Stripe signature,
    -   checks TTL window (discard old webhooks),
    -   idempotency (dedupe by Stripe `eventId`),
    -   parses `PaymentIntent` + `orderId` from metadata (now
        `TryParse`),
    -   publishes RabbitMQ integration event(s),
    -   marks webhook processing record Completed/Failed.

### 0.3 Consumers (Orders/Warehouse) current behavior

-   Orders consumes `OrderPaidEvent` and marks order paid (idempotent
    after your change).
-   Warehouse has reservation confirm/release APIs already implemented
    but not yet fully wired from events.

### 0.4 Important architectural consequences

-   Because everything is a single MSSQL DB, you *can* use a DB
    transaction for atomicity *inside one request*.
-   But **RabbitMQ publishing is outside the DB** unless you implement
    an **Outbox** pattern.
-   Stripe webhooks can be duplicated/out-of-order; your idempotency +
    idempotent domain methods are essential.

------------------------------------------------------------------------

## 1. The "New vs PendingPayment" question (and best-practice answer)

### 1.1 Your observation is correct

If you do: - create order as `New` - then (within the same transaction)
reserve + create Stripe session - then set `PendingPayment` - then
commit

...then **persisted** state will be **only `PendingPayment`**. The user
will never see `New`, because `New` never gets committed.

### 1.2 Should both statuses exist?

It depends on what you want the model to express.

#### Option A (recommended for your current synchronous + transactional flow): **Remove `New`**

If placing an order always includes stock reservation + starting
checkout *before commit*, then the first persisted state is
effectively: - "reserved and waiting for payment"

In that case, `New` is redundant. Use only: - `PendingPayment` as the
first committed status.

#### Option B: Keep `New` but change semantics so it can be persisted

`New` becomes meaningful if **an order can exist before
reservation/payment starts**, e.g.: - user creates an order draft, -
order is persisted first, - reservation/payment happen later (async or
in a subsequent step).

This is common if you: - implement a saga, - separate "place order" and
"start checkout", - or support "Pay later" variants.

#### Option C: Keep both but treat `New` as an *in-memory transient step*

This is mostly for internal code readability, not as a persisted
lifecycle. It's valid, but then: - do not document `New` as a real state
visible to users/ops, - or rename it to something like
`CreatedTransient` (usually not worth it).

### 1.3 What I recommend for GuitarStore *right now*

Given your current code, **Option A** is the cleanest: - Persist first
state as `PendingPayment`. - Remove `New` from the lifecycle exposed to
users (you can keep it if you want, but treat as internal only).

If later you move toward a more decoupled process (Outbox + async
reservation), reintroduce `New` as a real persisted step.

------------------------------------------------------------------------

## 2. Canonical target lifecycle (recommended)

### 2.1 Order statuses (final recommended semantics)

  -----------------------------------------------------------------------
  Status                  Meaning                 Terminal?
  ----------------------- ----------------------- -----------------------
  PendingPayment          Stock reserved (soft    No
                          reservation w/ TTL),    
                          waiting for payment     
                          confirmation            

  Paid                    Payment confirmed via   No
                          webhook                 

  Sent                    Fulfillment started /   No
                          shipped                 

  Realized                Delivered               Yes

  Canceled                User/admin cancellation Yes
                          before shipping         

  Expired                 Reservation TTL         Yes
                          exceeded without        
                          successful payment      
  -----------------------------------------------------------------------

> If you insist on keeping `New`, define it as "order persisted but
> reservation not done yet" and make it observable by splitting the
> process (see section 6).

### 2.2 Reservation statuses in Warehouse (recommended)

-   `Active` (reserved, TTL running)
-   `Confirmed` (payment confirmed, reservation becomes final)
-   `Released` (released due to cancel/expire)
-   (Optional) `Expired` (if you want explicit state instead of
    Released+reason)

------------------------------------------------------------------------

## 3. Recommended event taxonomy (avoid semantic confusion)

Right now you publish `OrderCancelledEvent` for
`payment_failed`/`canceled` Stripe events. This is usually **not
correct** semantically.

### 3.1 Separate "payment outcome" from "order decision"

Recommended integration events:

**From Payments:** -
`OrderPaymentSucceeded(orderId, paymentIntentId, occurredAtUtc)` -
`OrderPaymentFailed(orderId, paymentIntentId, failureCode?, occurredAtUtc)`
*(non-terminal)* -
`OrderPaymentCanceled(orderId, paymentIntentId, occurredAtUtc)`
*(non-terminal; depends on Stripe flow)*

**From Orders (business decisions):** -
`OrderCanceled(orderId, reason, occurredAtUtc)` *(terminal)* -
`OrderExpired(orderId, occurredAtUtc)` *(terminal)* -
`OrderReadyForFulfillment(orderId, occurredAtUtc)` *(optional; after
Paid)*

### 3.2 Why this matters

-   A payment failure can often be retried successfully within minutes.
-   If you publish `OrderCancelled` on payment_failed, you:
    -   prematurely terminate orders,
    -   create out-of-order conflicts (Paid after Cancel),
    -   and risk poison messages unless all handlers become "no-op
        tolerant".

------------------------------------------------------------------------

## 4. Stripe realities you must design for

### 4.1 Webhooks are *at least once*

Stripe can resend the same event multiple times. You handled this
with: - webhook idempotency store keyed by Stripe `eventId`, -
idempotent consumers (`MarkPaid` no-op when already paid).

### 4.2 Events can be out-of-order

You must assume `payment_failed` could arrive before `succeeded` in
pathological cases, or consumers process them in different order.

**Rule of thumb:** Consumers must be safe for: - duplicates, -
reordering, - late arrival.

### 4.3 Checkout Session behavior (practical guidance)

-   Checkout sessions expire after some time window (varies /
    configurable).
-   PaymentIntent final status is authoritative.
-   Your system should allow retry within reservation TTL when sensible.

------------------------------------------------------------------------

## 5. Failure-mode matrix (what should happen)

### 5.1 PlaceOrder request failures

  ---------------------------------------------------------------------------
  Failure           Where             Effect            Recommended response
  ----------------- ----------------- ----------------- ---------------------
  Missing delivery  Orders            No changes        400
  address                                               (Domain/Validation)

  Stock             Warehouse         No changes        409 (DomainException)
  insufficient                        (transaction      
                                      rollback)         

  Stripe session    Payments/Stripe   No changes        502/500 -\> return
  creation fails                      (rollback)        error to client

  DB error          Any               No changes        500
  ---------------------------------------------------------------------------

> Because it's one DB and a single transaction, you're safe here today.

### 5.2 Webhook processing failures

  -----------------------------------------------------------------------
  Failure           Effect            Stripe retry?     Your response
  ----------------- ----------------- ----------------- -----------------
  Missing/invalid   Security          No (should not)   400
  signature         rejection                           

  Event too old     Ignore            No                200 (no-op)
  (TTL policy)                                          

  Duplicate event   Ignore            No                200 (no-op)

  DB down           Cannot dedupe     Yes               500
  (idempotency                                          
  store)                                                

  Rabbit publish    Payment event not Yes               500 (and mark
  fails             propagated                          failed)

  Parser cannot     Ignore / log      No                200 (no-op)
  extract orderId                                       
  -----------------------------------------------------------------------

**Key idea:** only return 500 when you genuinely want Stripe to retry.

------------------------------------------------------------------------

## 6. Hardening step #1: Outbox pattern (high-impact)

### 6.1 Problem

Today: Payments handler publishes to RabbitMQ and then marks webhook
record Completed. If Rabbit publish succeeds but DB commit fails (or
vice versa), you can get inconsistencies.

### 6.2 Solution: transactional Outbox (recommended)

-   In the same DB transaction where you write webhook processing state,
    also write an **OutboxMessage** record.
-   A background dispatcher publishes Outbox messages to Rabbit and
    marks them as sent.

**Benefits:** - Exactly-once effect from at-least-once delivery. - No
lost events if Rabbit is down.

**Minimal model:** -
`OutboxMessages(id, occurredAtUtc, type, payloadJson, processedAtUtc, attempts, lastError)`

### 6.3 Where to apply outbox

-   At minimum: in Payments (webhook → integration events)
-   Later: in Orders (business events like OrderExpired/Canceled)

------------------------------------------------------------------------

## 7. Hardening step #2: optimistic concurrency for JSON snapshots

### 7.1 Current risk

Orders and Cart are stored as JSON snapshots. Without `RowVersion`,
concurrent updates are last-write-wins.

Example: - `OrderPaidEventHandler` loads order and sets Paid. - Almost
simultaneously `OrderExpiredJob` loads order and sets Expired. - Both
write JSON; last commit wins. You can end up with Paid overwritten by
Expired (or vice versa).

### 7.2 Fix (recommended)

Add `rowversion` column and configure EF optimistic concurrency. On
`DbUpdateConcurrencyException`, decide: - retry, - or re-load and
reapply domain operation, - or dead-letter the message if conflict
persists.

This is critical once you have multiple asynchronous consumers/jobs.

------------------------------------------------------------------------

## 8. Reservation expiration: the missing "clock-driven" part

You confirmed there is a job concept (from prior discussion), but ensure
you implement it explicitly.

### 8.1 Recommended expiration job responsibilities

1.  Find reservations with `Status=Active` and `ExpiresAtUtc <= now` (or
    `Created+TTL`).
2.  For each affected order:
    -   publish `OrderExpired` (business decision by Orders), OR
    -   publish `ReservationExpired` and let Orders decide.
3.  Release stock in Warehouse.
4.  Mark order status to Expired.

### 8.2 Ordering choice (who decides Expired?)

Two good designs:

**Design A (Orders is the decider):** - Job emits
`ReservationExpired(orderId)` - Orders consumes and decides to set
`Expired`, then emits `OrderExpired(orderId)` - Warehouse consumes
`OrderExpired` and releases reservations

**Design B (Warehouse is the decider, Orders reacts):** - Warehouse job
sets reservations released and emits `OrderExpired(orderId)` - Orders
consumes and sets `Expired`

Given you want Orders to own order lifecycle, **Design A** is cleaner.

------------------------------------------------------------------------

## 9. Recommended end-to-end flows (diagrams)

### 9.1 PlaceOrder (recommended, status starts as PendingPayment)

``` mermaid
sequenceDiagram
    participant U as User
    participant O as Orders
    participant C as Customers (Cart)
    participant W as Warehouse
    participant P as Payments
    participant S as Stripe

    U->>O: POST /orders (PlaceOrder)
    O->>C: GetCheckoutCart(customerId)
    O->>W: ReserveProducts(orderId, items, ttl)
    O->>P: CreateCheckoutSession(orderId, items)
    P->>S: Create Checkout Session (metadata: orderId)
    S-->>P: sessionUrl + sessionId
    O->>O: Set status PendingPayment
    O-->>U: 200 OK (sessionUrl)
```

### 9.2 Webhook success → events → finalization

``` mermaid
sequenceDiagram
    participant S as Stripe
    participant G as ApiGateway
    participant P as Payments
    participant DB as MSSQL
    participant R as RabbitMQ
    participant O as Orders
    participant W as Warehouse

    S->>G: POST /webhook (signed)
    G->>P: StripeWebhookCommand(json, sig)
    P->>P: verify signature + TTL + idempotency
    P->>DB: store processed webhook + outbox (recommended)
    P->>R: publish OrderPaymentSucceeded (or via outbox dispatcher)
    R->>O: consume OrderPaymentSucceeded
    O->>DB: MarkPaid (idempotent) + save
    R->>W: consume OrderPaymentSucceeded
    W->>DB: ConfirmReservation
```

### 9.3 Payment failure (non-terminal)

``` mermaid
sequenceDiagram
    participant S as Stripe
    participant P as Payments
    participant R as RabbitMQ
    participant O as Orders

    S->>P: webhook payment_failed
    P->>R: publish OrderPaymentFailed(orderId)
    R->>O: consume OrderPaymentFailed
    O->>O: keep PendingPayment (optional log / store failure info)
```

### 9.4 Expiration (clock-driven)

``` mermaid
sequenceDiagram
    participant J as Expiration Job
    participant W as Warehouse
    participant R as RabbitMQ
    participant O as Orders

    J->>W: scan Active reservations past TTL
    W->>R: publish ReservationExpired(orderId)
    R->>O: consume ReservationExpired
    O->>O: set status Expired
    O->>R: publish OrderExpired(orderId)
    R->>W: consume OrderExpired
    W->>W: ReleaseReservations(orderId) (restore stock)
```

------------------------------------------------------------------------

## 10. Concrete implementation recommendations (next steps)

### 10.1 Orders: make first committed state `PendingPayment`

-   In `PlaceOrderCommandHandler`, set status to `PendingPayment` before
    saving.
-   If you keep `New`, make it meaningful by splitting persistence (see
    10.4).

### 10.2 Payments: change emitted events

-   Replace `OrderCancelledEvent` from payment_failed with
    `OrderPaymentFailedEvent`.
-   Emit `OrderPaymentSucceededEvent` on success.

### 10.3 Warehouse: wire confirmations/releases

-   On payment success event: `ConfirmReservations(orderId)`
-   On order expiration/cancel event: `ReleaseReservations(orderId)`

### 10.4 (Optional) If you want `New` to matter

Split PlaceOrder into 2 phases: 1. Create order `New` and persist. 2.
Reserve + create session; move to `PendingPayment`. This gives you: -
observable `New`, - ability to retry reserve/session without losing
order, but costs: - more compensations, - more failure modes, - requires
clear UX semantics.

### 10.5 Introduce Outbox (strongly recommended)

-   Payments outbox for webhook -\> Rabbit publication.
-   Later: Orders outbox for business events.

### 10.6 Add RowVersion to snapshot tables (strongly recommended)

-   Prevent last-write-wins across async handlers/jobs.

------------------------------------------------------------------------

## 11. Quick decision guide (what to choose)

### Keep vs remove `New`

-   If PlaceOrder does everything before commit → **remove `New`**.
-   If you plan multi-step checkout / retryable flows → **keep `New`**
    and persist it.

### Payment failure mapping

-   If you want retries → **do not cancel order on payment_failed**.
-   Cancel only on explicit user action or TTL expiration.

------------------------------------------------------------------------

## 12. AI-optimized structured summary

**PlaceOrder** - Input: customerId, delivery address (optional) - Steps:
read cart → create order → reserve stock (TTL) → create Stripe session →
set PendingPayment → persist - Output: sessionUrl, sessionId

**Webhook** - Input: (rawJson, Stripe-Signature) - Steps: verify
signature → TTL gate → dedupe by eventId → parse (paymentIntentId,
orderId) → publish integration event → mark processed - Output: 200 on
ignore/duplicate/expired; 400 on signature; 500 on infra failure

**Events** - PaymentSucceeded → Orders MarkPaid; Warehouse
ConfirmReservations - PaymentFailed → Orders keep PendingPayment
(optionally store failure); Warehouse no-op - ReservationExpired →
Orders Expired; Orders emits OrderExpired; Warehouse releases stock

------------------------------------------------------------------------

# End of deep analysis document
