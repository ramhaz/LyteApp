    using LyteApi.Models;
    using Microsoft.EntityFrameworkCore;
    using LyteApp.Models;

    namespace LyteApp.Data;

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) 
            : base(options) { }

        //liste over vores tabeller i databasen, som vi skal bruge i vores api
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Ingredient> Ingredients => Set<Ingredient>();
        public DbSet<Review> Reviews => Set<Review>();
        
        
        public DbSet<HydrationLog> HydrationLogs => Set<HydrationLog>();
       
        public DbSet<HydrationPlan> HydrationPlans => Set<HydrationPlan>();
        
        public DbSet<Challenge> Challenges { get; set; }
        
        public DbSet<UserChallenge> UserChallenges { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Product -> Ingredients (one-to-many)
            modelBuilder.Entity<Product>()
                .HasMany(p => p.Ingredients)
                .WithOne(i => i.Product)
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<HydrationPlan>()
                .HasMany(p => p.HydrationLogs)
                .WithOne(l => l.HydrationPlan)
                .HasForeignKey(l => l.PlanId)
                .OnDelete(DeleteBehavior.Cascade);
           
            modelBuilder.Entity<UserChallenge>()
                .HasIndex(uc => new { uc.UserId, uc.ChallengeId })
                .IsUnique();

            modelBuilder.Entity<UserChallenge>()
                .HasOne(uc => uc.Challenge)
                .WithMany()
                .HasForeignKey(uc => uc.ChallengeId);

        }
    }