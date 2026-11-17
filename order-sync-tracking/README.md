# Order Sync Failure Tracking

An order can be saved correctly but fail while moving to another system. This project gives an operations employee one place to find that failure, see its cause, and retry it.

## Start on this PC

Double-click **Start Project.cmd**. Open http://127.0.0.1:3000.
The API explorer is http://localhost:5080/swagger.

## Setup on another PC

Install .NET 10 SDK, Node.js LTS, Postman CLI, and Power BI Desktop.

1. In `backend`, run `dotnet restore`.
2. In `dashboard`, run `npm install`, then `npm run build`.
3. From the project folder, run `postman simulation run postman/simulations/order-sync.sim.yaml --port 4010 --no-history`.
4. In another terminal, enter the `backend` folder and run `dotnet run --no-launch-profile`.
5. In `dashboard`, run `npm start`.
6. Open http://127.0.0.1:3000.

The API applies its checked-in Entity Framework migration at startup. SQLite stores orders, attempts, and trace events. The first launch adds 42 fictional historical records, identified in DataOrigin. Interactive orders are also fictional. No real customer data or credentials are included.

## Order flow

Received -> Validated -> Saved -> Sent -> Completed or Failed.

Create an order, choose the receiver outcome, and press Send. To show recovery, choose Unavailable, send the order, then choose Confirm order and press Retry. Open the order number to see its trace and all earlier attempts.

## Power BI

Open `powerbi/Order Sync.pbix` for the saved report and data. Open `powerbi/Order Sync.pbip` to work with the report source folders. Both have an Order performance page and a Failure review page.
Run **Refresh Power BI Data.cmd**, then click **Refresh** in Power BI.
On another PC, set the Power Query parameter **DataFile** to that PC's `powerbi/data/orders.csv`.
The CSV is a snapshot, so a dashboard change appears in Power BI only after both refresh steps.

Measures count one row per order:
- Completion rate = completed orders / all orders.
- Open failure rate = currently failed orders / all orders.
- Recovery rate = completed orders with earlier failures / orders that ever failed.
- Retry attempts = all attempts after the first attempt.
- Average recovery minutes uses only recovered orders, from first failure to completion.

## Code map

- `backend/Program.cs`: connect services and start the API.
- `backend/Models/Order.cs`: order fields, inputs, attempts, and trace events.
- `backend/Controllers/OrdersController.cs`: create, read, update, delete, send, and retry.
- `backend/Services/SyncService.cs`: call the receiver and save its result.
- `backend/Controllers/AnalyticsController.cs`: one-row-per-order data and CSV export.
- `backend/Data/OrderDb.cs`: tables and relationships.
- `backend/Data/SeedOrders.cs`: clearly identified fictional history.
- `dashboard/app/page.js`: order queue, filters, order form, Send and Retry.
- `dashboard/app/orders/[id]/page.js`: saved errors, responses, and trace.
- `powerbi`: editable report, model, DAX measures, and data snapshot.

## Verification

Run `python scripts/verify.py` while the API and local receiver are running.
Run `postman collection run postman/collections/order-sync.postman_collection.json`.
These checks exercise the actual API, SQLite, and an HTTP receiver.
`docs/live-echo-result.json` records a separate successful transfer to the public Postman Echo API. Echo confirms JSON transport; it does not fulfill orders.

## Scope and next changes

This is a local, single-instance application. The receiver supplies controlled HTTP responses; it is not a warehouse. Only fictional payloads should use the public Echo action.

Future changes: real warehouse contract and credentials, user login, provider-side idempotency, a background retry queue, multi-instance locking, scheduled Power BI refresh, and an explicit review step for timeouts with uncertain receiver outcomes.

See `docs/Monday checklist.md` for the project plan and verification record. The project owner confirmed on November 17, 2026 that another person tested the project.

## Walkthrough files

- `output/pdf/Order Sync - How It Works.pdf`: step-by-step guide and speaking order.
- `output/video/Order Sync Walkthrough.mp4`: a 42-second walkthrough assembled from actual application captures.
- `docs/screenshots`: original captures of the order flow, Power BI, and November task.
- `docs/analytics-results.json`: database/export totals verified against Power BI.
- `docs/persistence-results.json`: connection error and restart checks.

The saved report contains 45 fictional orders: 42 initial records and three interactive transfers. A fresh database starts with 42 records; exporting and refreshing updates the report to that database's current totals.
