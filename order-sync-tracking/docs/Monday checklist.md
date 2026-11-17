# Delivery checklist

Source: https://transformnhv.monday.com/boards/18424893351
Reviewed November 17, 2026. Existing Done labels were not treated as proof of implementation.

| Week | Requirement | Evidence |
| --- | --- | --- |
| 1 | Purpose, user stories, workflow, risks and repository | README, API contract, Project plan |
| 2 | C# types, classes, lists and methods | Models, controllers, and services; original console tracker retained elsewhere |
| 3 | ASP.NET API, GET/POST and Swagger | backend and /swagger |
| 4 | CRUD, IDs, dates and validation | OrdersController; acceptance tests |
| 5 | EF Core, SQLite, migration and persistence | Data/OrderDb; Migrations; restart check |
| 6 | Save, search, filter and update orders | API and dashboard filters; acceptance tests |
| 7 | HttpClient, outside JSON API and saved result | SyncService; live-echo-result.json |
| 8 | Connection failures, validation and saved errors | attempt records; timeout, 503 and invalid-response tests |
| 9 | Logging, retry, duplicate prevention and trace | built-in .NET logs; attempts/events; concurrency test |
| 10 | Next.js dashboard, details, filters and retry | dashboard/app |
| 11 | Success/error checks, fixes and independent tester | automated checks complete; independent testing confirmed by the project owner on November 17, 2026 |
| 12 | README, screenshots, walkthrough and final release | docs and repository release |
| November addition | Power BI implementation | powerbi project, data export, DAX measures and guide |

## Independent review

Completed based on the project owner's confirmation on November 17, 2026: another person tested the project. The tester's name, specific steps, and feedback were not provided; no additional findings are claimed.
