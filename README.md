# Reclaim — Lost-and-Found API

A Skills Challenge practice project by Sultan. Reclaim records found items and manages ownership claims.

**Estimated project time: approximately 20 focused hours**, including learning, coding, setup, debugging and testing. Time was not formally tracked.

Built with C#, ASP.NET Core (.NET 9), Entity Framework Core 9, SQL Server and xUnit. Tested through **Postman**, not Swagger. No frontend or login system is included.

## What the project does

- Creates, lists, updates and deletes found-item records.
- Searches by name and filters by category or status, with newest found dates first.
- Returns short item summaries or full details with related claims.
- Creates claims and lets them be rejected, approved or completed.

Approving a claim reserves the item and rejects its other pending claims. Completing the approved claim marks the item as returned. Items with claims cannot be deleted.

## How to run it

You need Windows, Visual Studio with ASP.NET development tools and support for `.slnx` solutions, the .NET 9 SDK, SQL Server LocalDB, SQL Server Management Studio (SSMS) and Postman.

1. Extract the ZIP and open `Reclaim App.slnx` in Visual Studio. Allow NuGet packages to restore.
2. In SSMS, connect to `(localdb)\MSSQLLocalDB` using Windows Authentication.
3. Execute `Reclaim.Api/Database/Reclaim_Database_First_Starter.sql` once to create `ReclaimPracticeDb`, four tables and sample data. It stops if tables already exist; do not delete an existing database to rerun it.
4. Check `Reclaim.Api/appsettings.json`. Its connection setting should point to the same SQL Server instance:

   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ReclaimPracticeDb;Trusted_Connection=True;TrustServerCertificate=True;"
   }
   ```

   Keep the rest of the JSON file. Change `Server` if using a different SQL Server instance.

5. Set **Reclaim.Api** as the startup project, select the **https** launch profile and run it.
6. In Postman, send `GET https://localhost:7062/api/FoundItems`. Expect `200 OK` and a JSON list. The app does not automatically open a browser.

This is **database-first**: generated models are included, so no migration commands are needed. Each computer needs its own database setup; Git transfers the script, not database contents.

## Main endpoints

Base URL: `https://localhost:7062`. Replace `{id}` with an actual item or claim ID.

| Method | Path | Purpose |
| --- | --- | --- |
| GET | `/api/FoundItems` | List, search and filter items |
| GET | `/api/FoundItems/{id}` | View an item and its claims |
| POST | `/api/FoundItems` | Create an item; see cleanup note below |
| PUT | `/api/FoundItems/{id}` | Update item information |
| DELETE | `/api/FoundItems/{id}` | Delete an item without claims |
| GET | `/api/ItemClaims/{id}` | View one claim |
| POST | `/api/ItemClaims` | Submit a claim |
| PUT | `/api/ItemClaims/{id}/reject` | Reject a pending claim |
| PUT | `/api/ItemClaims/{id}/approve` | Approve a pending claim |
| PUT | `/api/ItemClaims/{id}/complete` | Complete an approved claim |

Example filter: `/api/FoundItems?search=calculator&status=Available&categoryId=1`.

## Quick Postman demonstration

Fresh sample data includes available item **2** and claimants **1** and **2**. If your data changed, choose an available item first.

1. Send `POST https://localhost:7062/api/ItemClaims`. Under **Body → raw → JSON**, enter:

   ```json
   {
     "foundItemId": 2,
     "claimantId": 1,
     "ownershipDescription": "My black pencil case has two blue pens inside."
   }
   ```

2. Expect `201 Created`. Save the returned `id`. Submit another claim with `claimantId: 2`, before approving either.
3. Send `PUT /api/ItemClaims/{id}/approve` using the first claim's ID. No body is needed. Expect `204 No Content`.
4. Send `GET /api/FoundItems/2`. The item should be `Reserved`, the chosen claim `Approved`, and the other claim `Rejected`.
5. Send `PUT /api/ItemClaims/{id}/complete` for the approved claim. Check the item again: it should now be `Returned` and that claim `Completed`.

## Automated tests

Open **Test → Test Explorer → Run All**, or run this from the extracted project root:

```bash
dotnet test Reclaim.Api.Tests/Reclaim.Api.Tests.csproj
```

Six tests cover valid creation, missing/unavailable items, rejection, approval with competing claims, and completion. Sultan reported all six passing locally.

Tests use isolated in-memory data, without SQL Server. They check controller logic, not HTTP request validation or real SQL Server behavior. See Microsoft's [controller testing](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/testing?view=aspnetcore-9.0) and [test database](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy) guides.

## Where things are

| Location | Contains |
| --- | --- |
| `Reclaim.Api/Controllers` | Endpoints and business rules |
| `Reclaim.Api/Models` | Classes representing database records |
| `Reclaim.Api/Dtos` | Shapes of incoming requests and outgoing responses |
| `Reclaim.Api/Data` | EF Core database context |
| `Reclaim.Api/Database` | Database creation and sample-data script |
| `Reclaim.Api.Tests/UnitTest1.cs` | The six claim-controller tests |

## Known cleanup item

`CreateFoundItemDto` contains unused claim fields: `FoundItemId`, `ClaimantId` and required `OwnerShipDescription`. A normal create-item request without `OwnerShipDescription` returns `400`. Remove these extra fields from this DTO during cleanup; they belong in the separate claim-request DTO. Create/update item descriptions currently allow 80 characters.

Application code is unchanged. Tests were not rerun for this documentation because the review environment lacked the .NET SDK.
