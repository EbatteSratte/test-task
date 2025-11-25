using Microsoft.EntityFrameworkCore;
using backend.Models;

namespace backend.Data
{
    public class ApplicationContext : DbContext
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Master> Masters { get; set; } = null!;
        public DbSet<Detail> Details { get; set; } = null!;
        public DbSet<ErrorLog> ErrorLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Master>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.HasIndex(m => m.Number).IsUnique();
                entity.Property(m => m.Number).IsRequired().HasMaxLength(50);
                entity.Property(m => m.Date).IsRequired();
                entity.Property(m => m.Amount).HasColumnType("decimal(18,2)");
                entity.Property(m => m.Note).HasMaxLength(500);
                entity.Property(m => m.CreatedAt).HasColumnType("timestamp");
                entity.Property(m => m.UpdatedAt).HasColumnType("timestamp");
            });

            modelBuilder.Entity<Detail>(entity =>
            {
                entity.HasKey(d => d.Id);
                entity.Property(d => d.Name).IsRequired().HasMaxLength(200);
                entity.Property(d => d.Amount).HasColumnType("decimal(18,2)");
                entity.Property(d => d.CreatedAt).HasColumnType("timestamp");
                entity.Property(d => d.UpdatedAt).HasColumnType("timestamp");


                entity.HasOne(d => d.Master)
                    .WithMany(m => m.Details)
                    .HasForeignKey(d => d.MasterId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ErrorLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ErrorType).IsRequired().HasMaxLength(100);
                entity.Property(e => e.ErrorMessage).IsRequired();
                entity.Property(e => e.UserAction).HasMaxLength(100);
                entity.Property(e => e.EntityType).HasMaxLength(50);
                entity.Property(e => e.EntityId).HasMaxLength(50);
                entity.Property(e => e.IpAddress).HasMaxLength(45);
                entity.Property(e => e.Timestamp).HasColumnType("timestamp");
            });
        }

        public async Task LogErrorToBd(string userAction, Exception exception, string entityType, string? entityId = null, string? ipAddress = null)
        {
            try
            {
                var errorLog = new ErrorLog
                {
                    ErrorType = exception.GetType().Name,
                    ErrorMessage = exception.Message,
                    StackTrace = exception.StackTrace,
                    UserAction = userAction,
                    EntityType = entityType,
                    EntityId = entityId,
                    Timestamp = DateTime.UtcNow,
                    IpAddress = ipAddress
                };

                ErrorLogs.Add(errorLog);
                await SaveChangesAsync();
            }
            catch (Exception)
            {
            }
        }
    }
}