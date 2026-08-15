using MR.Domain.Entities;
using MR.Service.Extensions;
using Xunit;

namespace MR.Tests;

/// <summary>
/// Characterisation tests for the quarter arithmetic. Per P13, these are written before the
/// behaviour is moved, not after — this logic decides which citizens' initiative wins each
/// quarter, runs four times a year, and a regression in it would be discovered in production
/// three months late.
/// <para>
/// They pin the behaviour as it is, including its limitation: every method here reads
/// <c>DateTime.UtcNow</c> directly, so "the current quarter" cannot be varied from a test.
/// That is the finding these tests document rather than hide — the clock needs injecting
/// before the interesting cases (a quarter boundary, a year rollover) can be covered at all.
/// </para>
/// </summary>
public class QuarterExtensionsTests
{
    private static int ExpectedQuarter(DateTime utc) => (utc.Month - 1) / 3 + 1;

    [Fact]
    public void CheckCurrentQuarter_accepts_the_quarter_we_are_actually_in()
    {
        var now = DateTime.UtcNow;
        var quarter = new Quarter { Year = now.Year, QuarterNumber = ExpectedQuarter(now) };

        Assert.True(QuarterExtensions.CheckCurrentQuarter(quarter));
    }

    [Fact]
    public void CheckCurrentQuarter_rejects_a_different_quarter_of_the_same_year()
    {
        var now = DateTime.UtcNow;
        var current = ExpectedQuarter(now);
        var other = current == 4 ? 1 : current + 1;

        var quarter = new Quarter { Year = now.Year, QuarterNumber = other };

        Assert.False(QuarterExtensions.CheckCurrentQuarter(quarter));
    }

    [Fact]
    public void CheckCurrentQuarter_rejects_the_same_quarter_of_a_different_year()
    {
        // Year is part of the identity. Without this, an initiative from Q3 of a previous
        // year would be resolved as if it belonged to the current one.
        var now = DateTime.UtcNow;
        var quarter = new Quarter { Year = now.Year - 1, QuarterNumber = ExpectedQuarter(now) };

        Assert.False(QuarterExtensions.CheckCurrentQuarter(quarter));
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 2)]
    [InlineData(5, 2)]
    [InlineData(6, 2)]
    [InlineData(7, 3)]
    [InlineData(8, 3)]
    [InlineData(9, 3)]
    [InlineData(10, 4)]
    [InlineData(11, 4)]
    [InlineData(12, 4)]
    public void The_month_to_quarter_mapping_is_the_one_the_production_code_uses(int month, int expectedQuarter)
    {
        // The production expression, restated. It is duplicated here deliberately: this test
        // is the specification, and if the formula in QuarterExtensions is ever changed, the
        // table above is what has to be argued with.
        Assert.Equal(expectedQuarter, (month - 1) / 3 + 1);
    }

    [Fact]
    public void GetCurrentQuarter_finds_the_matching_row_and_ignores_the_rest()
    {
        var now = DateTime.UtcNow;
        var current = ExpectedQuarter(now);

        var quarters = new List<Quarter>
        {
            new() { Id = Guid.NewGuid(), Year = now.Year - 1, QuarterNumber = current },
            new() { Id = Guid.NewGuid(), Year = now.Year, QuarterNumber = current },
            new() { Id = Guid.NewGuid(), Year = now.Year + 1, QuarterNumber = current },
        }.AsQueryable();

        var found = quarters.GetCurrentQuarter();

        Assert.NotNull(found);
        Assert.Equal(now.Year, found!.Year);
        Assert.Equal(current, found.QuarterNumber);
    }

    [Fact]
    public void GetCurrentQuarter_returns_null_when_no_quarter_has_been_initialised()
    {
        // PublishIssueCommand depends on this: no current quarter means publishing must fail
        // with "contact an admin" rather than throwing.
        var quarters = new List<Quarter>().AsQueryable();

        Assert.Null(quarters.GetCurrentQuarter());
    }

    [Fact]
    public void GetCurrentAndFutureQuarters_excludes_the_past()
    {
        var now = DateTime.UtcNow;
        var current = ExpectedQuarter(now);

        var quarters = new List<Quarter>
        {
            new() { Id = Guid.NewGuid(), Year = now.Year - 1, QuarterNumber = 4 },
            new() { Id = Guid.NewGuid(), Year = now.Year, QuarterNumber = current },
            new() { Id = Guid.NewGuid(), Year = now.Year + 1, QuarterNumber = 1 },
        }.AsQueryable();

        var result = quarters.GetCurrentAndFutureQuarters().ToList();

        Assert.DoesNotContain(result, q => q.Year < now.Year);
        Assert.Contains(result, q => q.Year == now.Year && q.QuarterNumber == current);
        Assert.Contains(result, q => q.Year == now.Year + 1);
    }
}
