using DevartMcpDotnetDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace DevartMcpDotnetDemo.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Map Customer entity to public.customers table
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers", "public");
            entity.HasKey(e => e.Id).HasName("customers_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.Name)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            entity.HasMany(e => e.Orders)
                .WithOne(e => e.Customer)
                .HasForeignKey(e => e.CustomerId)
                .HasConstraintName("orders_customer_id_fkey");
        });

        // Map Order entity to public.orders table
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders", "public");
            entity.HasKey(e => e.Id).HasName("orders_pkey");

            entity.Property(e => e.Id)
                .HasColumnName("id");

            entity.Property(e => e.CustomerId)
                .HasColumnName("customer_id")
                .IsRequired();

            entity.Property(e => e.OrderDate)
                .HasColumnName("order_date")
                .IsRequired();

            entity.Property(e => e.TotalAmount)
                .HasColumnName("total_amount")
                .HasPrecision(12, 2)
                .IsRequired();

            entity.Property(e => e.Status)
                .HasColumnName("status")
                .HasMaxLength(20)
                .IsRequired();
        });
    }
}
