# Contributing

Thanks for looking. This file covers the things that are specific to Quorum and that you
would otherwise discover by hitting them.

## Scope

Quorum is a citizens'-initiative platform, and the work it needs right now is **operability
rather than features** — see [`ROADMAP.md`](ROADMAP.md) for what that means and
[#22](https://github.com/konradcinkusz/quorum/issues/22) for progress. A feature pull request
is not unwelcome, but it will get a slower read than one that closes a roadmap issue.

## Getting set up

You need the **.NET 10 SDK** — the exact version is pinned in
[`global.json`](global.json) — and **Docker** for the backing services.

```sh
scripts/dev-up.sh          # Postgres + a local authservice instance
dotnet run --project Server
```

On its first run `dev-up.sh` generates an RS256 signing key into `.dev/` (git-ignored, a
development convenience and never a deployed trust root) and seeds an admin account. Windows:
run `scripts/generate-jwt-signing-key.ps1 -Path .dev/keys/authservice-dev.pem` once, then
`docker compose up -d`.

The README's [Running it locally](README.md#running-it-locally) section is the full version,
including SQL Server and the Cloudinary credentials — this file does not repeat it.

**Identity is not in this repository.** Quorum runs a version-pinned image of
[`authservice`](https://github.com/konradcinkusz/authservice) and validates its tokens against
that instance's JWKS ([ADR 0001](docs/architecture/0001-identity-via-authservice.md)). If you
are changing anything about login, read that first: Quorum holds no signing key, and the
browser holds no token.

## What CI checks, and why it is the only thing that does

Two workflows run on every pull request. Between them they are the **only** automated
verification this project has, so it is worth knowing exactly what they establish.

**`build`** — `dotnet restore`, a Release build of all nine projects, then
`dotnet test tests/Quorum.Tests`. The test step asserts a **positive pass count** rather than
trusting the exit code, because a run that discovers zero tests also exits successfully. If
you delete the last test in a file, expect this to fail rather than go quietly green.

**`secret-scan`** — `gitleaks` over the working tree *and full history*, plus a step that
validates the config parses before either scan runs. That guard exists because the scanner
spent two weeks failing at config load while looking exactly like a normal red X
([#24](https://github.com/konradcinkusz/quorum/issues/24)).

**And a pre-commit hook, which runs before either.** `scripts/dev-up.sh` activates it; it
scans the staged changes against the same `.gitleaks.toml` CI uses, so the two cannot disagree
about what counts as a secret. Without `gitleaks` installed it warns and lets the commit
through rather than blocking on a missing tool — a hook that refuses for the wrong reason gets
disabled and never re-enabled.

`git commit --no-verify` bypasses it. That is stated here rather than left to be discovered,
because pretending an escape hatch does not exist does not remove it — and CI is not
bypassable, which is the point.

**What CI does not establish.** The test suite covers pure logic — scopes, rules, quarter
arithmetic. Nothing exercises the database, the HTTP pipeline, or a real request. A change can
be green and still not work; the standing example is a `FindAsync` call that compiled cleanly
for three years and threw the first time it ran. Issues
[#16](https://github.com/konradcinkusz/quorum/issues/16) and
[#17](https://github.com/konradcinkusz/quorum/issues/17) are closing that gap. Until they do,
say in your pull request what you actually verified and what you only read.

## Conventions

**Vertical slices.** One file per use case, under
`Quorum.Service/Features/<Domain>/<UseCase>Command.cs`, handled by MediatR. Follow the
neighbours rather than introducing a layer.

**Authorization is a type, not a habit.** Every command and query that resolves an issue by
id takes an `IssueOwnerScope` as a constructor argument, so the compiler rejects a call site
that has not chosen between `OwnedBy(userId)` and `Administrator()`. This exists because
every issue mutation was once missing its ownership check (architecture review F2). Do not
route around it, and prefer `Administrator()` only where an admin endpoint genuinely means
it.

Note also that not-found and not-yours deliberately return the **same** result, so the
endpoints cannot be used to probe for other users' issue ids. Keep that property.

**English for anything build- or deploy-relevant** — code, comments, commit messages, docs,
scripts. This is finding F11 of the architecture review; the repository used to mix languages
and it made the operational parts unreadable to half its potential readers.

**Architectural decisions go in `docs/architecture/`** as numbered ADRs, following
[ADR 0001](docs/architecture/0001-identity-via-authservice.md). Write one when a change
closes off an alternative someone would otherwise reasonably try — a dependency with a
licence consequence, a persistence or identity boundary, a protocol.

## Schema changes

Migrations live in per-provider projects: `Quorum.Persistence.Migrations.PostgreSQL` is the
set the deployed estate applies, and the SQL Server set is regenerated with
`scripts/generate-migrations.sh` after a model change.

**Never edit an applied migration.** They are a replay record, not source: changing one
changes what a replay produces and breaks every database built from it.

## Commits and pull requests

Commits follow Conventional Commits — `feat:`, `fix:`, `docs:`, `ci:`, `test:`, `refactor:` —
with a lowercase subject that says what changed, and a body that says *why*. Where a change
closes an architecture-review finding or an ADR, name it (`(F7, F9)`, `(ADR 0001)`).

One logical change per pull request. Say what you verified and how; if you could not verify
something, say that instead of implying you did.

## Reporting security issues

Do **not** open a public issue. See [`SECURITY.md`](SECURITY.md), which also lists what is
already known and open so you do not spend time re-reporting it.
