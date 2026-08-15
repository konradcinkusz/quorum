using MR.Domain.Entities;
using MR.Service.Features.Issues.Base;
using Xunit;

namespace MR.Tests;

/// <summary>
/// Covers the type introduced to close review findings F2 and F3 — the authorization hole
/// where no handler compared an issue's <c>CreatedById</c> to the caller. These are the
/// tests that would have failed against the code as it stood before 2026-08-15.
/// </summary>
public class IssueOwnerScopeTests
{
    private const string Alice = "alice-user-id";
    private const string Bob = "bob-user-id";

    [Fact]
    public void OwnedBy_keeps_the_user_it_was_given()
    {
        var scope = IssueOwnerScope.OwnedBy(Alice);

        Assert.Equal(Alice, scope.OwnerId);
        Assert.False(scope.IsUnrestricted);
    }

    [Fact]
    public void Administrator_is_unrestricted()
    {
        var scope = IssueOwnerScope.Administrator();

        Assert.Null(scope.OwnerId);
        Assert.True(scope.IsUnrestricted);
    }

    // The important one. If a null or blank caller id could produce a scope, it would
    // produce an *unrestricted* one — widening access to every issue rather than narrowing
    // it to none — which is the exact failure this type exists to prevent.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void OwnedBy_refuses_a_missing_user_id(string? userId)
    {
        Assert.Throws<ArgumentException>(() => IssueOwnerScope.OwnedBy(userId));
    }

    [Fact]
    public void A_default_scope_cannot_be_constructed_implicitly()
    {
        // There is no public constructor: the only ways in are the two factories. This test
        // exists so that adding one is a deliberate act that breaks a test, rather than a
        // quiet change that reintroduces "no scope means all issues".
        var constructors = typeof(IssueOwnerScope).GetConstructors();

        Assert.Empty(constructors);
    }

    [Fact]
    public void RestrictToOwner_returns_only_that_users_issues()
    {
        var issues = Issues().AsQueryable();

        var visible = issues.RestrictToOwner(IssueOwnerScope.OwnedBy(Alice)).ToList();

        Assert.Equal(2, visible.Count);
        Assert.All(visible, i => Assert.Equal(Alice, i.CreatedById));
    }

    [Fact]
    public void RestrictToOwner_does_not_leak_another_users_issue()
    {
        var issues = Issues().AsQueryable();

        var visible = issues.RestrictToOwner(IssueOwnerScope.OwnedBy(Alice)).ToList();

        Assert.DoesNotContain(visible, i => i.CreatedById == Bob);
    }

    [Fact]
    public void RestrictToOwner_returns_everything_for_an_administrator()
    {
        var issues = Issues().AsQueryable();

        var visible = issues.RestrictToOwner(IssueOwnerScope.Administrator()).ToList();

        Assert.Equal(4, visible.Count);
    }

    [Fact]
    public void RestrictToOwner_excludes_an_issue_with_no_creator()
    {
        // Issue.CreatedById is nullable. An ownerless row matching nobody is the safe
        // direction, and this pins that: it must not fall through to "visible to all".
        var issues = Issues().AsQueryable();

        var visible = issues.RestrictToOwner(IssueOwnerScope.OwnedBy(Alice)).ToList();

        Assert.DoesNotContain(visible, i => i.CreatedById is null);
    }

    private static List<Issue> Issues() =>
    [
        new() { Id = Guid.NewGuid(), CreatedById = Alice, Title = "Alice one" },
        new() { Id = Guid.NewGuid(), CreatedById = Alice, Title = "Alice two" },
        new() { Id = Guid.NewGuid(), CreatedById = Bob, Title = "Bob one" },
        new() { Id = Guid.NewGuid(), CreatedById = null, Title = "Orphan" },
    ];
}
