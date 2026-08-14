# MR

A citizens'-initiative platform: users publish a referendum question (an *issue*), other
users sign it, and each quarter the highest-rated issue is resolved as that quarter's
winner and taken forward for wet-signature collection.

**This is the current repository for this product.** Three earlier repositories hold
earlier versions of the same system and are deprecated — see
[Repository lineage](#repository-lineage).

> **Status: prototype, not deployed.** The last feature commit is from July 2023. It builds
> and runs locally against SQL Server; it has no container, no CI and no deployment
> pipeline. Before it is run anywhere other than a development machine, read
> [`docs/architecture/00-SECURITY-IMMEDIATE.md`](docs/architecture/00-SECURITY-IMMEDIATE.md)
> — there are credentials to rotate and a seeded administrator account to remove.

## What it does

| Capability | Where |
|---|---|
| Create an issue, pay the initial fee, get admin verification, publish | `MR.Service/Features/Issues/` |
| Sign and unsign published issues; signature pools | `MR.Service/Features/SignatureFeautres/`, `SignaturePoolsFeatures/` |
| Quarterly cycle: init a quarter, rate published issues, choose the winner | `MR.Service/Features/QuarterFeatures/`, `Issues/ChooseTheWinnerOfCurrentQuarter.cs` |
| Subscriptions: buy, activate, deactivate, refund, reject | `MR.Service/Features/SubscriptionFeatures/` |
| Payments with full status history and a SQL audit trigger | `MR.Service/Features/PaymentFeatures/`, `SQLs/` |
| PDF generation for winning issues; signed-document upload to Cloudinary | `MR.Service/Features/Issues/PDF/`, `MR.Service/FilesManagement/` |
| Admin console: issues, quarters, subscriptions, signature pools, logs | `Client/Pages/Admin/`, `Server/Controllers/Admin/` |

## Architecture

.NET 7, Blazor WebAssembly hosted by an ASP.NET Core server, layered as:

```
Client        Blazor WASM SPA
Server        controllers, AutoMapper profiles, IdentityServer host
  MR.Service       MediatR handlers, one file per use case (Features/<Domain>/<UseCase>Command.cs)
  MR.Infrastructure  DI composition, cross-cutting extensions
  MR.Persistence     ApplicationDbContext, EF migrations, seeds
  MR.Domain          entities, enums, constants, settings
Shared        DTOs shared between Client and Server
```

Authentication is ASP.NET Core Identity + Duende IdentityServer (`AddApiAuthorization`),
with role claims flowed into the SPA. Persistence is EF Core against SQL Server, schema by
migration.

## Running it locally

Requires the .NET 7 SDK (pinned in `global.json`) and a reachable SQL Server instance.

1. Point `ConnectionStrings:DEV` at your SQL Server. **Do not commit it** — use
   `dotnet user-secrets` (the `UserSecretsId` is already declared in `Server/MR.Server.csproj`):
   ```sh
   dotnet user-secrets --project Server set "ConnectionStrings:DEV" "<your connection string>"
   ```
   The value currently committed in `Server/appsettings.json` names a specific developer
   workstation and will not resolve for you.
2. Apply the schema:
   ```sh
   dotnet ef database update --project MR.Persistence --startup-project Server
   ```
3. Set `MR.Server` as the startup project and run. Swagger is at `/swagger`.

Cloudinary credentials are needed only for the document-upload and PDF paths; without them
those endpoints will fail rather than degrade. Supply them through user-secrets under
`CloudinaryOpt`, never in `appsettings.json`.

## Architecture documentation

- [`docs/architecture/ARCHITECTURE_REVIEW.md`](docs/architecture/ARCHITECTURE_REVIEW.md) —
  this repo measured against
  [`konradcinkusz/architecture-standards`](https://github.com/konradcinkusz/architecture-standards):
  strengths, twelve findings ranked by severity, the compliance checklist, and a
  prioritized alignment-actions table.
- [`docs/architecture/00-SECURITY-IMMEDIATE.md`](docs/architecture/00-SECURITY-IMMEDIATE.md)
  — credentials to rotate, across this repo and its three predecessors.

## Repository lineage

| Repo | Period | Status |
|---|---|---|
| [`mreferendaInternal`](https://github.com/konradcinkusz/mreferendaInternal) | Mar 2023 | Deprecated — first implementation |
| [`mreferenda`](https://github.com/konradcinkusz/mreferenda) | Mar 2023 | Deprecated — squashed POC extract |
| **`MR`** (this repo) | Apr–Jul 2023 | **Current** |
| [`MRef`](https://github.com/konradcinkusz/MRef) | Jun 2023 | Deprecated — single-commit snapshot of MR's 2023-06-29 tree |

MR is a strict superset of all three: 14 migrations against `MRef`'s 9, and quarter-winner
resolution, rating calculation, PDF generation, the Cloudinary pipeline and the signature-pool
admin exist only here.

## Backlog

`TODO.txt` at the repository root carries the product backlog as it stood in July 2023, in
Polish. It is kept as a record of intent; it is not a maintained plan.
