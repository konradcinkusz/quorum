using Quorum.Domain.Entities;
using Quorum.Domain.Enums;
using Quorum.Service.Features.Issues.Base;
using Xunit;

namespace Quorum.Tests;

/// <summary>
/// Covers the predicate that decides who may attach, or fetch, a signed petition sheet.
/// <para>
/// It was written out inline, in full, in two handlers before it was a thing you could call —
/// so these tests are as much about the extraction as about the rule. Two copies of an
/// authorization predicate agree until one of them is edited, and the second copy is the one
/// nobody remembers.
/// </para>
/// </summary>
public class SignedDocumentAccessTests
{
    private const string Signatory = "signed-this-one";
    private const string Stranger = "signed-nothing";

    [Fact]
    public void An_eligible_issue_is_visible_to_the_person_who_signed_it()
    {
        var issues = new[] { Eligible() }.AsQueryable();

        Assert.Single(issues.RestrictToSignatory(Signatory));
    }

    [Fact]
    public void An_issue_somebody_else_signed_is_not_visible()
    {
        var issues = new[] { Eligible() }.AsQueryable();

        Assert.Empty(issues.RestrictToSignatory(Stranger));
    }

    // The three clauses are separate rules, so each gets its own case. A single "eligible /
    // not eligible" pair would pass with any two of them deleted.
    [Fact]
    public void An_issue_that_has_not_ended_this_quarter_is_not_visible()
    {
        var issue = Eligible();
        issue.IssueProcess = IssueProcess.InCreation;

        Assert.Empty(new[] { issue }.AsQueryable().RestrictToSignatory(Signatory));
    }

    [Fact]
    public void An_issue_that_is_not_publicly_visible_is_not_visible_here_either()
    {
        var issue = Eligible();
        issue.IssueVisibility = IssueVisibility.NotVisible;

        Assert.Empty(new[] { issue }.AsQueryable().RestrictToSignatory(Signatory));
    }

    // The same guard IssueOwnerScope.OwnedBy carries, and for the same reason: a broken
    // principal yielding a null id must not be indistinguishable from a real user who has
    // signed nothing. Returning an empty set would be the safe answer here by luck rather
    // than by design, and the luck would run out the first time somebody added an "or".
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_missing_user_id_is_refused_rather_than_treated_as_nobody(string? userId)
    {
        var issues = new[] { Eligible() }.AsQueryable();

        Assert.Throws<ArgumentException>(() => issues.RestrictToSignatory(userId).ToList());
    }

    private static Issue Eligible() => new()
    {
        Id = Guid.NewGuid(),
        Title = "A question for the electorate",
        Question = "Should the bridge be rebuilt?",
        IssueProcess = IssueProcess.EndedInCurrentQuarter,
        IssueVisibility = IssueVisibility.VisibleForAll,
        Signatures = new List<Signature>
        {
            new()
            {
                SignaturePool = new SignaturePool { ApplicationUserId = Signatory },
            },
        },
    };
}
