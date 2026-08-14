# 00 — Security: immediate actions

> Credential exposure found while reviewing MR against
> [`architecture-standards`](https://github.com/konradcinkusz/architecture-standards).
> This document covers **all four mreferenda-lineage repositories**, not just MR, because
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

## Rotate now (P1)

| # | Secret | Repo | Location | Introduced |
|---|---|---|---|---|
| 1 | Cloudinary API key + secret (cloud `dho08…`) | **MR** | `Server/appsettings.json` — **still in HEAD** | `f0fca15`, 2023-07-18 |
| 2 | The same Cloudinary key + secret | `mreferendaInternal` | `mreferenda/Server/appsettings.json` | `103699f`, 2023-03-16 |
| 3 | Azure SQL admin password for `konradcinkusz.database.windows.net`, database `mreferenda`, user `konradcinkusz` | `mreferendaInternal` | `mreferenda/Server/appsettings.json`, full ADO.NET connection string | `f7e1663`, 2023-03-26 |
| 4 | Seed-user password (`UserPassword`) | `mreferendaInternal` | `mreferenda/Server/appsettings.json` | `7fb6849`, 2023-03-19 |
| 5 | IdentityServer signing-certificate password | `mreferendaInternal` | `mreferenda/Server/appsettings.json`, `IdentityServer:Key:Password` | `96e1fe5`, 2023-03-26 |
| 6 | `mreferenda_wasm_cert.pfx` — the signing certificate itself | `mreferendaInternal` | `mreferenda/Server/mreferenda_wasm_cert.pfx` | `96e1fe5` / `b4a6c18`, 2023-03-26 |

Rows 3 and 5–6 are the sharpest: an Azure SQL server reachable from the internet with a
committed admin password, and a signing certificate committed together with the password
that unlocks it. Both should be treated as compromised.

**Actions.**

1. **Cloudinary** — regenerate the API secret in the Cloudinary console. Check the account's
   usage/audit log for access you do not recognise before rotating, and after rotating,
   confirm the stored asset list is intact.
2. **Azure SQL** — reset the server admin password. Confirm the server is still yours and
   still exists; if it does, review its firewall rules (a committed connection string plus
   "allow Azure services" is a fully open door) and its audit log.
3. **Signing certificate** — the `.pfx` is compromised along with its password. Issue a new
   certificate; any token ever signed with the old one should be considered forgeable.
   Nothing in the current MR line uses this certificate (MR uses the development signing
   key), so this is a cleanup of a retired system rather than a live break — but the
   certificate is still valid material until it is replaced or revoked.
4. **Seeded application accounts** — `superadmin@gmail.com` / `basicuser@gmail.com` with
   password `Password@123` are seeded via `HasData` into every migration in **MR**
   (`MR.Persistence/Seeds/DefaultUser.cs:18`) and its predecessors, with
   `EmailConfirmed = true` so they bypass the confirmation gate. Delete these rows from any
   database created from these migrations, in every environment, and remove the seed. See
   [`ARCHITECTURE_REVIEW.md`](ARCHITECTURE_REVIEW.md) F4.

## Prevent recurrence (P2)

- **Add a secret scanner** — a pre-commit hook *and* a CI job. P5 is explicit that this is
  enforced mechanically, not by review, and every one of the six rows above passed a human
  review at the time. `gitleaks` or GitHub push protection both work; the point is that
  something runs on every commit.
- **Remove `Server/appsettings.json`'s secret section from MR** and move it to
  `dotnet user-secrets` locally and the platform secret store in deployment. The
  `UserSecretsId` is already declared in `Server/MR.Server.csproj:8`, so the local half
  costs one command.
- **Note the doc-comment case.** `OPEN-SOURCE-RELEASE.md` §2 flags XML doc comments as the
  most easily missed hiding place, because they explain what a value *is* and often quote
  it. `MR.Persistence/Seeds/DefaultUser.cs:18` is exactly that pattern here — a `// Password@123`
  comment above the hash. A scanner catches it; a reviewer reads past it.

## Before this repo could ever be made public (P3)

Rotation ends the exposure but does not remove the values from history. If MR is ever to be
published, the history must be rewritten (`git filter-repo`) or the repo re-created from a
squashed tree — and rotation must still happen first, because rewriting history does not
recall the clones that already exist. See
[`OPEN-SOURCE-RELEASE.md`](https://github.com/konradcinkusz/architecture-standards/blob/main/docs/guides/OPEN-SOURCE-RELEASE.md).

## Caveat

This was a targeted scan of configuration files and seed classes across four repositories,
not an exhaustive audit. Until a scanner has run over full history, assume there is more.
