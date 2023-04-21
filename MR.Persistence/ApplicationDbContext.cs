namespace MR.Persistence;

public interface IApplicationDbContext
{
    DbSet<Payment> Payments { get; set; }
    DbSet<AdminLog> Admin_Logs { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    DbSet<Subscription> Subscriptions { get; set; }
}

public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(
        DbContextOptions options,
        IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
    {
    }

    public DbSet<Payment> Payments { get; set; }
    public DbSet<Payment_Log> Payment_Logs { get; set; }
    public DbSet<AdminLog> Admin_Logs { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Seed();
        modelBuilder
            .Entity<Payment>()
            .ToTable(t => t.HasTrigger("trg_Payment_InsertUpdateDelete"));
        base.OnModelCreating(modelBuilder);
    }
}
