# MR — Architecture Review

> Measured against
> [`architecture-standards/docs/architecture/00-REFERENCE-ARCHITECTURE.md`](https://github.com/konradcinkusz/architecture-standards/blob/main/docs/architecture/00-REFERENCE-ARCHITECTURE.md)
> — the 15 principles and the compliance checklist. This document does not restate those
> principles; it references them.
>
> **Reviewed:** 2026-08-14, against `master` @ `737060a` (last commit 2023-07-26).
> **Findings F1–F4 fixed:** 2026-08-15 — see the status ledger in §4. The findings below are
> left as written, describing the code as reviewed, so the ledger has something to refer to.
> **Scope:** static review of the repository as committed. No build was run (no .NET SDK
> in the review environment) and no deployed instance was exercised. Per
> [`SECURITY-REVIEW.md`](https://github.com/konradcinkusz/architecture-standards/blob/main/docs/guides/SECURITY-REVIEW.md)
> §1: a code review is static analysis and does not replace a penetration test.

## 0. Why this repo, and what it is

MR is a Blazor WebAssembly citizens'-initiative platform: users create an *issue*
(a referendum question), pay an initial fee, an admin verifies it, it is published into
the current quarter, other users sign it, the quarter is resolved to a winner, and
winners produce a PDF for wet-signature collection which is uploaded back.

It is the surviving line of four repositories holding the same product:

| Repo | Created | Last commit | Commits | C# LOC | Relationship |
|---|---|---|---|---|---|
| `mreferendaInternal` | 2023-03-13 | 2023-03-26 | 50 | 25.5k | First implementation. Superseded. |
| `mreferenda` | 2023-03-29 | 2023-03-29 | 4 | 10.6k | Squashed POC extract of the above, for sharing. Superseded. |
| **`MR`** | **2023-04-11** | **2023-07-26** | **50** | **26.4k** | **The product. Current.** |
| `MRef` | 2023-06-29 | 2023-06-29 | 1 | 17.8k | Single-commit re-import of MR's 2023-06-29 tree (no shared git history). Superseded by MR's own later commits. |

MR is a strict superset of `MRef`: 14 migrations against 9, and the entire quarter-winner
resolution, rating calculation, signature-pool admin, PDF generation and Cloudinary
document pipeline exist only in MR. Everything `MRef` has, MR has later and further along.

**Mode recommendation: RECOVER, not REVIEW.** The playbook's mode table routes on two
signals that both fire here — the target framework is out of support (`net7.0`, EOL
2024-05-14) and there are production credentials in source (§2, F1). This document is the
first-look review the playbook allows before a mode is fixed; it is *not* the MODERNIZE or
RECOVER document set, which remains to be written.

---

## 1. Strengths

These are real and should survive any modernization. Recording them so the next reviewer
does not re-derive them.

**S1 — The layering is already the shape P9 asks for.** `Controllers → MediatR handlers →
DbContext`, with transport genuinely thin: `IssueController` binds, pulls the caller id and
delegates. There are no business rules in a controller. The vertical-slice layout
(`MR.Service/Features/<Domain>/<UseCase>Command.cs`, handler nested in the command) means
one use case is one file, which is the cheapest possible structure to move service by
service later.

**S2 — `Program.cs` is a manifest.** 97 lines, each block a named extension method
(`AddDbContextService`, `AddServiceLayer`, `AddScopedServices`, `AddVersion`). This is
exactly P9's "Agentic shape" rather than the 399-line inline `Program.cs` the constitution
cites as the harder file to change. It is the single clearest piece of evidence that this
repo was written with the architecture's instincts already present.

**S3 — Authorization is structurally correct at the controller boundary.** Every controller
carries a class-level `[Authorize]`; the five admin controllers carry
`[Authorize(Policy = RequireAdminRole)]`; the two genuinely public endpoints carry an
explicit `[AllowAnonymous]`. This is deny-by-default with named exceptions — the structure
[`SECURITY-REVIEW.md`](https://github.com/konradcinkusz/architecture-standards/blob/main/docs/guides/SECURITY-REVIEW.md)
§8 asks for. The failures in §2 are all *below* this line, inside handlers; the boundary
itself is right.

**S4 — Schema is migrated, not `EnsureCreated`.** 14 ordered EF migrations from
`20230426123223_init` to `20230724103000_CloudinaryFileIssueType`. P4's headline correction
— the one the reference SaaS itself violates — is already satisfied here.

**S5 — Domain history is modelled as history.** `IssueProcessingHistory`,
`IssueVisibilityHistory`, `IssueRatingHistory`, `PaymentStatusHistory` plus a
`Payment_Logs` SQL trigger. For a product whose entire value is the auditability of a
petition's lifecycle, append-only state transitions are the right call and were made early.

**S6 — Anti-corruption at the file-storage edge (P11).** `ICloudinaryService` exposes
`UploadImageAsync` / `UploadPdfAsync` over internal `UploadedFile` / `FileData` / `ImageData`
models; no `CloudinaryDotNet` type crosses into a handler. Swapping to blob storage is one
adapter. The namespace even documents *why* the `using` is file-scoped rather than global.

---

## 2. Findings

Ranked by severity. Format per
[`SECURITY-REVIEW.md`](https://github.com/konradcinkusz/architecture-standards/blob/main/docs/guides/SECURITY-REVIEW.md)
§2 — the attack/failure scenario is the load-bearing field.

All four product repos are **private** (verified via the GitHub API), which bounds the blast
radius of F1 but does not remove it: a credential in git history is a credential to rotate,
and it is an absolute bar to the repo ever being made public
([`OPEN-SOURCE-RELEASE.md`](https://github.com/konradcinkusz/architecture-standards/blob/main/docs/guides/OPEN-SOURCE-RELEASE.md)
§2).

---

### F1 — Live Cloudinary API secret committed in `appsettings.json` · **Critical** · P5

```
Location        Server/appsettings.json:19-23 (HEAD), introduced in f0fca15 (2023-07-18)
```

Values are truncated below. The first draft of this document pasted them in full, following
the finding format's "current code" field literally — which turned a document *about* a leak
into a second copy of it, in a new file, and is precisely the mistake P5 says review does not
catch. The `secret-scan` workflow added alongside this review is what flagged it.

```json
"CloudinaryOpt": {
  "Cloud": "dho08…",
  "ApiKey": "3815…",
  "ApiSecret": "QCth…"
}
```

**Attack scenario.** Anyone who has ever had read access to this repository — a
collaborator, a laptop backup, a CI cache, a future accidental public flip — holds the
Cloudinary account's full API credentials. Cloudinary's Admin API accepts them for
`destroy`, `rename` and `resources` calls. The attacker enumerates and deletes every
uploaded signed petition document, or downloads all of them.

**Impact.** Loss of the signed-document corpus, which is the product's legally meaningful
artefact and contains signatories' personal data. Rotation is not optional and is not
sufficient on its own — the value is in history and stays there until history is rewritten.

**Recommendation.** Rotate the Cloudinary key/secret now. Move the section to
`dotnet user-secrets` locally and the platform secret store in deployment; keep only the
non-secret `Cloud` name in `appsettings.json`, or nothing at all.

**Better.** Add a secret scanner as a pre-commit hook *and* a CI job (P5: "enforced by a
scanner, not by review"). Until one exists, this repository is assumed to contain further
credentials. See [`00-SECURITY-IMMEDIATE.md`](00-SECURITY-IMMEDIATE.md) for the full
rotation list across all four repos, including an Azure SQL production password in
`mreferendaInternal`'s history.

---

### F2 — No ownership check on any issue mutation (IDOR) · **Critical** · P-none, `SECURITY-REVIEW` §8

```
Location        MR.Service/Features/Issues/EditIssueCommand.cs:36
                MR.Service/Features/Issues/ArchiveIssueCommand.cs:20
                MR.Service/Features/Issues/Queries/GetIssueByIdForEdit.cs:20
                MR.Service/Features/Issues/Base/IssueCommandHandlerBase.cs:20-32
```

`IssueController` passes the caller's id into `PublishIssueCommand` and `PayForAnIssueCommand`
but **not** into `EditIssueCommand` or `ArchiveIssueCommand`, and the handlers resolve the
issue by primary key alone:

```csharp
// EditIssueCommand.cs:36
var issue = await _context.Issues …
    .FirstAsync(x => x.Id == request._id, cancellationToken);
// no filter on CreatedById
```

The shared base is not a backstop either. `CheckBasicConditionsAndReturnIssue` checks that
the *caller* has an active subscription, then fetches the issue by id — it never compares
`issue.CreatedById` to `request.CreatedById`:

```csharp
// IssueCommandHandlerBase.cs:22-31
var isActiveSub = await _MRUserManager.HasActiveSubscription(request.CreatedById);
if (!isActiveSub) throw new ApplicationException(…);
var issue = await _context.Issues.Include(x => x.InitialPayment)
    .FirstAsync(x => x.Id == request.IssueId, cancellationToken);
```

So `publish-issue/{id}` and `pay-for-an-issue/{id}` inherit the same hole.

**Attack scenario.** Mallory registers, subscribes, and reads any published issue's GUID
from the public `get-current-quarter-issues-published` response. She then
`PUT /edit-issue/{id}` with a new `Question`. The petition text is replaced *after* other
users have signed it. Or she `DELETE /archive-issue/{id}` on a rival initiative, which
sets `IsDeleted = true` and removes it from every listing.

**Impact.** The integrity guarantee the whole product exists to provide is absent. A
signature collected against question A can be silently retargeted to question B, and no
history row records the substitution because `EditIssueCommand` writes no
`IssueProcessingHistory`. Any user can also destroy any other user's paid, admin-verified
initiative.

**Recommendation.** Thread `GetUserId()` into `EditIssueCommand`, `ArchiveIssueCommand` and
`GetIssueByIdForEdit`, and filter on it: `.FirstOrDefaultAsync(x => x.Id == id && x.CreatedById == userId)`,
returning 404 (not 403) on miss. Add the same comparison inside
`CheckBasicConditionsAndReturnIssue` so `publish` and `pay` are covered by construction.

**Better.** Make the ownership check impossible to forget: have `IIssueCommandData` carry
the caller id (it already does — `CreatedById`), and give the base class a single
`LoadOwnedIssueAsync` that is the *only* way a handler obtains an `Issue`. Then also forbid
editing an issue that has any signatures at all — for this product, "published and signed"
should be immutable regardless of who is asking.

---

### F3 — Authenticated listing endpoint leaks every user's private drafts and contact details · **Critical** · `SECURITY-REVIEW` §8

```
Location        Server/Controllers/IssueController.cs:10-16
                MR.Service/Features/Issues/Queries/GetIssuesBySearchParamsQuery.cs:26-56
```

`get-issues-by-search-params` carries only `[Authorize]` — no admin policy. Its handler
applies `IsDeleted == false` and then only the filters the *caller* supplies. There is no
default owner filter and no `IssueVisibility` filter, and the query eager-loads
`.Include(x => x.CreatedBy)` — the full `ApplicationUser`.

The sibling endpoint `get-my-issues-by-search-params` (line 68) proves the intent: it sets
`command.CreatedById = GetUserId()` before dispatching. The unscoped one does not, and the
five admin controllers already have their own admin-policy-gated equivalents — so this
endpoint appears to have no legitimate non-admin caller at all.

**Attack scenario.** Any registered user calls
`GET /get-issues-by-search-params?PageSize=1000` and receives every issue in the system,
including those in `IssueVisibility.VisibleOnlyToMe` and `IssueProcess.InCreation`, each
with its author object attached.

**Impact.** Disclosure of unpublished draft petition text plus the authors' identity and
contact fields. For a political-initiative platform, "who is privately drafting which
petition" is precisely the sensitive pairing.

**Recommendation.** Move the endpoint behind `[Authorize(Policy = RequireAdminRole)]`, or
delete it and let `get-my-issues-by-search-params` serve the user-facing case. Independently,
project to `IssueReadDTO` in the query rather than returning entity graphs, so `CreatedBy`
cannot be over-served by accident.

**Better.** Apply the visibility rule at the data layer — an EF global query filter, or a
mandatory `IssueVisibility`/owner predicate in the query base — so a new endpoint cannot
opt out of it by omission. Findings F2 and F3 are the same root cause: authorization is
expressed at the controller and then re-derived, inconsistently, per handler.

---

### F4 — Seeded superadmin with a publicly-known password, baked into every migration · **High** · P4, P5

```
Location        MR.Persistence/Seeds/DefaultUser.cs:18-19, 34-35
                MR.Persistence/Seeds/ContextSeed.cs:23  (HasData)
                MR.Persistence/ApplicationDbContext.cs:49  (modelBuilder.Seed())
```

```csharp
// Password@123
PasswordHash = "AQAAAAEAACcQ…",   // redacted; the plaintext was the comment above it
EmailConfirmed = true,
```

`superadmin@gmail.com` is seeded into the `SuperAdmin` role with a fixed hash, a comment
naming the plaintext, and `EmailConfirmed = true` so it bypasses the
`RequireConfirmedAccount = true` gate set in `Program.cs:8`. The identical hash is used for
`basicuser@gmail.com`. (The seed rows also carry placeholder names — "Amit Naik" — from the
tutorial this was adapted from, which is a good tell that they were never reviewed as
production data.)

**Attack scenario.** Anyone who reaches the login page of any environment ever created from
these migrations signs in as superadmin with `Password@123` and gains every
`RequireAdminRole` endpoint: issue verification, quarter creation, winner selection,
subscription activation, payment seeding.

**Impact.** Total administrative compromise, including the ability to choose the winning
referendum.

**Recommendation.** Delete the seeded users. Provision the first admin out of band (a
one-time script or a first-run claim flow), and if a seeded account must exist for local
development, gate it on `IsDevelopment()` and generate the password at run time.

**Better.** This is also P4's seed-data rule: `HasData` in `OnModelCreating` embeds the
dataset in every migration snapshot, which is why the 14 `*.Designer.cs` files here total
~17,200 lines and why nobody has read a schema diff since April. Move roles/users to a
seeding service run once per environment, and migrations go back to describing schema only.

---

### F5 — Production always connects to the `DEV` connection string · **High** · P5, correctness

```
Location        MR.Service/DI/DependencyInjection.cs:44-56
```

```csharp
string connectionString = configurationVariableNames.connectionStringPROD;   // "PROD"
var env = configuration["ASPNETCORE_ENVIRONMENT"];
if (env == "Development") connectionString = configurationVariableNames.connectionStringDEV;

services.AddDbContext<ApplicationDbContext>(options => {
    options.EnableSensitiveDataLogging();
    options.UseSqlServer(configuration.GetConnectionString("DEV"), …);   // ← literal "DEV"
}, ServiceLifetime.Transient);
```

The `connectionString` local is computed and then never used; the literal `"DEV"` is passed
instead. `appsettings.json` defines only a `DEV` key, whose value is
`Server=KONRADLENOVO;Trusted_Connection=True` — a developer workstation name with Windows
integrated auth.

**Failure scenario.** Deployed to any environment, the app resolves `ConnectionStrings:DEV`.
If the environment supplies one, production silently runs against whatever is in the *dev*
slot; if it does not, `UseSqlServer(null)` throws at first resolution and every request
500s. There is no configuration a deployer can set that makes `PROD` take effect.

**Impact.** The repo cannot be deployed to a second environment without a code change, and
the failure mode is either "down" or, worse, "up and pointed at the wrong database".

**Recommendation.** Use the computed variable: `configuration.GetConnectionString(connectionString)`.
Better still, drop the DEV/PROD key-switching entirely and read one
`ConnectionStrings:Default` supplied per environment (P5: configuration through the
environment). Remove the workstation connection string from the committed file.

**Also here — `EnableSensitiveDataLogging()` is unconditional** (line 53). In production
this writes parameter values into logs: email addresses, petition text, payment reference
numbers. Guard it with `IsDevelopment()` or delete it. And `ServiceLifetime.Transient` on a
`DbContext` gives each injection site its own change-tracker, so a handler that resolves
the context twice will not see its own pending writes; `Scoped` is the correct lifetime.

---

### F6 — Signed petition documents are uploaded unvalidated to a public CDN · **High** · `SECURITY-REVIEW` §6, GDPR

```
Location        Server/Controllers/IssueController.cs:141-143
                MR.Service/Features/Issues/PDF/UploadSignedDocumentCommand.cs:27-47
                MR.Service/FilesManagement/CloudinaryService.cs:41-56
```

`upload-signed-document/{id}` accepts an `IFormFile` and, with no checks on content type,
magic bytes, extension or size, buffers the whole thing into a `byte[]` and ships it to
Cloudinary as an `ImageUploadParams` with the user-supplied `FileName`. The resulting
`SecureUri` is stored and served. Cloudinary delivery URLs are unauthenticated by default.
No check ties the caller to the issue (same root cause as F2).

**Attack scenario.** (a) A user uploads a 500 MB file; the whole thing is held in managed
memory twice (`MemoryStream` then `ToArray()`), and a handful of concurrent uploads
exhausts the process. (b) A user attaches a document to another user's winning issue. (c)
Anyone who obtains or guesses a delivery URL retrieves a scanned document bearing real
signatories' names and handwritten signatures, with no authorization check anywhere in the
path.

**Impact.** Personal data of petition signatories exposed on a public CDN — the highest-value
data this system holds, and the category with statutory breach-notification duties.

**Recommendation.** Validate before upload: allow-list `application/pdf`, verify the `%PDF-`
magic bytes, cap the size (both at the endpoint via `RequestSizeLimit` and in the handler),
and generate the stored name server-side rather than trusting `FileName`. Add the F2
ownership check. Stream to the client rather than buffering.

**Better.** Signed documents should not be publicly addressable at all: upload as
Cloudinary `ResourceType.Raw` with `type: "authenticated"` (or move to blob storage with
short-lived SAS), and serve them only through an authorizing endpoint. Note also
`UploadSignedDocumentCommand.cs:27` calls `FindAsync(request._issueId, cancellationToken)`
— that binds to the `params object[] keyValues` overload, so EF sees **two** key values for
a single-key entity and throws at run time. This path very likely does not work today; it
needs a test before it needs a fix.

---

### F7 — Health checks are written but never wired · **Medium** · P15, checklist

```
Location        MR.Infrastructure/Extension/ConfigureServiceContainer.cs:40-52
                MR.Infrastructure/Extension/ConfigureContainer.cs:26-45
                Server/Program.cs — no call site
```

`AddHealthCheck(...)` and `UseHealthCheck()` are fully implemented — DbContext check, URL
group, SQL Server check, a health-checks UI at `/healthcheck-ui` — and neither is ever
called. `Program.cs` does not reference them. The same is true of
`ConfigureCustomExceptionMiddleware()`: the middleware exists
(`MR.Service/Middleware/CustomExceptionMiddleware.cs`) and is never added to the pipeline,
so handler exceptions surface as unhandled.

Additionally, `AddHealthCheck` reads `configuration.GetConnectionString("OnionArchConn")`
— a key from the tutorial this was lifted from, which exists nowhere in this repo — so the
code would throw if it were called.

**Failure scenario.** There is no `/health` and no `/alive`. Any platform that decides
whether to route traffic by probing an endpoint has nothing to probe, and a deploy that
comes up with a broken database looks healthy.

**Impact.** The repo cannot satisfy the checklist rows "exposes `/health` and `/alive`" or
"the platform health check points at `/health`", and cannot be deployed behind a health-gated
rollout as it stands.

**Recommendation.** Call both extensions from `Program.cs`, fix the connection-string key,
and split the endpoints into `/health` (readiness, all checks) and `/alive` (liveness,
`live`-tagged only) per P2's table. Add `ConfigureCustomExceptionMiddleware()` to the
pipeline in the same pass.

---

### F8 — No deployment artefact of any kind · **Medium** · P6, P7, P12

There is no `Dockerfile`, no `fly.toml`, no `.github/workflows/`. `MRef`, `mreferenda` and
`mreferendaInternal` each carry at least an Azure App Service workflow; the repo that
replaced them carries none. Deployment today is an F5 in Visual Studio.

**Failure scenario.** The current state cannot be reproduced by anyone but its author on the
machine named in the connection string. There is no build that would catch F5 or F6's
`FindAsync` bug, because nothing ever builds this repository except an IDE.

**Recommendation.** The cheapest first step that repays itself immediately is a CI workflow
that runs `dotnet build` on push — before any container or platform work. Then the
multi-stage Dockerfile (P6) and one `fly.toml` (P7).

**Note on P1/P2.** MR has no Aspire `AppHost` and no `ServiceDefaults`. This is recorded but
**not** raised as a finding: MR is a single deployable unit (one ASP.NET Core host serving a
Blazor WASM client), and P3 is explicit that bounded contexts are drawn around data
cohesion, not nouns — the CopilotScope Collector is the cited precedent for a justified
monolith. A ServiceDefaults-equivalent still earns its place for the OTel/health/resilience
plumbing in F7 and F9, but "split MR into services" is not what this review recommends.

---

### F9 — No telemetry · **Medium** · P15

No OpenTelemetry package, no OTLP exporter, no Application Insights, no structured logging.
`ConfigureContainer.cs:22` calls `loggerFactory.AddSerilog()` from a method misleadingly
named `ConfigureSwagger`, and that method is also never called. Handlers log through
`ILogger<T>` to the default console provider only.

**Failure scenario.** When a payment is accepted but the issue does not publish, there is no
trace to follow and no correlation between the client call and the handler. Diagnosis means
reproducing locally.

**Recommendation.** Add `ConfigureOpenTelemetry` per P2's table: ASP.NET Core, HttpClient and
runtime instrumentation, OTLP exporter enabled when `OTEL_EXPORTER_OTLP_ENDPOINT` is set,
health-check requests filtered out of traces.

---

### F10 — No tests · **Medium** · P13

There is no test project in the solution. The logic-bearing layer — quarter resolution
(`ChooseTheWinnerOfCurrentQuarter`), rating calculation
(`CalculatePublishedIssueRatingForCurrentQuarter`), the publish precondition chain, the
subscription lifecycle — is exactly the "layer that has the logic" P13 names, and none of it
is covered.

**Failure scenario.** Quarter resolution runs four times a year and decides which initiative
wins. A regression in `QuarterExtensions.GetCurrentQuarterExpression()` would be discovered
in production, once, three months late.

**Recommendation.** One xUnit project. Start with characterisation tests over quarter
resolution and rating calculation *before* any restructuring — P13 is explicit that when
behaviour is being migrated, its characterisation tests are written before the move.
`AddDbContextService` will need the InMemory-provider fallback P4 describes; it currently
hard-codes `UseSqlServer`, which is also why the repo has no provider portability.

---

### F11 — No README, and the working language is mixed · **Low** · P14

The repository has no README. `TODO.txt` at the root is the only prose describing intent,
and it is in Polish, as are several XML doc comments
(`DependencyInjection.cs:5` — *"Nazwy muszą być zgodnę z nazwami kluczy w appsettings"*) and
most commit messages. The constitution's §2 resolution is: **English for anything needed to
build or deploy.**

There is no statement anywhere in the repo that MR — rather than `MRef`, `mreferenda` or
`mreferendaInternal` — is the live line.

**Recommendation.** Addressed in this change: a README has been added stating what the
product is, how to run it, and that the three sibling repositories are deprecated.
`TODO.txt`'s content is product backlog and belongs in issues, not the repo root.

---

### F12 — Target framework out of support · **Low today, blocking for any deployment** · P6

`net7.0` across all seven projects; `global.json` pins SDK `7.0.203`. .NET 7 reached
end of support on 2024-05-14 and receives no security patches. P6's rule that the runtime
image major version must equal the TFM major version cannot be satisfied against a base
image that is no longer published with updates.

**Recommendation.** Upgrade to the current LTS before any other structural work — it is the
change most likely to surface compile errors that hide other findings, so it should come
first, not last. The `Microsoft.AspNetCore.ApiAuthorization.IdentityServer` package
(Duende IdentityServer's ASP.NET integration) is the one to check early: it is the
dependency most likely to require a deliberate decision rather than a version bump.

---

## 3. Compliance checklist

Against §3 of the constitution.

| # | Item | Status |
|---|---|---|
| 1 | Declared in the AppHost with `WithReference`/`WaitFor`/`WithHttpHealthCheck` | ❌ no AppHost (see F8 note) |
| 2 | Calls `AddServiceDefaults()` and `MapDefaultEndpoints()` | ❌ no shared kernel |
| 3 | Exposes `/health` and `/alive` | ❌ F7 |
| 4 | Emits OTLP traces, metrics and logs | ❌ F9 |
| 5 | Owns its database; no other service connects to it | ✅ single service, single schema |
| 6 | Schema applied by `MigrateAsync` from migrations, in a hosted service | ⚠️ migrations ✅ (S4); applied manually, no hosted service |
| 7 | All config from env; no secret in source; scanner in CI | ❌ F1, F5 |
| 8 | Exactly one service holds a signing key; others validate via JWKS | ✅ N/A — one service; IdentityServer issues and validates in-process |
| 9 | Shared kernel holds no entity/DTO/seed/constant | ⚠️ N/A — no kernel. `MR.Shared` is a DTO contract shared with the Blazor client, which P2 permits |
| 10 | Every optional integration has a working no-op or fallback | ❌ Cloudinary registration is conditional on the config *section* existing, but there is no `NoOp` implementation, so `git clone && dotnet run` with no credentials cannot serve the upload path (P8) |
| 11 | Multi-stage Dockerfile; runtime major == TFM major; `:8080`; non-root | ❌ F8, F12 |
| 12 | One `fly.toml`; `min_machines_running = 1` on a synchronous path | ❌ F8 |
| 13 | Outbound `HttpClient`s carry the standard resilience handler | ❌ client-side `DataServiceBase` uses a bare `HttpClient` |
| 14 | `Program.cs` is a manifest; wiring in `ServiceCollectionExtensions` | ✅ S2 |
| 15 | Extension points are interfaces registered in DI, not base classes | ⚠️ mixed — `ICloudinaryService`/`IIssuePDFService` ✅ (S6); the `CommandHandlerBase` → `IssueCommandHandlerBase` chain is inheritance, and F2 shows the cost: the shared base looked like a policy enforcement point and was not one |
| 16 | Has a test project; the logic-bearing layer is covered | ❌ F10 |
| 17 | Built by tag-driven workflow with change detection | ❌ F8 |
| 18 | Architectural decisions recorded in `docs/` | ⚠️ this document is the first |

**Score: 3 ✅ · 4 ⚠️ · 11 ❌.** The pattern is consistent and worth naming plainly: what
this repo gets right is *code structure*, and what it lacks entirely is *operability*.
Layering, vertical slices, anti-corruption, migrations and a manifest `Program.cs` are all
in place — the parts an experienced developer writes. Health, telemetry, containers, CI,
tests and secret handling are absent — the parts a repo grows when someone other than its
author has to run it. That is a coherent profile for a solo product at feature-complete
prototype stage, and it means the modernization path is additive rather than a rewrite.

---

## 4. Alignment actions

Time windows per
[`SECURITY-REVIEW.md`](https://github.com/konradcinkusz/architecture-standards/blob/main/docs/guides/SECURITY-REVIEW.md)
§3 — P1 immediately, P2 within a week, P3 within two weeks, P4 long-term.

Legend: **FIXED** · **OPEN** · **OPEN (owner action)** — needs something outside the
repository, so no commit can close it.

### Blocks any deployment

| P | Action | Finding | Status |
|---|---|---|---|
| P1 | Remove the Cloudinary secret from `appsettings.json`; read it from user-secrets / the environment instead | F1 | **FIXED** — 2026-08-15 |
| P1 | **Rotate** the Cloudinary key/secret | F1 | **OPEN (owner action)** — the committed value stays valid until rotated in the Cloudinary console, and stays in git history regardless |
| P1 | Rotate the Azure SQL password and signing certificate in `mreferendaInternal` history | [`00-SECURITY-IMMEDIATE.md`](00-SECURITY-IMMEDIATE.md) | **OPEN (owner action)** |
| P1 | Delete the seeded superadmin/basic users | F4 | **FIXED** — 2026-08-15, seed removed and `20260815000000_RemoveSeededIdentityAccounts` deletes the rows |
| P1 | Apply that migration to every existing database | F4 | **OPEN (owner action)** — nothing applies migrations at startup (F7), so `dotnet ef database update` must be run per environment |
| P1 | Add ownership filters to edit / archive / read-for-edit / publish / pay | F2 | **FIXED** — 2026-08-15 |
| P1 | Gate or delete `get-issues-by-search-params` | F3 | **FIXED** — 2026-08-15, deleted |
| P2 | Validate upload type/size; make signed documents non-public | F6 | **OPEN** — `ICloudinaryService` is the single choke point |
| P2 | Fix the DEV/PROD connection-string switch; guard `EnableSensitiveDataLogging`; `DbContext` → `Scoped` | F5 | **OPEN** — three lines in one method |
| P2 | Add a secret scanner as a pre-commit hook and a CI job | F1 | **OPEN** |

### How F2 and F3 were fixed

Both findings had the same root cause — authorization declared at the controller and then
re-derived, inconsistently, per handler — so the fix is a type rather than five predicates.

`IssueOwnerScope` (`MR.Service/Features/Issues/Base/IssueOwnerScope.cs`) has no public
constructor and exactly two factories: `OwnedBy(userId)`, which restricts to one user and
throws if the id is absent, and `Administrator()`, which does not restrict. Every command
and query that resolves an issue by id now **requires** one as a constructor argument, so
the compiler rejects a new call site that has not made the choice, and a reader of a
controller action can see which scope is in force without opening the handler. The
restriction itself is applied by one `RestrictToOwner` query extension.

`CheckBasicConditionsAndReturnIssue` — the shared base that `publish` and `pay` route
through, and which previously looked like a policy enforcement point while checking only
the subscription — now scopes to `request.CreatedById`. It was the single most valuable
place to put the check, because both commands inherit it.

Three secondary changes fell out of this and are worth knowing about:

- **Not-found and not-yours return the same result.** Every one of these handlers now
  reports `NotFoundException` (or `false`, for archive) whether the issue does not exist or
  belongs to someone else, so the endpoints cannot be used to probe for other users' issue
  ids.
- **`EditIssue` is now wrapped in `HandleErrors`.** It was the one action on
  `IssueController` that was not, which was harmless while the handler could not throw and
  would have produced an unhandled 500 the moment it could.
- **`FirstAsync` → `FirstOrDefaultAsync`** in all four handlers. The former throws
  `InvalidOperationException` on a miss, which was the pre-existing behaviour for a
  non-existent id and is not a usable authorization signal.

`get-issues-by-search-params` was deleted rather than gated. It had no caller — the client's
user-facing pages use `get-my-issues-by-search-params`, and the admin console uses
`AdminIssueController`'s `get-issues-by-search-params-admin` — so an unscoped listing whose
admin-scoped twin already exists is a duplicate, not a missing policy. Its orphaned client
method went with it.

### Two things the F1 and F4 fixes deliberately do *not* do

- **Removing the secret from `appsettings.json` does not remove it from git history**, and
  does not make it invalid. Until it is rotated in the Cloudinary console, the value in
  commit `f0fca15` still works. This is the reason "rotate" is tracked as a separate,
  still-open row above rather than being folded into the code fix.
- **The Cloudinary fallback throws rather than no-ops.** P8 asks optional integrations to
  degrade, and the usual shape is a no-op that logs. That is wrong here:
  `CloudinaryNotConfiguredService` throws with an explanatory message, because the files
  crossing this interface are wet-signature petition documents and a stub that accepted an
  upload and discarded it would tell a user their signature sheet was stored when it had
  been thrown away. The application still starts and every other feature still works, which
  is the part of P8 that matters.

### Should be done before deployment

| P | Action | Finding |
|---|---|---|
| P2 | Add a CI workflow running `dotnet build` | F8 |
| P2 | Upgrade `net7.0` → current LTS | F12 |
| P3 | Wire `AddHealthCheck`/`UseHealthCheck` (fix the `OnionArchConn` key); split `/health` and `/alive`; add the exception middleware | F7 |
| P3 | Add an xUnit project; characterisation tests over quarter resolution and rating first | F10 |
| P3 | Add OpenTelemetry per P2's table | F9 |
| P3 | Multi-stage Dockerfile + one `fly.toml` | F8 |
| P4 | Move seed data out of `HasData` into a seeding service | F4 |
| P4 | Provider portability in `AddDbContextService` (InMemory fallback) | F10, P4 |
| P4 | Replace the handler inheritance chain with interface + registration | checklist 15 |
| P4 | English for build/deploy-relevant prose | F11 |

### Residual risks — deliberately not addressed here

- **This review is static, and so are the fixes.** No build, no run, no penetration test —
  there is no .NET SDK in the review environment and NuGet is unreachable from it, so the
  2026-08-15 changes are **not compile-verified**. They were written against the existing
  code's own idioms and reviewed by hand, but `dotnet build` is the first thing that should
  happen to this branch. F6's `FindAsync` bug is the clearest signal that some of these
  paths have not executed recently.
- **A user editing their own issue silently un-verifies it.** `EditIssueCommand.IsVerifyByAdmin`
  defaults to `false` rather than `null`, and the handler's `request.IsVerifyByAdmin ?? issue.IsVerifyByAdmin`
  therefore writes `false` on every user-facing edit. This is pre-existing behaviour, it
  fails safe (an edit invalidating admin verification is defensible), and it was left alone
  by the P1 pass — but it is almost certainly accidental rather than designed, and the
  `= false` initializer should become `= null` once there is a test to prove what the
  publish flow expects.
- **F1's secret remains in git history** after rotation. Rewriting history is a separate,
  disruptive operation and is only strictly required before the repo goes public.
- **Payments were not reviewed against
  [`PAYMENTS-AND-MONETIZATION.md`](https://github.com/konradcinkusz/architecture-standards/blob/main/docs/guides/PAYMENTS-AND-MONETIZATION.md).**
  `docs/stripe.txt` suggests Stripe was intended, but the committed payment flow is manual
  status transitions with no provider integration and no webhook. That is a whole guide's
  worth of review that has not been done.
- **The Blazor client's token handling was not audited** against
  [`SECURITY-REVIEW.md`](https://github.com/konradcinkusz/architecture-standards/blob/main/docs/guides/SECURITY-REVIEW.md)
  §4. It uses the framework-default `Microsoft.AspNetCore.Components.WebAssembly.Authentication`
  OIDC client, which is a reasonable default but was not verified.
- **No deviation register (§3a) is opened yet.** Once the P1 items are fixed, the remainder
  should move into `docs/architecture/DEVIATIONS.md` with dates, so what stays open stays
  visible.
