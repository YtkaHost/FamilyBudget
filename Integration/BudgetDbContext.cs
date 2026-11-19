using FamilyBudget.Models;
using Microsoft.EntityFrameworkCore;

namespace FamilyBudget.Integration
{
    public class BudgetDbContext : DbContext
    {
        private readonly string _databasePath;

        public BudgetDbContext(string databasePath)
        {
            _databasePath = databasePath;
        }

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<BudgetPlan> BudgetPlans { get; set; } = null!;
        public DbSet<SavingGoal> SavingGoals { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=" + _databasePath);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(c => c.Id).ValueGeneratedNever();
                entity.Property(c => c.Type).HasConversion<string>();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Id).ValueGeneratedNever();
                entity.Property(u => u.FamilyRole).HasConversion<string>();
            });

            // Не дозволяємо видаляти категорії й користувачів, на які посилаються операції.
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(t => t.Id).ValueGeneratedNever();
                entity.HasOne(t => t.Category).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(t => t.User).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict);
                entity.Ignore(t => t.Type);
            });

            modelBuilder.Entity<BudgetPlan>(entity =>
            {
                entity.Property(p => p.Id).ValueGeneratedNever();
                entity.HasOne(p => p.Category).WithMany().IsRequired().OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SavingGoal>(entity =>
            {
                entity.Property(g => g.Id).ValueGeneratedNever();
                entity.Ignore(g => g.ProgressPercent);
                entity.Ignore(g => g.IsAchieved);
                entity.Ignore(g => g.DaysLeft);
            });
        }
    }
}
