namespace Quorum.Service.UserManagement;

/// <summary>
/// Fills the <c>[NotMapped]</c> display-email properties from the <see cref="QuorumUser"/>
/// projection, one query per page. This replaced the <c>ApplicationUser</c> navigation the
/// admin lists used to <c>Include</c>: with identity in authservice (ADR 0001) there is no
/// user table here to join, and the projection is exactly the id→email cache kept for this.
/// </summary>
public static class UserEmailEnrichment
{
    public static async Task PopulateUserEmailsAsync<T>(
        this IApplicationDbContext context,
        IEnumerable<T> rows,
        Func<T, string?> userId,
        Action<T, string?> setEmail,
        CancellationToken cancellationToken)
    {
        var materialized = rows as ICollection<T> ?? rows.ToList();

        var ids = materialized
            .Select(userId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id!)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return;
        }

        var emails = await context.QuorumUsers
            .Where(x => ids.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => x.Email, cancellationToken);

        foreach (var row in materialized)
        {
            var id = userId(row);
            if (id is not null && emails.TryGetValue(id, out var email))
            {
                setEmail(row, email);
            }
        }
    }
}
