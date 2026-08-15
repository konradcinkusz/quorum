# ADR 0001 — Identity moves to `authservice`

**Status:** Proposed
**Date:** 2026-08-15
**Closes:** [`ARCHITECTURE_REVIEW.md`](ARCHITECTURE_REVIEW.md) F13, and unblocks F12
**Depends on:** [`konradcinkusz/authservice`](https://github.com/konradcinkusz/authservice)
ADR 0002 (RS256 + JWKS), which is implemented

## Context

MR authenticates with ASP.NET Core Identity plus Duende IdentityServer, wired through
`Microsoft.AspNetCore.ApiAuthorization.IdentityServer` — the package behind the old Blazor
WASM "Individual Accounts" template.

That package is discontinued. Restore against `net10.0` on 2026-08-15 answered it exactly:

```
error NU1102: Unable to find package Microsoft.AspNetCore.ApiAuthorization.IdentityServer
  with version (>= 10.0.0)
  - Found 154 version(s) in nuget.org [ Nearest version: 8.0.0-preview.6.23329.11 ]
```

The highest version it ever reached is an 8.0 **preview**; it was abandoned mid-preview when
Duende IdentityServer moved to a commercial licence. So MR's authentication pins it to .NET 7,
which left support on 2024-05-14 — and the cost is already being paid, not deferred: the
current test SDK refuses `net7.0` outright, and thirteen transitive `Microsoft.Extensions.*`
packages resolve only through a `netstandard2.0` compatibility shim.

MR also fails P5 today on its own terms. The constitution requires that **exactly one service
holds a signing key and every other service validates against its published JWKS**, and calls
a shared symmetric secret the estate's most reliably recurring mistake. MR runs its own
IdentityServer with a development signing key and no JWKS story at all.

## Options

**A. Migrate to ASP.NET Core Identity's built-in token endpoints (`MapIdentityApi`).**
Where Microsoft moved everyone, and it removes the Duende dependency. But it keeps identity
inside MR's database and process — MR would still own user accounts, password reset, email
verification, 2FA and OAuth, all of which it would then have to build and get right. This was
the recommendation before `authservice` was examined, and it is worse than B for this estate.

**B. Run an instance of `konradcinkusz/authservice` as MR's identity provider.**
A standalone identity service already extracted for exactly this purpose. It provides, today:
rotating refresh tokens stored as hashes with replay detection that kills the rotation family,
TOTP two-factor with recovery codes, Google and GitHub OAuth, an append-only audit log,
versioned consent tracking with GDPR export, soft-delete with retention, and an admin API.
Crucially it implements **RS256 with a published JWKS** (its ADR 0002), which is precisely
what P5 requires.

The estate has done this before: `authservice` was adopted by another system as its identity
provider, which the constitution records as the first case of one system running its own
instance of another system's extracted service.

**C. Stay on .NET 7 with IdentityServer.** The status quo, and not a decision.

## Decision

**B — MR consumes `authservice`, running as its own deployment with its own database.**

Identity leaves MR entirely. MR becomes a resource server that validates tokens and a BFF that
holds them on the browser's behalf.

### Why this is cheaper than it looks

Three things carry over unchanged, which is most of the risk gone:

- **`MRBaseController.GetUserId()` keeps working.** It reads
  `User.FindFirst(ClaimTypes.NameIdentifier)`, and `authservice` issues
  `ClaimTypes.NameIdentifier = user.Id` on every token. Every ownership check added for F2
  — `IssueOwnerScope.OwnedBy(GetUserId())` — is unaffected.
- **The `RequireAdminRole` policy keeps working.** It requires `ClaimTypes.Role` to contain
  `Admin` or `SuperAdmin`; `authservice` issues role claims under `ClaimTypes.Role`, and
  `SuperAdmin` is one of its own roles.
- **`Issue.CreatedById` stays a `string`.** ASP.NET Identity ids and `authservice` user ids
  are both string keys, so the column and every query filtering on it are untouched.

### The shape

```
Browser (Blazor WASM)
    │   own origin only; HttpOnly, secure, sameSite=strict session cookie
    ▼
MR.Server ── BFF: holds the tokens, injects the bearer header, proxies auth calls
    │   validates via JWKS; holds no key material of any kind
    ▼
authservice ── own Fly app, own PostgreSQL database (P3)
```

Consumer configuration is the whole of MR's token setup:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MetadataAddress = configuration["Auth:MetadataAddress"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = configuration["Auth:Issuer"],     // "AuthService" — a bare string, see below
            ValidAudience = configuration["Auth:Audience"],
        };
    });
```

Signing keys are fetched from the JWKS and refresh on rotation. **MR holds no secret**, which
is the entire point of P5: MR can verify a token and cannot mint one.

### The browser must not hold the token

[`FRONTEND-BFF.md`](https://github.com/konradcinkusz/architecture-standards/blob/main/docs/guides/FRONTEND-BFF.md)
§1 and §3, and
[`SECURITY-REVIEW.md`](https://github.com/konradcinkusz/architecture-standards/blob/main/docs/guides/SECURITY-REVIEW.md)
§4, both forbid tokens in `localStorage`/`sessionStorage` — any XSS exfiltrates them. Since
`authservice` speaks bearer tokens and MR's client is a browser, **MR.Server is the bridge**:

- `POST /bff/auth/login` proxies to `authservice`'s `POST /api/v1/auth/login`, and sets the
  access and refresh tokens as `httpOnly`, `secure`, `sameSite=strict` cookies. Client
  JavaScript never sees a token and never learns `authservice`'s URL.
- `GET /bff/auth/session` rehydrates client session state on page load, because client JS
  cannot read an HttpOnly cookie back — by design.
- Logout **deletes with the same attributes** the cookie was set with; a mismatched
  path/`sameSite` silently leaves it alive.
- MR's own API calls read the cookie server-side and inject the bearer header.

Registration, password reset, email verification and consent all proxy the same way, against
`authservice`'s documented routes (`/api/v1/auth/register`, `/forgot-password`,
`/reset-password`, `/verify-email`, `/consents/versions`).

## The one piece of real work: `Issue.CreatedBy`

Everything above is wiring. This is the change with judgement in it.

`Issue.CreatedBy` is an EF navigation to `ApplicationUser` in *MR's own* database. It is used
in five `.Include(x => x.CreatedBy)` calls, six AutoMapper member mappings, a search filter
(`CreatedBy.Email.Contains(...)`), a sort key (`OrderBy(p => p.CreatedBy.Email)`), and
`IssuePDFService`, which prints the creator's email onto the generated signature sheet.

Once identity lives in another service's database, none of that resolves — and P3 forbids
reaching across to it.

**Resolution: denormalise `CreatedByEmail` onto `Issue`, captured from the token at creation.**
[`IDENTITY-AND-ACCOUNTS.md`](https://github.com/konradcinkusz/architecture-standards/blob/main/docs/guides/IDENTITY-AND-ACCOUNTS.md)
§1 is explicit that claims are enriched at issuance so that a service holding a token **never
calls back** to ask about the user. The `email` claim is already in every token.

For a petition this is arguably *more* correct than the navigation it replaces. A signature
sheet should record the email of the person who filed the initiative **at the time they filed
it** — an audit snapshot — not a mutable lookup that silently rewrites historical documents
when someone later changes their address.

The cost is stated plainly: an email change in `authservice` does not propagate to issues
already created. That is accepted, and it is the correct trade for this domain.

## Consequences

**Gained**

- P5 satisfied: one signing key, held by one service, published as a JWKS. MR holds none.
- F13 closed, F12 unblocked — with the discontinued package gone, nothing pins MR to .NET 7.
- Password reset, email verification, 2FA, OAuth, audit logging, consent versioning and GDPR
  export arrive as someone else's tested code rather than MR's untested code.
- MR's database stops storing credentials at all.

**Costs and risks, including two inherited from `authservice`'s own deviation register**

- **`authservice` defaults to `Database:SchemaMode = EnsureCreated` and ships no committed
  migrations.** This is a direct P4 violation — the one correction the constitution makes to
  its own sources — and it means the identity schema is frozen at first boot. Their register
  records it as "to fix, blocked on a refactor". MR must pin `SchemaMode` explicitly and track
  this; it is a reason to watch that repository, not a reason to avoid it.
- **PostgreSQL is chosen for `authservice`, not SQL Server.** Its SQL Server path has no
  integration coverage — its test suite runs against SQLite — and PostgreSQL is its default
  and tested path. P3 gives it a separate database regardless, so it need not match MR's
  engine. The cost is a second database engine to operate.
- **`iss` is the bare string `AuthService`, not a URL.** Accepted upstream and self-consistent
  with its discovery document; MR sets `ValidIssuer` to match rather than assuming the OIDC
  URL convention.
- **`authservice` emits no OTLP telemetry** (its register, 2026-08-14). MR's F9 work stops at
  MR's boundary; a sign-in failure inside `authservice` will not appear in MR's traces.
- MR gains a runtime dependency on a service that must be up for anyone to log in. It is on
  the synchronous request path, so P7's rule applies: it pins a machine, or MR's timeout
  covers its cold start.

**Not a cost, because there is nothing to migrate.** MR has never been deployed — no
Dockerfile, no pipeline, no environment — and has no real user accounts. `authservice` starts
empty and the first administrator is provisioned through `InitialAdmin:Email` / `Password`.
Had there been live accounts this ADR would need an export/import preserving user ids, since
`Issue.CreatedById` points at them.

## Migration plan

Ordered so that each step is independently reviewable and nothing is half-migrated for long.

| # | Step | Notes |
|---|---|---|
| 1 | Deploy `authservice` with PostgreSQL, RS256 keypair, `InitialAdmin` seeded | `openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048`, then the PEM into a platform secret. Verify `/.well-known/jwks.json` returns a non-empty key set — an empty one means it is still on HS256 |
| 2 | Add `CreatedByEmail` to `Issue` + migration; populate at creation from the `email` claim | Additive and independent: can land before anything else changes |
| 3 | Repoint the five `.Include(CreatedBy)` sites, six mappings, the search filter, the sort key and `IssuePDFService` at `CreatedByEmail` | The bulk of the diff. Characterisation tests first, per P13 |
| 4 | Replace `AddIdentityServer`/`AddApiAuthorization` with `AddJwtBearer` + `MetadataAddress` | MR now validates and cannot mint |
| 5 | Add the BFF routes and HttpOnly cookie session; repoint the Blazor client | `FRONTEND-BFF.md` §3 |
| 6 | Delete `ApplicationUser`, `MRUserManager`, `CustomClaimsPrincipalFactory`, the Identity Razor pages, and the Identity tables; `ApplicationDbContext` stops deriving from `ApiAuthorizationDbContext` | A migration drops `AspNet*`. Safe only because there are no real accounts |
| 7 | Drop `Microsoft.AspNetCore.ApiAuthorization.IdentityServer` | The moment F12 becomes reachable |
| 8 | Upgrade to the current LTS | Previously blocked; retry the `net10.0` bump that produced NU1102 |

**Subscriptions stay in MR.** `MRUserManager.HasActiveSubscription` reads MR's own
`Subscription` table keyed by user id, and subscriptions are MR's domain, not identity's —
`authservice` deliberately excludes billing. Only the `MRUserManager` base class goes; the
subscription lookup becomes a plain query against the caller's subject id.

## Open question

`SignaturePool.ApplicationUserId` and `CloudinaryFileIssue.ApplicationUserId` are the same
pattern as `Issue.CreatedById` and are unaffected as identifiers. Whether either needs a
denormalised email for display has not been checked — step 3 should confirm before it is
called done.
