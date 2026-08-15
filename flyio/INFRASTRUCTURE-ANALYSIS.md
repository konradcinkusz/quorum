# Infrastructure analysis

Topology, sizing and cost reasoning for Quorum's Fly.io estate (P7). Three apps, one
region (`waw`), deployed by the tag-driven workflow in
[`.github/workflows/flyio.yml`](../.github/workflows/flyio.yml).

```
browser ──► quorum-server (API + BFF + Blazor client)
                │  bearer via HttpOnly cookie bridge; JWKS validation
                │  BFF login/refresh proxy
                ▼
            quorum-authservice (ghcr.io/konradcinkusz/authservice, pinned)
                │
                ▼
            quorum-postgres (quorumdb + authdb, 6PN only)
```

## 1. What runs when nothing is happening?

| App | Machines when idle | Memory | Why |
|---|---|---|---|
| quorum-postgres | 1 | 1 GB | Databases never scale to zero; volume-pinned |
| quorum-authservice | 1 | 512 MB | Pinned — see §2 |
| quorum-server | 0 | 512 MB | Browser-entered; wakes on request |

Idle footprint: two shared-cpu-1x machines plus a 10 GB volume.

## 2. Which services pin a machine, and which synchronous call forces it?

- **quorum-authservice pins one machine.** Two named in-request calls force it:
  quorum-server fetches and periodically refreshes this instance's **JWKS** to validate
  every bearer token, and the **BFF proxies login/refresh/register** to it while a user
  waits. A cold start there is a failed or multi-second sign-in, not a slow page.
- **quorum-postgres pins one machine** by nature: `.internal` addresses bypass the proxy,
  so nothing could wake it, and databases do not cold-start well.
- **quorum-server scales to zero.** It is entered only from a browser; the first page
  after idle pays the cold start. Nothing calls it synchronously from inside the estate.

## 3. What is the cheaper option and what does it actually cost?

Letting quorum-authservice scale to zero would save one shared-cpu-1x machine, at the
price of a cold boot in the middle of every first sign-in after idle **and** of the JWKS
refresh path of the API. `min_machines_running = 1` is the decision; revisit only if the
estate gains a cheaper always-on tier.

Physical co-location of `quorumdb` and `authdb` on one Postgres instance is the accepted
cost decision (P3 allows it; the shared-service pattern's reconciliation states it for
this exact case). The logical boundary holds: separate databases, separate roles, no
cross-grants — each service holds credentials for exactly one database, so moving authdb
to its own instance later is a configuration change.

## 4. What is off the table?

- **A public listener on quorum-postgres.** It is reached only over 6PN; reach it from a
  laptop with `fly proxy 15432:5432 --app quorum-postgres`.
- **Turning off `force_https`.**
- **Sharing the signing key or a database with any other system's authservice
  instance.** Independent instance means independent trust root; see
  `flyio/SECRETS.md`.
- **Tracking `:latest` of the authservice image.** The pin (`v0.3.1` today) is bumped
  deliberately, in a reviewed diff, after reading the release notes — the HTTP contract
  is 0.x and may move. Note the pinned instance keeps its default
  `Database:SchemaMode=EnsureCreated`: fine to bootstrap an empty authdb, but a version
  bump that changes the identity schema needs authservice's own upgrade guidance
  (`docs/schema/` in that repo) before the pin moves. Recorded as an accepted, tracked
  deviation in ADR 0001.
- **CORS on quorum-authservice.** The browser never calls it; the BFF is the only
  caller. Adding origins would only widen the attack surface.

## 5. Registry

Server images are built once per tag and pushed to `registry.fly.io/quorum-server:<tag>`;
every deploy references the image rather than rebuilding. The authservice image comes
from GHCR, already built and multi-arch, published by that repository's own release
workflow — building it here would recreate the source-level coupling the shared-service
pattern exists to avoid.
