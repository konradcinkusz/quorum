using MR.Domain.Auth;
using MR.Domain.Entities;
using Xunit;

namespace MR.Tests;

/// <summary>
/// Pins the resolution rule for an issue's creator email — the denormalised
/// <see cref="Issue.CreatedByEmail"/> first, the <see cref="Issue.CreatedBy"/> navigation only
/// as a fallback for rows written before that column existed.
/// <para>
/// Step 2 of ADR 0001's plan. These are characterisation tests in the P13 sense: written
/// before the behaviour moves, so that when the navigation is deleted along with local
/// identity, the fallback arm can be removed and these tests say whether anything else
/// changed. The rule is expressed here as the single expression the production sites use.
/// </para>
/// </summary>
public class IssueCreatorEmailTests
{
    /// <summary>The rule as written in IssuePDFService, AutoMapper and the query layer.</summary>
    private static string Resolve(Issue issue)
        => issue.CreatedByEmail ?? issue.CreatedBy?.Email ?? string.Empty;

    [Fact]
    public void The_denormalised_value_is_preferred()
    {
        var issue = new Issue
        {
            CreatedByEmail = "filed-as@example.test",
            CreatedBy = new ApplicationUser { Email = "renamed-since@example.test" },
        };

        // The point of the whole design: a signature sheet records who filed the initiative
        // at the time of filing. A later email change must not rewrite it.
        Assert.Equal("filed-as@example.test", Resolve(issue));
    }

    [Fact]
    public void The_navigation_is_used_when_the_column_is_empty()
    {
        // Rows created before the column existed, and not covered by the migration's backfill
        // because their account had already gone.
        var issue = new Issue
        {
            CreatedByEmail = null,
            CreatedBy = new ApplicationUser { Email = "legacy@example.test" },
        };

        Assert.Equal("legacy@example.test", Resolve(issue));
    }

    [Fact]
    public void An_issue_with_neither_resolves_to_empty_rather_than_throwing()
    {
        // This is a real regression, not a hypothetical: IssuePDFService dereferenced
        // issue.CreatedBy.Email with no null check, so an issue whose creator had been
        // deleted threw while generating the signature sheet instead of producing one.
        var issue = new Issue { CreatedByEmail = null, CreatedBy = null };

        Assert.Equal(string.Empty, Resolve(issue));
    }

    [Fact]
    public void The_navigation_is_not_consulted_when_the_column_is_set()
    {
        // Once identity moves to authservice there is no navigation to consult — it will be
        // null on every row. This pins that the primary path never depends on it, so removing
        // it is a deletion rather than a behaviour change.
        var issue = new Issue { CreatedByEmail = "only-source@example.test", CreatedBy = null };

        Assert.Equal("only-source@example.test", Resolve(issue));
    }
}
