# Project plan

## Users and goals
An operations employee needs to locate failed order transfers and retry them.
An analyst needs to compare failure reasons, channels, and trends without counting retries as new orders.
A reviewer needs to trace what happened to a specific order.

## Required behavior
Save each order once. Validate required fields. Send JSON. Keep each attempt and response.
Make failures visible. Allow a failed order to be retried. Show the order trace.
Provide a dashboard and a Power BI report with clear metric definitions.

## Risks
An unavailable receiver can delay orders. A timeout can leave the outside result uncertain.
A second application instance needs stronger coordination than the local write gate.
A CSV report snapshot can become stale until it is exported and refreshed.
The local receiver and historical fixtures are fictional and do not prove real warehouse delivery.

## Twelve-week sequence
1. Purpose, user stories and flow.
2. C# types, lists and methods.
3. API routes and JSON.
4. Order CRUD and validation.
5. Database and migration.
6. Persistence, search and filtering.
7. Outside API transfer.
8. Error handling.
9. Logging, retry and duplicate prevention.
10. Dashboard and order details.
11. Automated checks and independent usability review.
12. Documentation, walkthrough and release.
November addition: Power BI measures, trends, filters and refresh workflow.
