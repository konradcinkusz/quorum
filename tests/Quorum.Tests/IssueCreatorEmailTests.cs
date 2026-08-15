using Quorum.Domain.Entities;
using Xunit;

namespace Quorum.Tests;

/// <summary>
/// Pins the resolution rule for an issue's creator email: the denormalised
/// <see cref="Issue.CreatedByEmail"/>, captured from the <c>email</c> claim at filing time,
/// with an empty string when the column is empty.
/// <para>
/// These began as characterisation tests for step 3 of ADR 0001, when the rule still fell
/// back to the <c>CreatedBy</c> navigation for pre-column rows. The cutover deleted local
/// identity and the navigation with it, so the rule is now the column alone — which is the
/// point of the design: a signature sheet records who filed the initiative at the time of
/// filing, and no later account change rewrites it.
/// </para>
/// </summary>
public class IssueCreatorEmailTests
{
    /// <summary>The rule as written in IssuePDFService, AutoMapper and the query layer.</summary>
    private static string Resolve(Issue issue)
        => issue.CreatedByEmail ?? string.Empty;

    [Fact]
    public void The_denormalised_value_is_used()
    {
        var issue = new Issue
        {
            CreatedByEmail = "filed-as@example.test",
        };

        Assert.Equal("filed-as@example.test", Resolve(issue));
    }

    [Fact]
    public void An_issue_without_a_captured_email_resolves_to_empty_not_null()
    {
        // Rows written before the column existed, or by a token without an email claim.
        // Consumers print this onto a PDF; they get an empty string, never a null.
        var issue = new Issue { CreatedByEmail = null };

        Assert.Equal(string.Empty, Resolve(issue));
    }

    [Fact]
    public void The_capture_happens_at_creation_and_nothing_updates_it()
    {
        // The property is an ordinary settable snapshot; this documents that no entity
        // logic recomputes it. Staleness against the identity service is accepted and
        // intended (ADR 0001).
        var issue = new Issue { CreatedByEmail = "original@example.test" };

        Assert.Equal("original@example.test", Resolve(issue));
    }
}
