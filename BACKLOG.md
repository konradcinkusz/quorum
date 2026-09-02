# Backlog

Product intent, in English, checked against the code rather than carried forward on trust.

This file replaces `TODO.txt`, which was the repository's only backlog: written in Polish in
July 2023, never maintained after it, and — as the README conceded — "not a maintained plan".
The problem was never that it was Polish. It was that several genuine product intentions
existed *only* there, in a form most people who can now read this repository cannot read, and
with no indication of which were still true.

**This is product work.** None of it is on [`ROADMAP.md`](ROADMAP.md), which covers
operability and deliberately adds no capability. The two are separate on purpose.

The original Polish text is preserved verbatim in [§ Appendix](#appendix--the-original-todotxt)
below — it is the only record of 2023 intent, and the architecture review treats provenance
as worth keeping.

## Still live

Each of these was checked against the code on 2026-09-01.

### 1. Make the cycle length configurable, so an issue need not wait a quarter

> *Zamienić kwartał na czas trwania danej kwestii (np. miesiąc), żeby np. w przeciągu tygodnia
> roztrzygać kwestie*

The quarter is currently structural: `Quarter`, `QuarterIssue`, `QuarterExtensions` and
`ChooseTheWinnerOfCurrentQuarter` all assume one. Making the resolution window a property of
an issue — a month, a week — is the largest product change in this list, and it changes the
signature-pool model with it, since pools are allocated per quarter in `InitQuarterCommand`.

### 2. Generate the signing PDF as part of choosing the winner

> *Podczas wybierania kwartału (zwycięzcy) należy również utworzyć PDF do podpisywania przez
> ludzi*

**Still open, and the code says so.** `ChooseTheWinnerOfCurrentQuarter.cs` carries a comment
at the point where this belongs — *Utworzenie pliku PDF do podpisywania przez zainteresowanych*
— with nothing beneath it. PDF generation exists, as `GeneratePDFForAnIssueCommand`, but it is
a separate call rather than part of resolving a quarter, so a winner today has no signing
sheet until someone asks for one.

Note that #21 replaces the PDF library, so this is best done after it rather than twice.

### 3. Publishing: button visibility

> *Proces publikowania - widoczności przycisków*

A client-side concern in `Client/Pages`. Vague as written, and it needs restating as what a
user should see at each `IssueProcess` step before it is actionable.

### 4. A "complete" step on the sign/submitted list page

> *Dodanie procesu do list page sign and submitted (tzn. użytkownik ma mieć możliwość
> kliknięcia complete, po czym nie będzie mógł nanosić nowych zmian)*

A user marks their submission complete and can no longer change it. This is a state
transition, so it belongs in `IssueProcess` rather than in the client alone.

### 5. Subscription types, with document-backed account verification

> *Różne typy subskrypcji - potwierdzenia konta za pomocą uploadowanego dokumentu*

`SubscriptionFeatures` has buy, activate, deactivate, refund and reject, but one kind of
subscription. Verifying an account against an uploaded document also intersects
[#19](https://github.com/konradcinkusz/quorum/issues/19): it would put a second class of
personal document into the same storage path whose delivery is not yet authenticated. Do not
start this before #19.

### 6. Long term: documents signed with a trusted signature

> *Uploadowanie dokumentów podpisanych podpisem zaufanym*

Signatures under Poland's *podpis zaufany* / ePUAP scheme, rather than a scan of a wet
signature. A regulatory question before it is a technical one, and out of reach until the
storage path is sound.

### 7. Long term: decide which issue fields are public

> *Sprawdzić które wartości mają iść do publiczności z query od issue*

**Partly addressed, and worth finishing deliberately.** The architecture review's F3 deleted
`get-issues-by-search-params`, an authenticated listing that returned every user's private
drafts and contact details. What remains is the narrower question the original item asked:
of the fields a *public* query returns, which should be there at all. No one has been through
`PublicPublishedEndedIssueRead` and the other public projections field by field.

## Done since the list was written

- **Editing issues** (*Dodanie edycji*) — `EditIssueCommand` exists. Note
  [#18](https://github.com/konradcinkusz/quorum/issues/18): an edit currently clears admin
  verification as a side effect.
- **Document upload** (*Uploadowanie dokumentów w ogóle*) — `UploadSignedDocumentCommand` plus
  the Cloudinary pipeline. Upload is validated and eligibility-checked; delivery is not
  authenticated, which is [#19](https://github.com/konradcinkusz/quorum/issues/19).
- The four items already under `DONE:` in the original, preserved below.

## Appendix — the original TODO.txt

Verbatim, as it stood in July 2023.

```
In progress:

TODO:
Różne typy subskrypcji - potwierdzenia konta za pomocą uploadowanego dokumentu
Proces publikowania - widoczności przycisków
Zamienić kwartał na czas trwania danej kwestii (np. miesiąc), żeby np. w przeciągu tygodnia roztrzygać kwestie
Dodanie edycji
Dodanie procesu do list page sign and submitted (tzn. użytkownik ma mieć możliwość kliknięcia complete, po czym nie będei mógł nanosić nowych zmian)
Podczas wybierania kwartału (zwycięzcy) należy również utworzyć PDF do podpisywania przez ludzi

Long Term TODO:
Uploadowanie dokumentów w ogóle (w sensie że użytkownik wgrywa do nas dokument)
Uploadowanie dokumentów podpisanych podpisem zaufanym
Sprawdzić które wartości mają iść do publiczności z query od issue

DONE:
Dodać (pokazywać) QuarterYear i QuarterNumber do PublicPublishedEndedIssueRead w cely prezentacji kwartału na Issue.Winners.ListPage
Dodanie roztrzygnięcia kwartału
Dodać obsługę publikowania kwestii
Poprawić kontroler public async Task<IActionResult> EditPayment([FromRoute] Guid id, [FromBody] PaymentUpdateDTO paymentDto)
dodać cancellation tokeny do cloudinary service
Uzupełnić w nav menu: Upload real sign oraz Assigned Issues
Lista podpisanych kwestii, które zostały zakończone w danym kwartale i czekają na przesłania podpisu (Your Winners)
```

## A note on the rest of the Polish prose

`TODO.txt` was the largest piece of untranslated prose, not the last one. Around a dozen
Polish comments remain in the C# sources — in `ChooseTheWinnerOfCurrentQuarter`,
`InitQuarterCommand`, `DeleteQuarterCommand`, `ForceDeleteIssueCommand`, `CloudinaryService`,
`Payment` and a few DTOs. Architecture review finding F11 asks for English in
build- and deploy-relevant prose; those comments explain domain rules rather than build steps,
and translating them is a separate pass that should be done by someone who can check the
domain meaning rather than the words.
