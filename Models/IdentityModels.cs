using System.Collections.Generic;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Travel_Safe.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<Location> Locations { get; set; }

        public ApplicationUser()
        {
            Reviews = new HashSet<Review>();
            Locations = new HashSet<Location>();
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
            Database.SetInitializer<ApplicationDbContext>(null);
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewImage> ReviewImages { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<ReviewReport> ReviewReports { get; set; }
        public DbSet<Article> Articles { get; set; }




        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // กำหนด Primary Key สำหรับ ReviewImage
            modelBuilder.Entity<ReviewImage>()
                .HasKey(r => r.ReviewImageId);  // กำหนดให้ ReviewImageId เป็น Primary Key

            // การตั้งค่าความสัมพันธ์ระหว่าง Review และ ReviewImage
            modelBuilder.Entity<Review>()
                .HasMany(r => r.ReviewImages)
                .WithRequired(ri => ri.Review)
                .HasForeignKey(ri => ri.ReviewId)
                .WillCascadeOnDelete(false);

            // การตั้งค่าความสัมพันธ์ระหว่าง Review และ ReviewImage
            modelBuilder.Entity<Review>()
                .HasMany(r => r.ReviewImages)
                .WithRequired(ri => ri.Review)
                .HasForeignKey(ri => ri.ReviewId)
                .WillCascadeOnDelete(false);

            // ตั้งค่าความสัมพันธ์ระหว่าง ApplicationUser และ Review
            modelBuilder.Entity<Review>()
                .HasRequired(r => r.User)  // การเชื่อมโยงกับ ApplicationUser
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .WillCascadeOnDelete(false);

            // ตั้งค่าความสัมพันธ์ระหว่าง ApplicationUser และ Location
            modelBuilder.Entity<Location>()
                .HasRequired(l => l.Creator)  // การเชื่อมโยงกับ ApplicationUser
                .WithMany(u => u.Locations)
                .HasForeignKey(l => l.CreatorId)
                .WillCascadeOnDelete(false);
        }


        // วิธีการเพิ่ม Role "User", "Admin", "Provider" เมื่อสร้างฐานข้อมูล
        public void SeedRoles()
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(this));

            // ตรวจสอบและสร้าง Role "User"
            if (!roleManager.RoleExists("User"))
            {
                roleManager.Create(new IdentityRole("User"));
            }

            // ตรวจสอบและสร้าง Role "Admin"
            if (!roleManager.RoleExists("Admin"))
            {
                roleManager.Create(new IdentityRole("Admin"));
            }

            // ตรวจสอบและสร้าง Role "Provider"
            if (!roleManager.RoleExists("Provider"))
            {
                roleManager.Create(new IdentityRole("Provider"));
            }
        }
    }

}