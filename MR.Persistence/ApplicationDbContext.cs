namespace MR.Persistence;

public interface IApplicationDbContext
{
    DbSet<Payment> Payments { get; set; }
    DbSet<AdminLog> Admin_Logs { get; set; }
    DbSet<Subscription> Subscriptions { get; set; }
    DbSet<SubscriptionPayment> SubscriptionPayment { get; set; }
    DbSet<Issue> Issues { get; set; }
    DbSet<Quarter> Quarters { get; set; }
    DbSet<SignaturePool> SignaturePools { get; set; }
    DbSet<Signature> Signatures { get; set; }
    DbSet<IssueProcessingHistory> IssueProcessingHistories { get; set; }
    DbSet<IssueVisibilityHistory> IssueVisibilityHistories { get; set; }
    DbSet<QuarterIssue> QuarterIssues { get; set; }
    DbSet<IssueRatingHistory> IssueRatingHistories { get; set; }
    DbSet<CloudinaryFile> CloudinaryFiles { get; set; }
    DbSet<CloudinaryFileIssue> CloudinaryFileIssues { get; set; }
    DbSet<MrUser> MrUsers { get; set; }

    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions options, IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
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
    public DbSet<IssueProcessingHistory> IssueProcessingHistories { get; set; }
    public DbSet<IssueVisibilityHistory> IssueVisibilityHistories { get; set; }
    public DbSet<QuarterIssue> QuarterIssues { get; set; }
    public DbSet<IssueRatingHistory> IssueRatingHistories { get; set; }
    public DbSet<CloudinaryFile> CloudinaryFiles { get; set; }
    public DbSet<CloudinaryFileIssue> CloudinaryFileIssues { get; set; }
    public DbSet<MrUser> MrUsers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Seed();

        Logs_config(modelBuilder);
        SubscriptionPaymentConfig(modelBuilder);
        QuarterIssueConfig(modelBuilder);
        CloudinaryFileIssueConfig(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        UpdateCreatedAtProperty();
        return base.SaveChanges();
    }

    public override EntityEntry<TEntity> Remove<TEntity>(TEntity entity)
    {
        var entityEntry = Entry(entity);

        if (entityEntry.State == EntityState.Deleted)
        {
            // If the entity is already marked for deletion, return the entity entry
            return entityEntry;
        }

        // Otherwise, set the IsDeleted property to true and mark the entity as Modified
        if (entityEntry.Entity is BaseEntity baseEntity)
        {
            baseEntity.IsDeleted = true;
            entityEntry.State = EntityState.Modified;
        }

        return entityEntry;
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateCreatedAtProperty();
        UpdateUpdatedAtProperty();
        return base.SaveChangesAsync(cancellationToken);
    }

    void UpdateCreatedAtProperty()
    {
        var entities = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .OfType<BaseEntity>();

        foreach (var entity in entities)
        {
            entity.CreatedAt = DateTime.UtcNow;
        }
    }

    void UpdateUpdatedAtProperty()
    {
        var entities = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Modified)
            .Select(e => e.Entity)
            .OfType<BaseEntity>();

        foreach (var entity in entities)
        {
            entity.UpdatedAt = DateTime.UtcNow;
        }
    }

    void Logs_config(ModelBuilder modelBuilder)
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

    void SubscriptionPaymentConfig(ModelBuilder modelBuilder)
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

    void QuarterIssueConfig(ModelBuilder modelBuilder)
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

    void CloudinaryFileIssueConfig(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CloudinaryFileIssue>()
                    .HasKey(sp => new { sp.IssueId, sp.CloudinaryFileId });

        modelBuilder.Entity<CloudinaryFileIssue>()
            .HasOne(sp => sp.Issue)
            .WithMany(s => s.CloudinaryFileIssues)
            .HasForeignKey(sp => sp.IssueId)
            .OnDelete(DeleteBehavior.Restrict);// remove cascading delete behavior on SubscriptionPayment -> Subscription relationship

        modelBuilder.Entity<CloudinaryFileIssue>()
            .HasOne(sp => sp.CloudinaryFile)
            .WithMany(p => p.CloudinaryFileIssues)
            .HasForeignKey(sp => sp.CloudinaryFileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
