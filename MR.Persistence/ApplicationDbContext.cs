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
    public DbSet<Subscription_Log> Subscription_Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Seed();

        modelBuilder
            .Entity<Payment>()
            .ToTable(t => t.HasTrigger("trg_Payment_Log"));

        modelBuilder
            .Entity<Subscription>()
            .ToTable(t => t.HasTrigger("trg_Subscription_Log"));

        SubscriptionPaymentConfig(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private void Logs_triggers_config(ModelBuilder modelBuilder)
    {
    }

    private void SubscriptionPaymentConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SubscriptionPayment>()
                    .HasKey(sp => new { sp.SubscriptionId, sp.PaymentId });

        modelBuilder.Entity<SubscriptionPayment>()
            .HasOne(sp => sp.Subscription)
            .WithMany(s => s.SubscriptionPayments)
            .HasForeignKey(sp => sp.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);// remove cascading delete behavior on SubscriptionPayment -> Subscription relationship

        modelBuilder.Entity<SubscriptionPayment>()
            .HasOne(sp => sp.Payment)
            .WithMany(p => p.SubscriptionPayments)
            .HasForeignKey(sp => sp.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
