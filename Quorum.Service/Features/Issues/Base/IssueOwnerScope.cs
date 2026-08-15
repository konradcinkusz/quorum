namespace Quorum.Service.Features.Issues.Base;

/// <summary>
/// Who a request is allowed to touch. Every command or query that resolves an
/// <see cref="Issue"/> by id takes one of these, so the decision "may this caller see or
/// change this issue?" is made explicitly at the call site instead of being forgotten in
/// the handler.
/// <para>
/// There is deliberately no parameterless constructor and no implicit conversion: the only
/// ways to obtain a scope are <see cref="OwnedBy"/>, which restricts to one user, and
/// <see cref="Administrator"/>, which does not restrict at all. A reader of a controller
/// action can therefore see which one is in force without opening the handler.
/// </para>
/// </summary>
public readonly struct IssueOwnerScope
{
    /// <summary>
    /// The user whose issues are in scope, or <c>null</c> for an unrestricted
    /// (administrator) scope.
    /// </summary>
    public string? OwnerId { get; }

    private IssueOwnerScope(string? ownerId) => OwnerId = ownerId;

    /// <summary>
    /// Restricts the request to issues created by <paramref name="userId"/>.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when the caller has no user id. This is a guard against the failure that
    /// motivated this type: an unauthenticated or malformed principal yielding a null id,
    /// which — if it were allowed through — would widen the scope to every issue rather
    /// than narrowing it to none.
    /// </exception>
    public static IssueOwnerScope OwnedBy(string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "An owner-scoped request requires an authenticated user id.", nameof(userId));
        }

        return new IssueOwnerScope(userId);
    }

    /// <summary>
    /// An unrestricted scope, for endpoints already gated behind the
    /// <c>RequireAdminRole</c> policy. Every use of this is a deliberate authorization
    /// bypass and should be readable as one.
    /// </summary>
    public static IssueOwnerScope Administrator() => new(null);

    /// <summary>True when this scope places no restriction on which issues are visible.</summary>
    public bool IsUnrestricted => OwnerId is null;
}

public static class IssueOwnerScopeQueryExtensions
{
    /// <summary>
    /// Narrows a query to the issues <paramref name="scope"/> permits. The branch is taken
    /// in C# rather than inside the predicate so the restricted case produces a plain
    /// equality filter that SQL Server can index, and the unrestricted case adds nothing
    /// to the query at all.
    /// </summary>
    public static IQueryable<Issue> RestrictToOwner(this IQueryable<Issue> query, IssueOwnerScope scope)
    {
        if (scope.IsUnrestricted)
        {
            return query;
        }

        var ownerId = scope.OwnerId;
        return query.Where(x => x.CreatedById == ownerId);
    }
}
