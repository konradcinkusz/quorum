namespace MR.Persistence;

public interface IApplicationDbContext
{
    DbSet<Payment> Payments { get; set; }
}