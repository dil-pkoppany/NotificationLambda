using NotificationTablePopulationLambda.Models;
using Microsoft.EntityFrameworkCore;

namespace NotificationTablePopulationLambda.Data;

public interface IManageCO2DbContext
{
    DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
    DbSet<ChangeLog> ChangeLog { get; set; }
    DbSet<ChangeLogNotification> ChangeLogNotification { get; set; }
    DbSet<CompaniesUsers> CompaniesUsers { get; set; }
    DbSet<Notification> Notification { get; set; }
    int SaveChanges();
    void Dispose();
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
}

public class ManageCO2DbContext : DbContext, IManageCO2DbContext
{
    public DbSet<ChangeLogNotification> ChangeLogNotification { get; set; }
    public DbSet<ChangeLog> ChangeLog { get; set; }
    public DbSet<CompaniesUsers> CompaniesUsers { get; set; }
    public DbSet<AspNetUserRoles> AspNetUserRoles { get; set; }
    public DbSet<Notification> Notification { get; set; }

    public ManageCO2DbContext(DbContextOptions<ManageCO2DbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChangeLogNotification>().ToTable("ChangeLogNotification");
        modelBuilder.Entity<ChangeLog>().ToTable("ChangeLog");
        modelBuilder.Entity<CompaniesUsers>().ToTable("CompaniesUsers");
        modelBuilder.Entity<AspNetUserRoles>().ToTable("AspNetUserRoles").HasKey(x => new { x.UserId, x.RoleId });
        modelBuilder.Entity<Notification>().ToTable("Notification");
    }
}