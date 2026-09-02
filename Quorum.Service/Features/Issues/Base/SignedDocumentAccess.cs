namespace Quorum.Service.Features.Issues.Base;

/// <summary>
/// Who may attach, or fetch, the signed petition sheet for an issue.
/// </summary>
/// <remarks>
/// <para>
/// The rule is not "who owns the issue" — it is "who signed it", and only once the issue has
/// ended as a winner in the current quarter and is visible to everyone. That is the predicate
/// <see cref="Queries.GetYourWinnersCommand"/> uses to decide which issues to offer a user,
/// and it is the one the upload path adopted so the two could not disagree about eligibility.
/// </para>
/// <para>
/// It was nonetheless written out twice, in full, in both of those handlers. Two copies of an
/// authorization predicate are one drift away from disagreeing, and delivery is now a third
/// caller, so it lives here instead. This is the same move <see cref="IssueOwnerScope"/>
/// makes for ownership: the check becomes a thing you call rather than a thing you remember.
/// </para>
/// </remarks>
public static class SignedDocumentAccess
{
    /// <summary>
    /// Narrows a query to the issues <paramref name="userId"/> may see a signed sheet for.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Thrown when the caller has no user id. Guarding here rather than returning an empty
    /// set is deliberate and follows <see cref="IssueOwnerScope.OwnedBy"/>: a null id is a
    /// broken principal, not a user who happens to have signed nothing, and the two should
    /// not be indistinguishable at the call site.
    /// </exception>
    public static IQueryable<Issue> RestrictToSignatory(this IQueryable<Issue> issues, string? userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException(
                "A signatory-scoped request requires an authenticated user id.", nameof(userId));
        }

        return issues
            .Where(issue => issue.Signatures.Any(signature =>
                signature.SignaturePool.ApplicationUserId == userId))
            .Where(issue => issue.IssueProcess == IssueProcess.EndedInCurrentQuarter)
            .Where(issue => issue.IssueVisibility == IssueVisibility.VisibleForAll);
    }
}
