namespace Quorum.Infrastructure.Persistence;

/// <summary>
/// Completed by <see cref="MigrationBackgroundService"/> once the schema is in place.
/// <para>
/// P4 runs migrations in a hosted service after Kestrel starts, so the app answers health
/// probes while schema work is in flight. The corollary (SERVICE-API-PATTERNS §7) is that
/// everything that needs the schema — other background services, the readiness health
/// check — must wait on this signal instead of racing the migration.
/// </para>
/// </summary>
public interface IMigrationCompletionSignal
{
    /// <summary>True once the schema has been applied and the database is usable.</summary>
    bool IsCompleted { get; }

    /// <summary>Completes when the schema has been applied.</summary>
    Task WaitAsync(CancellationToken cancellationToken);
}

public sealed class MigrationCompletionSignal : IMigrationCompletionSignal
{
    private readonly TaskCompletionSource _completion =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public bool IsCompleted => _completion.Task.IsCompleted;

    public void MarkCompleted() => _completion.TrySetResult();

    public Task WaitAsync(CancellationToken cancellationToken)
        => _completion.Task.WaitAsync(cancellationToken);
}
