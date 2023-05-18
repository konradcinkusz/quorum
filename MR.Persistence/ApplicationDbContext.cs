using Microsoft.EntityFrameworkCore.Metadata;
using System.Diagnostics.CodeAnalysis;

namespace MR.Persistence;

public interface IApplicationDbContext
{
    DbSet<Payment> Payments { get; set; }
    DbSet<AdminLog> Admin_Logs { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    DbSet<Subscription> Subscriptions { get; set; }
    DbSet<SubscriptionPayment> SubscriptionPayment { get; set; }
    DbSet<Issue> Issues { get; set; }
    DbSet<Quarter> Quarters { get; set; }
    DbSet<SignaturePool> SignaturePools { get; set; }
    DbSet<Signature> Signatures { get; set; }
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
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
    public DbSet<SubscriptionPayment> SubscriptionPayment { get; set; }
    public DbSet<Issue> Issues { get; set; }
    public DbSet<Quarter> Quarters { get; set; }
    public DbSet<SignaturePool> SignaturePools { get; set; }
    public DbSet<Signature> Signatures { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Seed();

        Logs_config(modelBuilder);
        Logs_triggers_config(modelBuilder);
        SubscriptionPaymentConfig(modelBuilder);
        QuarterIssueConfig(modelBuilder);
        base.OnModelCreating(modelBuilder);
    }

    private void Logs_config(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Subscription>()
            .HasMany(s => s.Subscription_Logs)
            .WithOne(sl => sl.Subscription)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Payment>()
            .HasMany(s => s.Payment_Logs)
            .WithOne(sl => sl.Payment)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void Logs_triggers_config(ModelBuilder modelBuilder)
    {
        modelBuilder
            .Entity<Payment>()
            .ToTable(t => t.HasTrigger("trg_Payment_Log"));

        modelBuilder
            .Entity<Subscription>()
            .ToTable(t => t.HasTrigger("trg_Subscription_Log"));
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

    private void QuarterIssueConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QuarterIssue>()
                    .HasKey(sp => new { sp.IssueId, sp.QuarterId });

        modelBuilder.Entity<QuarterIssue>()
            .HasOne(sp => sp.Issue)
            .WithMany(s => s.QuarterIssues)
            .HasForeignKey(sp => sp.IssueId)
            .OnDelete(DeleteBehavior.Restrict);// remove cascading delete behavior on SubscriptionPayment -> Subscription relationship

        modelBuilder.Entity<QuarterIssue>()
            .HasOne(sp => sp.Quarter)
            .WithMany(p => p.QuarterIssues)
            .HasForeignKey(sp => sp.QuarterId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
