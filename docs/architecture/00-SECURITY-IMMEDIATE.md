# 00 — Security: immediate actions

> Credential exposure found while reviewing Quorum against
> [`architecture-standards`](https://github.com/konradcinkusz/architecture-standards).
> This document covers **all four mreferenda-lineage repositories**, not just Quorum, because
> the credentials are shared between them and rotating one without the others leaves the
> account exposed.
>
> **Found:** 2026-08-14. **Method:** `git log --all -p` over `*appsettings*.json` and the
> seed classes in each repo — HEAD alone is not sufficient
> ([`OPEN-SOURCE-RELEASE.md`](https://github.com/konradcinkusz/architecture-standards/blob/main/docs/guides/OPEN-SOURCE-RELEASE.md)
> §2: "a clean HEAD says nothing about commit 4 of 200").

**Values are redacted below.** Each row gives the repository, path and commit so the
credential can be located and rotated without this document becoming a second copy of it.

## Status

All four repositories are **private** (verified via the GitHub API, 2026-08-14). That bounds
the exposure to anyone who has or had read access — but it does not change the required
action. A credential that has been committed is a credential that has left your control:
it is present in every clone, every fork, every IDE workspace backup and every CI cache
that ever touched the repo. Rotation is the only thing that ends the exposure.

## What the 2026-08-15 commit did and did not do

The code half of this document is done; the operational half is not, and only you can do it.

**Done in the repository:** the Cloudinary section is gone from `Server/appsettings.json`
and is now read from user-secrets or the environment; the seeded `superadmin@gmail.com` and
`basicuser@gmail.com` accounts are removed from the model, and migration
`20260815000000_RemoveSeededIdentityAccounts` deletes them and their role mappings from
existing databases.

**Not done, and not doable from here:**

- **Every rotation in the table below.** Removing a secret from the working tree does not
  invalidate it. Until you rotate, the Cloudinary credentials in commit `f0fca15` still
  authenticate, and so does everything in `mreferendaInternal`'s history.
- **Applying the migration.** Nothing in this repo applies migrations at startup, so
  `dotnet ef database update` has to be run against each environment that was ever created
  from these migrations. Until it runs, the known-password superadmin still exists there.
- **Rewriting history.** All six rows below remain in git history after rotation.

## 🚨 Roll this first

**A Stripe *live* secret key (`sk_live_…`) was committed to `docs/stripe.txt`** in `e43abc8`
(2023-06-05) and sat in `HEAD` until it was deleted on 2026-08-15. It was also listed as a
solution item in `Quorum.sln`, so it opened in Visual Studio's Solution Explorer.

This outranks everything else in this document. A live Stripe secret key is not a
credential for *this* application — it is full API access to the Stripe **account**: create
charges, issue refunds, read every customer record and payment method, and on most account
configurations move money out. It does not expire, and it is not scoped to an environment.

**Do this before reading further:**

1. Roll the key in the Stripe dashboard (Developers → API keys → roll the live secret key).
2. Review the account's [Events and logs](https://dashboard.stripe.com/logs) for API calls
   you do not recognise, going back to 2023-06-05. That is a ~3-year exposure window.
3. Check for unexpected refunds, transfers, payouts and newly-created API keys or webhooks.

**How it was found, and why that matters.** Not by the architecture review, and not by a
manual search — this document's first version listed six credentials and missed this one,
because a hand-written grep looks for the patterns its author thought of (`ApiSecret`,
`Password=`, `ConnectionString`) and `sk_live_…` in a stray notes file was not one of them.
The `secret-scan` workflow found it on its first run, in four seconds. That is the entire
argument for P5's "enforced by a scanner, not by review", made at this repository's expense.

**Assume there are more.** The scanner now runs on every push; the two `gitleaks` steps and
a short, dated allowlist in `.gitleaks.toml` are what keep that true.

## Rotate now (P1)

| # | Secret | Repo | Location | Introduced |
|---|---|---|---|---|
| 0 | **Stripe live secret key** — see above | **Quorum** | `docs/stripe.txt` (deleted 2026-08-15) | `e43abc8`, 2023-06-05 |
| 1 | Cloudinary API key + secret (cloud `dho08…`) | **Quorum** | `Server/appsettings.json` — **still in HEAD** | `f0fca15`, 2023-07-18 |
| 2 | The same Cloudinary key + secret | `mreferendaInternal` | `mreferenda/Server/appsettings.json` | `103699f`, 2023-03-16 |
| 3 | Azure SQL admin password for `konradcinkusz.database.windows.net`, database `mreferenda`, user `konradcinkusz` | `mreferendaInternal` | `mreferenda/Server/appsettings.json`, full ADO.NET connection string | `f7e1663`, 2023-03-26 |
| 4 | Seed-user password (`UserPassword`) | `mreferendaInternal` | `mreferenda/Server/appsettings.json` | `7fb6849`, 2023-03-19 |
| 5 | IdentityServer signing-certificate password | `mreferendaInternal` | `mreferenda/Server/appsettings.json`, `IdentityServer:Key:Password` | `96e1fe5`, 2023-03-26 |
| 6 | `mreferenda_wasm_cert.pfx` — the signing certificate itself | `mreferendaInternal` | `mreferenda/Server/mreferenda_wasm_cert.pfx` | `96e1fe5` / `b4a6c18`, 2023-03-26 |

Row 0 is the one that matters most. After it, rows 3 and 5–6 are the sharpest: an Azure SQL
server reachable from the internet with a committed admin password, and a signing
certificate committed together with the password that unlocks it. Both should be treated as
compromised.

**Actions.**

0. **Stripe** — roll the live secret key and audit the account's event log, as set out above.
1. **Cloudinary** — regenerate the API secret in the Cloudinary console. Check the account's
   usage/audit log for access you do not recognise before rotating, and after rotating,
   confirm the stored asset list is intact.
2. **Azure SQL** — reset the server admin password. Confirm the server is still yours and
   still exists; if it does, review its firewall rules (a committed connection string plus
   "allow Azure services" is a fully open door) and its audit log.
3. **Signing certificate** — the `.pfx` is compromised along with its password. Issue a new
   certificate; any token ever signed with the old one should be considered forgeable.
   Nothing in the current Quorum line uses this certificate (Quorum uses the development signing
   key), so this is a cleanup of a retired system rather than a live break — but the
   certificate is still valid material until it is replaced or revoked.
4. **Seeded application accounts** — `superadmin@gmail.com` / `basicuser@gmail.com` with
   password `Password@123` are seeded via `HasData` into every migration in **Quorum**
   (`Quorum.Persistence/Seeds/DefaultUser.cs:18`) and its predecessors, with
   `EmailConfirmed = true` so they bypass the confirmation gate. Delete these rows from any
   database created from these migrations, in every environment, and remove the seed. See
   [`ARCHITECTURE_REVIEW.md`](ARCHITECTURE_REVIEW.md) F4.

## Prevent recurrence (P2)

- **Add a secret scanner** — a pre-commit hook *and* a CI job. P5 is explicit that this is
  enforced mechanically, not by review, and every one of the six rows above passed a human
  review at the time. `gitleaks` or GitHub push protection both work; the point is that
  something runs on every commit.
- **Remove `Server/appsettings.json`'s secret section from Quorum** and move it to
  `dotnet user-secrets` locally and the platform secret store in deployment. The
  `UserSecretsId` is already declared in `Server/Quorum.Server.csproj:8`, so the local half
  costs one command.
- **Note the doc-comment case.** `OPEN-SOURCE-RELEASE.md` §2 flags XML doc comments as the
  most easily missed hiding place, because they explain what a value *is* and often quote
  it. `Quorum.Persistence/Seeds/DefaultUser.cs:18` is exactly that pattern here — a `// Password@123`
  comment above the hash. A scanner catches it; a reviewer reads past it.

## Before this repo could ever be made public (P3)

Rotation ends the exposure but does not remove the values from history. If Quorum is ever to be
published, the history must be rewritten (`git filter-repo`) or the repo re-created from a
squashed tree — and rotation must still happen first, because rewriting history does not
recall the clones that already exist. See
[`OPEN-SOURCE-RELEASE.md`](https://github.com/konradcinkusz/architecture-standards/blob/main/docs/guides/OPEN-SOURCE-RELEASE.md).

## Caveat

This was a targeted scan of configuration files and seed classes across four repositories,
not an exhaustive audit. Until a scanner has run over full history, assume there is more.
