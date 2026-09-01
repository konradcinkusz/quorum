# ADR 0002 — Replacing iTextSharp for PDF generation

**Status:** Accepted
**Date:** 2026-09-01
**Supersedes:** nothing
**Implemented by:** [#21](https://github.com/konradcinkusz/quorum/issues/21)

## Context

`Quorum.Service` depends on **iTextSharp 5.5.13.3**, which generates the sheet a winning
issue produces for signature collection. Architecture review finding **F14** flags it as
**P3, OPEN** for three independent reasons:

- **It is AGPL.** Confirmed from the package's own metadata: `iTextSharp` declares no SPDX
  licence expression and points at `https://www.gnu.org/licenses/agpl.html`. This repository
  is MIT and the Fly.io workflow distributes a container image, so the combination is a real
  conflict for anyone deploying it — not a theoretical one.
- **It is .NET Framework-era.** 5.x predates .NET Core. The project moved on to `iText 7`
  and renamed. It happens to load on `net10.0`, which is not the same as being supported.
- **It drags in a vulnerable BouncyCastle.**

### What actually has to be migrated is smaller than it looks

Reading `IssuePDFService` before choosing changed the shape of this decision.

The service has three members. Only one of them does anything that matters:

| Member | State |
|---|---|
| `GeneratePdfBytes(Issue)` | The real one. Builds a two-column table into a `MemoryStream`. |
| `GetIssuePDFFileName(Issue)` | Pure string formatting. No PDF library involved. |
| `WrtiePDFDocumentToFile(Issue)` | **Never called, and broken.** |

`WrtiePDFDocumentToFile` calls `document.Open()` and `document.Add(table)` **before**
attaching a `PdfWriter`, which is not a valid iTextSharp sequence, and writes to
`AppDomain.CurrentDomain.BaseDirectory` — inside the application directory of a container.
Its only two references are its own declaration and its own definition. It is another
instance of what the review means by *the application has still never been run*, and it
should be **deleted rather than ported**.

So the migration surface is one method producing a table of: title, question, creator email,
verification status, rating, and one row per processing-history entry. No images, no custom
fonts, no absolute positioning.

### And the sheet does not do what its name says

`GeneratePdfPTable` produces a metadata dump. **There is no signature area anywhere in it** —
no ruled lines, no name/address/date columns, nothing for a citizen to write on. The backlog
item that describes this document calls it *"PDF do podpisywania przez ludzi"* — a PDF for
people to sign.

That matters for this decision because it settles the visual-parity question below.

## Decision

**Replace iTextSharp with PDFsharp + MigraDoc.**

**Visual parity with the current sheet is explicitly not a requirement.** Reproducing a layout
that has no signature area would be reproducing the defect. The replacement should render the
same *data*, and the design of an actual signature sheet is product work — it belongs with
`BACKLOG.md` item 2, which is the same subject from the other end.

## Options considered

Licences read from each package's own metadata on 2026-09-01, not from memory.

| Option | Licence | Verdict |
|---|---|---|
| **PDFsharp + MigraDoc 6.2.4** | **MIT** | **Chosen.** Unconditional, matches this repository, no eligibility to track. MigraDoc is the document-layout layer over PDFsharp — tables, paragraphs, cells — which is a close match for what `GeneratePdfPTable` does today. |
| iText 7 (9.x) | AGPL, or commercial | Rejected. Solves the framework and BouncyCastle problems and **solves nothing about the licence**, which is the reason this ADR exists. |
| QuestPDF 2026.8.0 | Dual: Community / Professional / Enterprise | Rejected for this project — see below. |
| Render HTML and print it | n/a | Rejected. Trades a library dependency for a headless-browser dependency in the container, which is a much larger operational surface for one table. |

### Why QuestPDF is rejected despite being the nicest API

QuestPDF's Community License is free under **USD 1,000,000** annual gross revenue, which this
project comfortably satisfies today. But its eligibility rules also state, in as many words:

> Public-sector entities and government agencies — other than academic institutions covered by
> category (4) — and publicly traded companies are not eligible for the Community License,
> regardless of revenue.

**Quorum is a citizens'-initiative platform.** Its most plausible serious deployer is a public
body, a municipality, or an electoral commission — precisely the category the Community
License excludes *regardless of revenue*. Choosing it would mean that the moment this software
succeeded at its intended purpose, whoever ran it would need a paid licence, and they would
find that out after adopting it.

That is a poor property for software whose point is to be picked up and run by civic
organisations. An MIT dependency has no such cliff.

The revenue threshold is a second, weaker argument: eligibility is a moving target that has to
be re-checked, and a dependency whose licence depends on the deployer's balance sheet is a
recurring question rather than a decision.

## Consequences

- `IIssuePDFService` **does not change**. The rewrite is confined to `FilesManagement`, which
  is one of the two places the review credits this codebase with getting extension points
  right.
- `iTextSharp` leaves the dependency graph, and BouncyCastle should leave with it — to be
  confirmed rather than assumed, since another package could pull it in.
- `WrtiePDFDocumentToFile` is deleted from the interface and the implementation.
- **Existing generated documents are not regenerated.** They live on Cloudinary and are
  historical artefacts; nothing reads them back through this service.
- The repository becomes MIT-clean: after this and the `LICENSE` added in #4, there is no
  copyleft dependency in the shipped image.
- A test should assert that generation produces a valid, non-trivial PDF containing the
  issue's title. That is a weak assertion about appearance and a strong one about *it ran and
  produced something*, which is exactly the gap `WrtiePDFDocumentToFile` sat in for three
  years.

## References

- `docs/architecture/ARCHITECTURE_REVIEW.md` — F14
- `docs/architecture/DEVIATIONS.md` — D7
- [#15](https://github.com/konradcinkusz/quorum/issues/15) (this decision),
  [#21](https://github.com/konradcinkusz/quorum/issues/21) (implementation)
- `BACKLOG.md` item 2 — generating the sheet as part of choosing a winner
