namespace MR.Persistence;

public partial class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>, IApplicationDbContext
{
    public ApplicationDbContext(
        DbContextOptions options,
        IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
    {
    }

    public DbSet<Payment> Payments { get; set; }
    public DbSet<Payment_Log> Payment_Logs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Seed();
        base.OnModelCreating(modelBuilder);
    }
}
