# Order Sync Failure Tracking
Local API: http://localhost:5080. Dashboard: http://localhost:3000.
Fictional data only. No authentication; bind to loopback for local use.

- GET /api/orders: list orders; optional status and search.
- GET /api/orders/{id}: one order, its attempts and its trace; 404 when absent.
- POST /api/orders: receive and save a validated order; 201. Required orderNumber, customerName, product, quantity (1-10000), channel. Unique orderNumber; duplicate 409. Invalid body 400.
- PUT /api/orders/{id}: change fields on Received or Failed orders only. Same validation; completed/sent orders return 409.
- DELETE /api/orders/{id}: remove a local order and its child records; 204 or 404. Sent orders return 409.
- POST /api/orders/{id}/sync: send Received order; completed/failed orders return 409.
- POST /api/orders/{id}/retry: only Failed orders; other statuses return 409.
- Sync/retry body: scenario = success, unavailable, timeout, or invalid-response. Local receiver scenarios are explicit; they are not real warehouse responses.
- POST /api/orders/{id}/sync-live: send a Received order to Postman Echo. Fictional payload only; Echo verifies transport and does not fulfill orders.
- GET /api/analytics/orders: one row per order. Completion rate uses all orders; failure rate uses Failed / all orders. Recovery rate uses completed previously failed orders / previously failed orders.
- GET /api/analytics/export: download a CSV snapshot for Power BI.
- GET /health: service readiness.

Trace: Received -> Validated -> Saved -> Sent -> Completed or Failed.
Each send stores an attempt, response or error, UTC timestamp, duration and outcome.
A single in-process write gate prevents duplicate concurrent sends. This is a local single-instance app; multi-server delivery and provider-side idempotency are future work.
Retry never deletes earlier failure history. Recovery time starts at the first failed attempt and ends at completion.
