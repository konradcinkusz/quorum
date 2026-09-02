# Security Policy

Quorum is a citizens'-initiative platform. The asset that makes a vulnerability here matter is
not the code — it is the **signed petition documents**: real names and real signatures,
attached to a named political position, uploaded by people who did not choose this software.
Please report problems privately and give us a chance to fix them before they are public.

## Reporting a vulnerability

**Use GitHub Private Vulnerability Reporting:**
[Report a vulnerability](https://github.com/konradcinkusz/quorum/security/advisories/new)

That channel is private between you and the maintainer, and it allows a coordinated advisory
when a fix ships.

Please do **not** open a public issue for a suspected vulnerability. If private reporting is
unavailable to you for any reason, open a public issue saying only "security report, please
provide a private contact", with no details.

### What to include

- What the issue is and which component it affects — endpoint, service, or configuration.
- How to reproduce it. A `curl` sequence is ideal.
- What an attacker gains: account takeover, privilege escalation, disclosure of another
  user's issues or documents, denial of service.
- The commit you tested against, and any relevant configuration.

## What to expect

| Stage | Target |
| --- | --- |
| Acknowledgement | 7 days |
| Initial assessment | 14 days |
| Fix or a stated plan | depends on severity; you will be told which |

This is a prototype maintained by one person in their own time. Those are honest targets
rather than a commitment, and if they slip you are entitled to say so publicly.

## Supported versions

**None yet.** There are no releases and no deployed environment. `master` is the only thing
that exists, and it is where fixes land. When that changes, this section will name the
supported versions instead of explaining their absence.

## Already known — please do not re-report

These are recorded, open, and being tracked. A report about them costs you effort and tells
us nothing new. A report that one of them is *worse than documented* is very welcome.

- **Credentials in git history, unrotated.** A Cloudinary API secret and a Stripe live key
  were committed in 2023 and remain reachable in this repository's history. They are removed
  from the working tree, which does not invalidate them. Rotation is an owner action in a
  third-party console and has not happened. See
  [`docs/architecture/00-SECURITY-IMMEDIATE.md`](docs/architecture/00-SECURITY-IMMEDIATE.md)
  and [#27](https://github.com/konradcinkusz/quorum/issues/27).
- **Signed petition documents uploaded before 2026-09-02 are still public.** Delivery is
  authenticated now — new uploads are stored so their bare URL is not fetchable, and reaching
  one goes through an endpoint that checks eligibility ([#19](https://github.com/konradcinkusz/quorum/issues/19),
  review finding F6). That change does **not** reach assets already uploaded under the old
  public delivery type: those URLs stay live until someone re-uploads or deletes them in the
  Cloudinary console. It is an owner action, like the credential rotations above, and it is
  recorded in [`docs/architecture/DEVIATIONS.md`](docs/architecture/DEVIATIONS.md) §4.
- **The application has never been run against a real database.** The test suite covers pure
  logic; no endpoint has been exercised end to end. Behavioural claims about paths those
  tests do not reach are read from source rather than observed —
  [#16](https://github.com/konradcinkusz/quorum/issues/16) and
  [#17](https://github.com/konradcinkusz/quorum/issues/17) address this.

## What this project does to find problems itself

- `gitleaks` over the working tree **and full history** on every push and pull request.
- `dotnet build` of all nine projects and the test suite on every push and pull request.
- A static review against
  [`architecture-standards`](https://github.com/konradcinkusz/architecture-standards), with
  findings and their status in
  [`docs/architecture/ARCHITECTURE_REVIEW.md`](docs/architecture/ARCHITECTURE_REVIEW.md).

None of that is a penetration test, and this project has not had one.
