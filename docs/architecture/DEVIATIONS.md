# Deviations from the reference architecture

> Opened 2026-09-01, discharging the last residual risk in
> [`ARCHITECTURE_REVIEW.md`](ARCHITECTURE_REVIEW.md) §4:
>
> > **No deviation register (§3a) is opened yet.** Once the P1 items are fixed, the remainder
> > should move into `docs/architecture/DEVIATIONS.md` with dates, so what stays open stays
> > visible.
>
> Measured against
> [`architecture-standards`](https://github.com/konradcinkusz/architecture-standards) — the 15
> principles and the compliance checklist.

**Why this file exists.** The review is a good gap analysis and a poor standing record: knowing
what is still open means reading fifty kilobytes and correctly working out which rows of a
table are live. An open deviation nobody re-reads becomes an accepted one by default. Every row
here has a reason and an exit, and a row with no exit is an accepted deviation wearing a
temporary one's clothes.

**The review is dated 2026-08-14 and several of its ❌ marks are no longer true.** It predates
the authservice cutover, the Dockerfile and the Fly.io workflow. Where that is the case this
file says so rather than restating a stale finding — see [§4](#4-closed-since-the-review).

---

## 1. Structural, and probably permanent

| # | Deviation | Why | Exit |
|---|---|---|---|
| D1 | **No AppHost**; services are not declared with `WithReference`/`WaitFor` (checklist 1) | The reference architecture's composition model assumes several services orchestrated together. Quorum is one service plus a version-pinned `authservice` image it does not own. There is nothing to compose. | None intended. Revisit only if a second first-party service appears. |
| D2 | **No shared kernel**, so `AddServiceDefaults()` is not called (checklist 2) | Same reason. A kernel shared between one service and itself is indirection. Note that `MapDefaultEndpoints()` **does** exist, implemented locally in `HealthCheckExtensions` — the endpoints the checklist wants are present; the shared package is not. | None intended. |

## 2. Accepted, with a reason that is better than the rule

| # | Deviation | Why | Exit |
|---|---|---|---|
| D3 | **The Cloudinary fallback throws rather than no-ops** (checklist 10, P8) | P8 asks optional integrations to degrade, usually via a no-op that logs. The files crossing `ICloudinaryService` are wet-signature petition documents. A stub that accepted an upload and discarded it would tell a citizen their signature sheet was stored when it had been thrown away. `CloudinaryNotConfiguredService` throws with an explanatory message; the application still starts and every other feature still works, which is the part of P8 that matters. | None. This is the correct behaviour for this interface. |

## 3. Open, with an issue against them

| # | Deviation | Why it is still here | Exit |
|---|---|---|---|
| D4 | **Signed petition documents are delivered publicly** (F6, open half) | Upload is validated, size-capped and eligibility-checked, and the stored name carries 256 bits from a CSPRNG — but delivery is unauthenticated, so that name is a **share token, not an access control**, and `SECURITY-REVIEW.md` §5 is explicit that an unguessable identifier is not a secret. The review calls this *mitigation, not a fix*. It was deferred because changing the read path breaks every stored `SecureUri` and could not be verified without running the application. | [#19](https://github.com/konradcinkusz/quorum/issues/19) |
| D5 | **The handler inheritance chain is inheritance** (checklist 15) | `CommandHandlerBase` → `IssueCommandHandlerBase` are base classes where the checklist asks for interfaces registered in DI. The review is pointed about the cost: the shared base *"looked like a policy enforcement point and was not one"*, which is how F2 — no ownership check on any issue mutation — happened. `IssueOwnerScope` now makes the check structural, so the risk is contained rather than removed. | Not yet scheduled. Contained by `IssueOwnerScope`; the register is the reminder. |
| D6 | **Outbound `HttpClient`s carry no resilience handler** (checklist 13) | `AuthServiceGateway` proxies login, registration and refresh to the external identity instance, on the synchronous request path. It sets a 30 s timeout for cold start and disables auto-redirect, both deliberately — what it lacks is retry and circuit breaking. The Blazor client's clients are out of scope: they run in the browser, where the handler stack is the browser's. | [#12](https://github.com/konradcinkusz/quorum/issues/12) |
| D7 | **`iTextSharp` 5.x is AGPL, in an MIT repository that ships a container image** | Also .NET Framework-era and it drags in a vulnerable BouncyCastle (F14). | [#15](https://github.com/konradcinkusz/quorum/issues/15) decides the replacement, [#21](https://github.com/konradcinkusz/quorum/issues/21) does it. |
| D8 | **The application has never been run** | The test suite is 65 tests of pure logic. Nothing exercises the database, the HTTP pipeline, a migration against a real schema, or the health endpoints. Every behavioural claim about those paths is read from source rather than observed. F6's `FindAsync` bug is the standing evidence: it compiled cleanly for three years and threw the first time it executed. | [#16](https://github.com/konradcinkusz/quorum/issues/16), [#17](https://github.com/konradcinkusz/quorum/issues/17) |
| D9 | **Secrets are caught after the commit, not before** | `secret-scan` runs on push and pull request, so by the time it fires the credential is already in a branch's history on GitHub. | [#20](https://github.com/konradcinkusz/quorum/issues/20) |

## 4. Owner actions — no commit can close these

These are not code deviations. They are open risks with no other home, and they are the reason
this file leads with them rather than burying them.

| # | Action | Status |
|---|---|---|
| O1 | **Roll the Stripe live secret key** and audit that account's event log back to 2023-06-05 | Open. The review calls it *"the single most urgent row in this document"* — written when the repository was believed private. |
| O2 | **Rotate the Cloudinary key and secret** | Open. Removed from the working tree 2026-08-15; still valid, and still in history. |
| O3 | **Rotate the `mreferendaInternal` Azure SQL password and signing certificate** | Open. See [`00-SECURITY-IMMEDIATE.md`](00-SECURITY-IMMEDIATE.md). |
| O4 | **Decide what to do about history** — rotation ends the exposure, it does not remove the values | Open. |
| O5 | **Re-check the visibility of the other three lineage repositories** | Open. Quorum was verified **public** on 2026-09-01; the other three were last checked on 2026-08-14 and the record for them has not been revisited. |

**The escalation that changed all five.** `00-SECURITY-IMMEDIATE.md` bounded the exposure to
"anyone who has or had read access", on the basis that all four repositories were private. Quorum
is public, and was made public without the precondition that document sets — history rewritten,
rotation first — being met. The credentials above are world-readable.
See [#27](https://github.com/konradcinkusz/quorum/issues/27).

## 5. Closed since the review

Recorded so that a reader of the 2026-08-14 review does not act on a stale ❌.

| Checklist item | Then | Now |
|---|---|---|
| 3 — `/health` and `/alive` | ❌ F7 | ✅ Both mapped; `/alive` deliberately independent of the database. |
| 4 — OTLP traces, metrics, logs | ❌ F9 | ✅ OpenTelemetry, with the probe paths filtered out. |
| 6 — schema applied by a hosted service | ⚠️ applied manually | ✅ `MigrationBackgroundService`, with readiness reporting 503 until it lands. |
| 7 — no secret in source; scanner in CI | ❌ F1, F5 | ✅ …with the caveat that the scanner was **failing at config load from 2026-08-15 to 2026-09-01** and scanning nothing ([#24](https://github.com/konradcinkusz/quorum/issues/24)). |
| 11 — multi-stage Dockerfile | ❌ F8, F12 | ✅ `Server/Dockerfile`, on .NET 10. |
| 12 — one `fly.toml` per app | ❌ F8 | ✅ `flyio/`. |
| 16 — a test project covering the logic-bearing layer | ❌ F10 | ✅ 65 tests — **of pure logic only**, which is D8. |
| 17 — tag-driven workflow with change detection | ❌ F8 | ✅ `flyio.yml`. |
| 18 — decisions recorded in `docs/` | ⚠️ the review was the first | ✅ ADR 0001, and this register. |

---

## Keeping this file honest

- A row moves to §5 when the thing is *done*, not when an issue is opened for it.
- A row with no exit belongs in §1 or §2, where "none intended" is the exit and the reason has
  to carry it.
- When the review and this file disagree, **this file is the current one** — the review
  deliberately describes the code as reviewed and is not rewritten as things change.
