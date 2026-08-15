namespace MR.Domain.Entities;

/// <summary>
/// MR's own projection of a user, keyed by the subject id in the token.
/// <para>
/// This is not an identity record and holds no credentials. Identity belongs to
/// <c>authservice</c> (ADR 0001), and P3 forbids MR reaching into its database — so MR keeps
/// the small amount it genuinely needs, populated from the claims of callers it has already
/// authenticated. Nothing here is authoritative; <c>authservice</c> is.
/// </para>
/// <para>
/// It exists because MR needs three things that a token alone cannot answer:
/// a <b>roster</b> — <c>InitQuarterCommand</c> issues a signature pool to every user when a
/// quarter opens, and has to know who they are without any of them being signed in;
/// an <b>id-to-email lookup</b> for the admin console, which resolves users MR is not
/// currently serving a request for; and a place to record that a user has been
/// <b>provisioned</b>, so MR's per-user setup runs exactly once.
/// </para>
/// </summary>
[Table(nameof(TableNames.MrUsers), Schema = SchemasNames.MRBasics)]
public class MrUser
{
    /// <summary>
    /// The identity service's subject id, as it arrives in <c>sub</c> /
    /// <see cref="System.Security.Claims.ClaimTypes.NameIdentifier"/>. Deliberately the same
    /// value already stored in <c>Issue.CreatedById</c>, <c>Subscription.ApplicationUserId</c>
    /// and <c>SignaturePool.ApplicationUserId</c>, so nothing has to be re-keyed.
    /// </summary>
    [Key]
    public string Id { get; set; } = null!;

    /// <summary>
    /// Last email seen for this user, refreshed on each authenticated request.
    /// <para>
    /// A cache, not a record: it can lag behind the identity service between sign-ins, and it
    /// is the right value for "who is this user now" — an administrator looking someone up
    /// wants their current address. It is deliberately <i>not</i> what a signature sheet uses;
    /// that reads <see cref="Issue.CreatedByEmail"/>, frozen at the moment of filing.
    /// </para>
    /// </summary>
    public string? Email { get; set; }

    public DateTime FirstSeenAt { get; set; }

    public DateTime LastSeenAt { get; set; }
}
