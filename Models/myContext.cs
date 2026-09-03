using Microsoft.EntityFrameworkCore;

namespace dress_ordering_system.Models
{
    public class myContext : DbContext
    {
        public myContext(DbContextOptions<myContext> options)
            : base(options)
        {
        }

        public DbSet<Admin> tbl_admin { get; set; }

        public DbSet<Customer> tbl_customer { get; set; }

        public DbSet<Category> tbl_category { get; set; }

        public DbSet<Dress> tbl_dress { get; set; }

        public DbSet<Cart> tbl_cart { get; set; }

        public DbSet<Blog> tbl_blog { get; set; }

        public DbSet<Feedback> tbl_feedback { get; set; }

        public DbSet<Faqs> tbl_faqs { get; set; }

        public DbSet<Order> tbl_order { get; set; }

        // Wishlist
        public DbSet<Wishlist> tbl_wishlist { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Dress → Category
            modelBuilder.Entity<Dress>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Dress)
                .HasForeignKey(p => p.cat_id);


            // Wishlist → Customer
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Customer)
                .WithMany()
                .HasForeignKey(w => w.customer_id)
                .OnDelete(DeleteBehavior.Cascade);


            // Wishlist → Dress
            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Dress)
                .WithMany()
                .HasForeignKey(w => w.dress_id)
                .OnDelete(DeleteBehavior.Cascade);


            // Prevent duplicate wishlist entries
            modelBuilder.Entity<Wishlist>()
                .HasIndex(w => new
                {
                    w.customer_id,
                    w.dress_id
                })
                .IsUnique();


            // Order amount precision
            modelBuilder.Entity<Order>()
                .Property(o => o.total_amount)
                .HasPrecision(18, 2);
        }
    }
}