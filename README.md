# Quorum

A citizens'-initiative platform: users publish a referendum question (an *issue*), other
users sign it, and each quarter the highest-rated issue is resolved as that quarter's
winner and taken forward for wet-signature collection.

**This is the current repository for this product.** It was called `MR` until 2026-08-15;
three earlier repositories hold earlier versions of the same system and are deprecated — see
[Repository lineage](#repository-lineage).

The name is the mechanic: an initiative goes nowhere until enough people have signed it, and
the quarterly cycle exists to find the one that reaches its quorum first.

> **Status: prototype, not deployed.** The last feature commit is from July 2023. It builds
> and runs locally against SQL Server; it has no container, no CI and no deployment
> pipeline. Before it is run anywhere other than a development machine, read
> [`docs/architecture/00-SECURITY-IMMEDIATE.md`](docs/architecture/00-SECURITY-IMMEDIATE.md)
> — credentials that were committed to this repository are still valid until they are
> rotated, which is not something a code change can do for you.

## What it does

| Capability | Where |
|---|---|
| Create an issue, pay the initial fee, get admin verification, publish | `Quorum.Service/Features/Issues/` |
| Sign and unsign published issues; signature pools | `Quorum.Service/Features/SignatureFeautres/`, `SignaturePoolsFeatures/` |
| Quarterly cycle: init a quarter, rate published issues, choose the winner | `Quorum.Service/Features/QuarterFeatures/`, `Issues/ChooseTheWinnerOfCurrentQuarter.cs` |
| Subscriptions: buy, activate, deactivate, refund, reject | `Quorum.Service/Features/SubscriptionFeatures/` |
| Payments with full status history and a SQL audit trigger | `Quorum.Service/Features/PaymentFeatures/`, `SQLs/` |
| PDF generation for winning issues; signed-document upload to Cloudinary | `Quorum.Service/Features/Issues/PDF/`, `Quorum.Service/FilesManagement/` |
| Admin console: issues, quarters, subscriptions, signature pools, logs | `Client/Pages/Admin/`, `Server/Controllers/Admin/` |

## Architecture

.NET 7, Blazor WebAssembly hosted by an ASP.NET Core server, layered as:

```
Client        Blazor WASM SPA
Server        controllers, AutoMapper profiles, IdentityServer host
  Quorum.Service       MediatR handlers, one file per use case (Features/<Domain>/<UseCase>Command.cs)
  Quorum.Infrastructure  DI composition, cross-cutting extensions
  Quorum.Persistence     ApplicationDbContext, EF migrations, seeds
  Quorum.Domain          entities, enums, constants, settings
Shared        DTOs shared between Client and Server
```

Authentication is ASP.NET Core Identity + Duende IdentityServer (`AddApiAuthorization`),
with role claims flowed into the SPA. Persistence is EF Core against SQL Server, schema by
migration.

## Running it locally

Requires the .NET 7 SDK (pinned in `global.json`) and a reachable SQL Server instance.

1. Point `ConnectionStrings:DEV` at your SQL Server. **Do not commit it** — use
   `dotnet user-secrets` (the `UserSecretsId` is already declared in `Server/Quorum.Server.csproj`):
   ```sh
   dotnet user-secrets --project Server set "ConnectionStrings:DEV" "<your connection string>"
   ```
   The value currently committed in `Server/appsettings.json` names a specific developer
   workstation and will not resolve for you.
2. Apply the schema:
   ```sh
   dotnet ef database update --project Quorum.Persistence --startup-project Server
   ```
3. Set `Quorum.Server` as the startup project and run. Swagger is at `/swagger`.

Cloudinary credentials are needed only for the document-upload and PDF paths. Without them
the application starts and everything else works; those two endpoints fail with a message
telling you what to set. Supply them through user-secrets, never in `appsettings.json`:

```sh
dotnet user-secrets --project Server set "CloudinaryOpt:Cloud"     "<cloud name>"
dotnet user-secrets --project Server set "CloudinaryOpt:ApiKey"    "<api key>"
dotnet user-secrets --project Server set "CloudinaryOpt:ApiSecret" "<api secret>"
```

In a deployed environment the same values are `CloudinaryOpt__Cloud`, `__ApiKey` and
`__ApiSecret`.

> **If you have a database created before 2026-08-15**, step 2 also applies migration
> `20260815000000_RemoveSeededIdentityAccounts`, which deletes the seeded
> `superadmin@gmail.com` and `basicuser@gmail.com` accounts. Those accounts share one
> password that was written in a source comment, so run it against every environment. The
> migration is deliberately not reversible.

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
| **`Quorum`** (this repo, formerly `MR`) | Apr 2023 – present | **Current** |
| [`MRef`](https://github.com/konradcinkusz/MRef) | Jun 2023 | Deprecated — single-commit snapshot of Quorum's 2023-06-29 tree |

Quorum is a strict superset of all three: 14 migrations against `MRef`'s 9, and quarter-winner
resolution, rating calculation, PDF generation, the Cloudinary pipeline and the signature-pool
admin exist only here.

## Project name

Renamed from `MR` to `Quorum` on 2026-08-15. Two initials told a newcomer nothing and
collided with everything, which was part of why four repositories holding one product were
hard to tell apart in the first place.

What changed: the eight projects, their directories, every namespace and assembly, the
solution, and the types that carried the old initials (`MRBaseController`,
`MRUserManager`, `MrUser`). The `MrUsers` table is renamed by migration
`20260815030000_RenameMrUsersToQuorumUsers`.

What deliberately did **not** change: the `MRBasics`, `MRPayments` and `MRDicts` database
schemas. They are referenced by seventeen historical migrations that are a replay record
rather than source, they are invisible outside the database, and renaming them would mean a
multi-table `ALTER SCHEMA … TRANSFER` for no benefit. The physical database name is
per-environment configuration and is yours to set.

Old GitHub URLs redirect, so existing clones and links keep working; update your remote with
`git remote set-url origin https://github.com/konradcinkusz/Quorum`.

## Backlog

`TODO.txt` at the repository root carries the product backlog as it stood in July 2023, in
Polish. It is kept as a record of intent; it is not a maintained plan.
