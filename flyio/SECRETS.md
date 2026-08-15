# Secrets

What is a secret, where it lives, and how it is set. The rule for the split: anything you
would not paste into a pull request is a secret; everything else is `[env]` in a
`fly.toml`, reviewable in a diff.

Secrets are **set from the deploy workflow, not by hand** — a secret set manually on one
app and forgotten is how environments drift. The GitHub environment `flyio` holds the
small set of root secrets; the workflow derives everything else (connection strings are
assembled from a password plus the known host, not stored per service).

## Root secrets (GitHub environment `flyio`)

| Secret | Used for |
|---|---|
| `FLY_API_TOKEN` | Deploys. Create with `fly tokens create org` |
| `POSTGRES_PASSWORD` | The `quorum` role on quorum-postgres; also derives quorum-server's connection string |
| `AUTH_DB_PASSWORD` | The `auth` role on quorum-postgres; also derives quorum-authservice's connection string |
| `AUTH_JWT_PRIVATE_KEY_PEM` | The RS256 signing key of the quorum-authservice instance — the estate's trust root |
| `AUTH_INITIAL_ADMIN_EMAIL` | Seeds the first SuperAdmin in authservice |
| `AUTH_INITIAL_ADMIN_PASSWORD` | Ditto |

Generated passwords should be hex or alphanumeric, not raw base64: `+`, `/`, `=` and `;`
all mean something inside a connection string, and the bug surfaces one rotation later.

## Per-app secrets (derived and staged by the workflow)

| App | Secret | Derived from |
|---|---|---|
| quorum-postgres | `POSTGRES_PASSWORD` | root secret, verbatim |
| quorum-postgres | `AUTH_DB_PASSWORD` | root secret, verbatim (read by the first-boot init script) |
| quorum-authservice | `ConnectionStrings__DefaultConnection` | `Host=quorum-postgres.internal;Port=5432;Database=authdb;Username=auth;Password=<AUTH_DB_PASSWORD>` |
| quorum-authservice | `Jwt__PrivateKeyPem` | root secret, verbatim |
| quorum-authservice | `InitialAdmin__Email` / `InitialAdmin__Password` | root secrets, verbatim |
| quorum-server | `ConnectionStrings__Default` | `Host=quorum-postgres.internal;Port=5432;Database=quorumdb;Username=quorum;Password=<POSTGRES_PASSWORD>` |

quorum-server deliberately has **no signing key and no shared secret** (P5): it validates
tokens against quorum-authservice's published JWKS and can verify but never mint. There is
nothing to leak on the app that faces the internet.

## The signing key

Generated, never invented — and the local development key is **not** the production key.

```bash
# macOS / Linux / Git Bash
scripts/generate-jwt-signing-key.sh
```

```powershell
# Windows PowerShell (no openssl required)
scripts/generate-jwt-signing-key.ps1
```

Either writes a 2048-bit RSA private key in PKCS#8 PEM (`BEGIN PRIVATE KEY`, not
`BEGIN RSA PRIVATE KEY` — importers care) to stdout or a file. Put the value into the
GitHub environment as `AUTH_JWT_PRIVATE_KEY_PEM`. Multi-line values work as normal quoted
arguments to `fly secrets set`, and authservice repairs `\n`-escaped newlines on read, so
any of the usual delivery routes is safe.

The public half is published at
`https://quorum-authservice.fly.dev/.well-known/jwks.json` and is not a secret in any
sense. **The deploy asserts this key set is non-empty** — where the algorithm is inferred,
a missing key silently selects HS256 and publishes a valid, empty JWKS while everything
looks healthy and every consumer rejects every token.

Rotation is rolling: move the retired public key to `Jwt__PreviousPublicKeyPem` (it stays
in the JWKS and keeps validating), sign with the new private key, and drop the old one
after one access-token lifetime. Consumers pick keys by `kid`; nothing downstream changes.

## Never reuse across systems

This instance's signing key and databases belong to Quorum's deployment alone. Reusing a
key or a database across two systems' authservice instances collapses the whole point of
independent instances: whoever can read one system's tokens could then mint the other's.
