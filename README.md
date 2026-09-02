# Quorum

A citizens'-initiative platform: users publish a referendum question (an *issue*), other
users sign it, and each quarter the highest-rated issue is resolved as that quarter's
winner and taken forward for wet-signature collection.

**This is the current repository for this product.** It was called `MR` until 2026-08-15;
three earlier repositories hold earlier versions of the same system and are deprecated — see
[Repository lineage](#repository-lineage).

The name is the mechanic: an initiative goes nowhere until enough people have signed it, and
the quarterly cycle exists to find the one that reaches its quorum first.

> **Status: prototype, deployable.** CI builds and tests every push; a tag-driven
> workflow deploys the whole estate to Fly.io (`.github/workflows/flyio.yml`, config in
> [`flyio/`](flyio/)). No environment is live yet. Before running one, read
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

.NET 10, Blazor WebAssembly hosted by an ASP.NET Core server, layered as:

```
Client        Blazor WASM SPA
Server        controllers, AutoMapper profiles, BFF auth endpoints
  Quorum.Service       MediatR handlers, one file per use case (Features/<Domain>/<UseCase>Command.cs)
  Quorum.Infrastructure  DI composition, cross-cutting extensions, JWT validation, cookie bridge
  Quorum.Persistence     ApplicationDbContext, provider switch
  Quorum.Persistence.Migrations.{PostgreSQL,SqlServer}  per-provider migration sets
  Quorum.Domain          entities, enums, constants
Shared        DTOs shared between Client and Server (incl. the BFF auth contract)
```

**Identity lives outside this repository**
([ADR 0001](docs/architecture/0001-identity-via-authservice.md)): Quorum runs its own
instance of [`konradcinkusz/authservice`](https://github.com/konradcinkusz/authservice) —
a version-pinned image, never a source dependency — and validates the RS256 tokens it
issues against that instance's published JWKS. Quorum holds no signing key and cannot mint
a token. The browser holds no token either: `Quorum.Server` is a BFF whose
`/bff/auth/*` endpoints proxy login/registration/refresh to authservice and keep the
token pair in `HttpOnly; Secure; SameSite=Strict` cookies, translated back into a bearer
header server-side on every API call.

Persistence is EF Core behind a provider switch (`DATABASE_PROVIDER`): PostgreSQL is the
deployed default, SQL Server remains supported, and with no connection string configured
the InMemory fallback keeps `git clone && dotnet run` and the test suite working with
zero infrastructure. The schema is applied by migration from a background service after
the listener is up — readiness (`/health`) reports 503 until it lands.

## Running it locally

Requires the .NET 10 SDK (pinned in `global.json`) and Docker (for the backing services).

1. Bring up Postgres and the local authservice instance:
   ```sh
   scripts/dev-up.sh
   ```
   On the first run this generates a local RS256 signing key into `.dev/` (git-ignored —
   a development convenience, never a deployed trust root) and seeds an initial admin,
   `admin@quorum.local` / `Admin123!`. Windows: run
   `scripts/generate-jwt-signing-key.ps1 -Path .dev/keys/authservice-dev.pem` once, then
   `docker compose up -d`.
2. Run the app:
   ```sh
   dotnet run --project Server
   ```
   The schema is applied automatically at startup by the migration background service.
   Swagger is at `/swagger`; register or sign in at `/login`.

Prefer SQL Server locally? Set `DatabaseProvider` to `SqlServer` and point
`ConnectionStrings:Default` at your instance through user-secrets (the `UserSecretsId` is
declared in `Server/Quorum.Server.csproj`) — **never** in a committed `appsettings` file:
```sh
dotnet user-secrets --project Server set "DatabaseProvider" "SqlServer"
dotnet user-secrets --project Server set "ConnectionStrings:Default" "<your connection string>"
```
SQL Server migrations are regenerated with `scripts/generate-migrations.sh` after model
changes; the PostgreSQL set is the one the deployed estate applies.

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

> **If you have a local database created before the authservice cutover** (ADR 0001,
> 2026-08-15): drop and recreate it. The migration history was re-baselined when the
> `AspNet*`/IdentityServer tables left the schema — there has never been a deployed
> environment or a real user account, so there is nothing to carry over
> (`docs/DropAllTablesInSchema.txt` has the script). This also disposes of the seeded
> `superadmin@gmail.com`/`basicuser@gmail.com` accounts and their shared committed
> password: user accounts no longer exist in this database at all.

## Deploying

The estate deploys to Fly.io from a tag (`git tag v0.x.y && git push --tags`): three
apps — `quorum-postgres`, `quorum-authservice` (the pinned identity image),
`quorum-server` — deployed in dependency order with change detection against the previous
tag. A missing Fly app is always selected, so a cold estate comes up from a single tag.
The deploy asserts what health checks cannot see: that the identity instance publishes a
**non-empty JWKS**.

- [`flyio/`](flyio/) — one `fly.toml` per app, annotated
- [`flyio/SECRETS.md`](flyio/SECRETS.md) — what is a secret, where it lives, how it is set
- [`flyio/INFRASTRUCTURE-ANALYSIS.md`](flyio/INFRASTRUCTURE-ANALYSIS.md) — topology,
  sizing and cost reasoning, including which synchronous call pins which machine
- `flyio-scale` / `flyio-destroy` workflows — manual scaling and (typed-confirmation)
  teardown

## Architecture documentation

- [`docs/architecture/ARCHITECTURE_REVIEW.md`](docs/architecture/ARCHITECTURE_REVIEW.md) —
  this repo measured against
  [`konradcinkusz/architecture-standards`](https://github.com/konradcinkusz/architecture-standards):
  strengths, twelve findings ranked by severity, the compliance checklist, and a
  prioritized alignment-actions table.
- [`docs/architecture/DEVIATIONS.md`](docs/architecture/DEVIATIONS.md) — what this repository
  does differently from that reference architecture, dated, each with a reason and an exit,
  plus the open actions no commit can close.
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
`git remote set-url origin https://github.com/konradcinkusz/quorum`.

## Contributing

[`CONTRIBUTING.md`](CONTRIBUTING.md) covers the local setup, what CI does and does not
establish, and the conventions that are specific to this codebase — vertical slices,
`IssueOwnerScope`, ADRs, and why an applied migration is never edited.
[`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md) applies.

## Security

Found a vulnerability? Please report it privately — see [`SECURITY.md`](SECURITY.md), which
also lists what is **already known and open** so you do not spend effort re-reporting it.

The short version of what is known: credentials committed in 2023 remain in this
repository's history and are not yet rotated, and signed petition documents uploaded before
delivery was locked down are still reachable on the CDN. Both are tracked, both need an
action in somebody's console rather than a commit, and both are read before running an
environment —
[`docs/architecture/00-SECURITY-IMMEDIATE.md`](docs/architecture/00-SECURITY-IMMEDIATE.md).

## License

MIT — see [`LICENSE`](LICENSE).

Every runtime dependency is MIT or similarly permissive. The one that was not — **iTextSharp
5.x, which is AGPL** — generated the signature sheet, and an MIT licence on this repository
does not change the terms such a dependency carries into a distributed binary. It was replaced
with PDFsharp + MigraDoc under [ADR 0002](docs/architecture/0002-pdf-generation.md)
([#15](https://github.com/konradcinkusz/quorum/issues/15) chose,
[#21](https://github.com/konradcinkusz/quorum/issues/21) did it).

## Roadmap

[`ROADMAP.md`](ROADMAP.md) carries the plan this repository is being taken through: what
"complete" means for it, four phases with their issues, why they are in that order, which
paths are protected, and what is deliberately out of scope. Progress is tracked in
[#22](https://github.com/konradcinkusz/quorum/issues/22).

The short version: the gap here is operability rather than features, so nothing on the
roadmap adds a capability.

## Backlog

[`BACKLOG.md`](BACKLOG.md) carries the product intent: seven items still live, each checked
against the code rather than carried forward on trust, plus what has since been built. It
replaces the Polish `TODO.txt` of July 2023, whose text it preserves verbatim in an appendix.

Product work is deliberately separate from [`ROADMAP.md`](ROADMAP.md), which covers
operability and adds no capability.
