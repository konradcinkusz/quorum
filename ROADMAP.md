# Quorum — Roadmap

> The plan this repository is being taken through, and the reasoning behind its order.
> Tracked in [#22](https://github.com/konradcinkusz/quorum/issues/22), which carries the
> running log and the decision log.
>
> **Written:** 2026-09-01, against `master` @ `d88cc93`.

## 1. What "complete" means here

Quorum is a feature-complete prototype whose gap is **operability, not features**. That is
not a guess; it is what
[`docs/architecture/ARCHITECTURE_REVIEW.md`](docs/architecture/ARCHITECTURE_REVIEW.md)
measured, and it is worth quoting because it sets the whole shape of this plan:

> What this repo gets right is *code structure*, and what it lacks entirely is *operability*.
> Layering, vertical slices, anti-corruption, migrations and a manifest `Program.cs` are all
> in place — the parts an experienced developer writes. Health, telemetry, containers, CI,
> tests and secret handling are absent — the parts a repo grows when someone other than its
> author has to run it.

Since that was written the second list has shrunk: health, telemetry, a container, CI, a
secret scanner and a test project all exist, and identity moved out to `authservice` under
[ADR 0001](docs/architecture/0001-identity-via-authservice.md). What remains is this
roadmap. Complete means:

1. **A stranger can legally use, run and contribute to it.** Today the repository has no
   licence at all, which means exclusive copyright — nobody may legally run it, including
   anyone invited to contribute. It also has no disclosure route, no contribution guide and
   no code of conduct, all of which the four sibling repositories in this estate carry.
2. **The supply chain is current and watched.** No known-vulnerable or end-of-life package,
   versions pinned in one place, and an automated bump loop — rather than a human reading
   `.csproj` files once a year, which is how the two open advisories were found.
3. **It matches the architecture it was measured against.** Every remaining ❌ or ⚠️ on the
   review's compliance checklist is either closed or recorded in a dated deviation register
   with a reason and an exit.
4. **There is evidence it runs, not merely that it compiles.** This is the largest standing
   risk and the review states it plainly: at the time of the review the 65 tests covered pure
   logic, and *the application has still never been run*. It is 96 now and a booted host is
   among them, so what is left of the gap is a real database rather than the whole of it. F6's `FindAsync` bug is the standing proof of what that hides —
   it compiled cleanly for three years and threw on first execution, meaning that endpoint
   could never have worked.
5. **Security findings are closed rather than mitigated.** Signed petition documents — real
   names and signatures — are behind authenticated delivery rather than an unguessable public
   URL, and secrets are stopped before the commit exists rather than after it is pushed.
6. **The plan outlives the session.** This file, an English backlog, and issues a cold reader
   can pick up.

Two things deliberately **not** in that list are covered under [§7](#7-non-goals).

## 2. Phases

Two-week phases, inferred from a solo maintainer and a burst-shaped commit history rather
than from a stated cadence.

| Phase | Target | Issues | Goal |
|---|---|---|---|
| **1 — Governance and supply chain** | 2026-09-15 | [#4](https://github.com/konradcinkusz/quorum/issues/4) [#5](https://github.com/konradcinkusz/quorum/issues/5) [#6](https://github.com/konradcinkusz/quorum/issues/6) [#7](https://github.com/konradcinkusz/quorum/issues/7) [#8](https://github.com/konradcinkusz/quorum/issues/8) [#9](https://github.com/konradcinkusz/quorum/issues/9) [#10](https://github.com/konradcinkusz/quorum/issues/10) | Make the repository legally and socially usable, and start watching dependencies. Touches no code. |
| **2 — Operability and architecture debt** | 2026-09-29 | [#11](https://github.com/konradcinkusz/quorum/issues/11) [#12](https://github.com/konradcinkusz/quorum/issues/12) [#13](https://github.com/konradcinkusz/quorum/issues/13) [#14](https://github.com/konradcinkusz/quorum/issues/14) [#15](https://github.com/konradcinkusz/quorum/issues/15) | Close the open High-severity advisory, the resilience gap on the synchronous auth path, and open the deviation register. |
| **3 — Prove it runs** | 2026-10-13 | [#16](https://github.com/konradcinkusz/quorum/issues/16) [#17](https://github.com/konradcinkusz/quorum/issues/17) [#18](https://github.com/konradcinkusz/quorum/issues/18) | Boot the application in a test, cover the IDOR fix against regression, and fix the defect that was waiting on a test. |
| **4 — Security close-out** | 2026-10-27 | [#19](https://github.com/konradcinkusz/quorum/issues/19) [#20](https://github.com/konradcinkusz/quorum/issues/20) [#21](https://github.com/konradcinkusz/quorum/issues/21) | Authenticated delivery for petition documents, secrets stopped at commit time, iTextSharp gone. |

Phases are **labels** — `phase-1-governance`, `phase-2-operability`, `phase-3-verification`,
`phase-4-security` — rather than GitHub milestones, because milestone creation was not
available to the automation that wrote this plan. Nothing depends on the distinction: the
grouping, the ordering and the "phase exhausted, move to the next" rule all work off the
label. Anyone who prefers real milestones can create four and bulk-assign by label.

## 3. Why this order

**Risk rises as confidence does, and confidence has to be built first.**

The constraint that shapes everything is that **there is no .NET SDK on the machine doing
this work**. Nothing can be compiled or tested locally; CI is the only verifier, and each
pull request gets three fix attempts before the policy in [§6](#6-execution-policy) applies.
That makes an untested code change genuinely expensive, and it argues for spending the early
phases on work whose blast radius is knowable by reading.

So:

- **Phase 1 changes no code.** Licence, disclosure, contribution docs, Dependabot, CodeQL,
  an `.editorconfig` that declares style without enforcing it. Every one of these is verified
  by CI continuing to pass rather than by CI proving something new, and none can break the
  build. It is also the phase that unblocks other people, which is worth doing first on its
  own merits.
- **Phase 2 is small, contained code changes** with a clear failure mode: a dependency bump
  that fails at compile time and names the file, a resilience handler on one registration, and
  two documents. The register (#14) belongs here rather than in Phase 1 because it can only be
  written accurately once the Phase 2 decisions are made.
- **Phase 3 builds the instrument.** Everything before it is verified by "CI still compiles";
  from here on there is a running application to assert against. This is deliberately placed
  before Phase 4 rather than after, because Phase 4's headline item was *deferred by the
  review for exactly this reason* — F6 "cannot be verified without running the application,
  which nothing in this repository can do yet". Phase 3 is what makes that stop being true.
- **Phase 4 is the work that needed all of it.** The authenticated-delivery change rewrites a
  read path with stored data behind it; doing it without Phase 3's fixture would be changing
  security-critical behaviour with no way to observe the result.

The one ordering that is about tidiness rather than risk: #13 waits for #11 so that a
security bump is not entangled in a mechanical rewrite of ten project files.

## 4. Dependencies

Every `Blocked by` in one place.

| Issue | Blocked by | Why |
|---|---|---|
| [#13](https://github.com/konradcinkusz/quorum/issues/13) Central Package Management | [#11](https://github.com/konradcinkusz/quorum/issues/11) | The AutoMapper bump should land against the current per-project layout, so a security fix and a restructuring of every `.csproj` are separately reviewable and separately revertable. |
| [#17](https://github.com/konradcinkusz/quorum/issues/17) ownership regression test | [#16](https://github.com/konradcinkusz/quorum/issues/16) | #16 builds the `WebApplicationFactory` host that #17 asserts against. |
| [#18](https://github.com/konradcinkusz/quorum/issues/18) `IsVerifyByAdmin` default | [#17](https://github.com/konradcinkusz/quorum/issues/17) | The review deferred this one-character fix explicitly until "there is a test to prove what the publish flow expects". |
| [#19](https://github.com/konradcinkusz/quorum/issues/19) authenticated delivery | Phase 3 ([#16](https://github.com/konradcinkusz/quorum/issues/16), [#17](https://github.com/konradcinkusz/quorum/issues/17)) | Not a formal block, a practical one: this changes a security-critical read path and needs somewhere to assert that an ineligible user is refused. |
| [#21](https://github.com/konradcinkusz/quorum/issues/21) replace iTextSharp | [#15](https://github.com/konradcinkusz/quorum/issues/15) | #15 decides *what* replaces it, and the constraint is a licence one. Choosing while coding is how that gets discovered after the rewrite. |

Everything else is parallel-safe.

## 5. Protected paths

Files whose breakage compromises every later pull request. A change touching one of these is
never force-merged past a failing check — see [§6](#6-execution-policy).

| Path | Why |
|---|---|
| `.github/workflows/*.yml` | `build.yml` is the only thing in existence that compiles this repository. `secret-scan.yml` is the only thing that watches for credentials, and this repository's history is the argument for it. |
| `global.json`, `Quorum.sln`, every `*.csproj` | The build graph. A broken restore fails every subsequent branch identically, and with no local SDK the diagnosis is a CI round trip. |
| `flyio/*.toml`, `Server/Dockerfile` | The deployment artefact. Breakage here surfaces at tag time, not at pull-request time, which is the worst place to find it. |
| `.gitleaks.toml` | Shared by CI and (from #20) the pre-commit hook. A config change silently widens what both will accept. |
| `Quorum.Persistence.Migrations.*` | A replay record rather than source. Editing an applied migration rewrites history that a deployed database has already acted on. |

## 6. Execution policy

- **One issue, one pull request**, with `Closes #N`. Never batched.
- **CI must actually run** on the pushed branch. A diff that "would obviously pass" is not a
  passing diff — particularly here, where nothing is compiled before it is pushed.
- **Three fix attempts per pull request.** The number is fixed, not renegotiated mid-run.
- After three failures:
  - the diff **touches no protected path** → force-merge, say so explicitly in the pull
    request, and open a `Fix CI:` follow-up carrying the last failure excerpt, labelled
    `tech-debt`;
  - the diff **touches a protected path** → do not merge. Leave the pull request open, label
    it and its issue `blocked`, and record the diagnosis. A broken content file is contained;
    a broken build workflow turns every later pull request into three-retries-and-force-merge,
    which is CI ceasing to exist.
- **Infrastructure failures** — auth, quota, runner outage, anything whose log shows no code
  path — do not consume the retry budget. Re-run once; if it persists, force-merge and open a
  single pipeline issue rather than one per pull request.
- Tracking issue: [#22](https://github.com/konradcinkusz/quorum/issues/22).

## 7. Non-goals

Named because leaving them unnamed makes them look forgotten.

- **Features.** Nothing in this roadmap adds a capability. The 2023 backlog — variable cycle
  length, subscription types, document upload flows — is product work. #10 moved it into
  [`BACKLOG.md`](BACKLOG.md), in English and checked against the code, rather than acting on
  it.
- **Rotating the leaked credentials.** The Stripe and Cloudinary keys and the
  `mreferendaInternal` database password are **owner actions**: they live in third-party
  consoles, no commit can close them, and they stay valid until someone rotates them. They
  are tracked in [`00-SECURITY-IMMEDIATE.md`](docs/architecture/00-SECURITY-IMMEDIATE.md) and
  will appear in the deviation register (#14) so they stay visible. This roadmap cannot do
  them.
- **Rewriting git history** to remove those secrets. Disruptive, and only strictly required
  before the repository goes public.
- **A payments audit.** The review flags that `docs/stripe.txt` held a *live* key, implying a
  real account taking real money against an integration this codebase does not contain. Where
  that lives is an open question a roadmap cannot answer.
- **Running a deployed environment.** The estate deploys from a tag and no environment is
  live. Phase 3 proves the application starts in a test host, which is not the same claim and
  should not be read as one.
