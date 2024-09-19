using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class ApplicationDBContext : IdentityDbContext<AppUser>
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options)
            : base(options) { }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }

        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Ensure the base IdentityDbContext configurations are applied.

            // Post -> Comment relationship with cascade delete
            modelBuilder
                .Entity<Post>()
                .HasMany(p => p.Comments)
                .WithOne(c => c.Post)
                .HasForeignKey(c => c.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            // Post -> Category relationship with restricted delete
            modelBuilder
                .Entity<Post>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Posts)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Post -> Like relationship without cascade delete (to avoid multiple cascade paths)
            modelBuilder
                .Entity<Post>()
                .HasMany(p => p.Likes)
                .WithOne(l => l.Post)
                .HasForeignKey(l => l.PostId)
                .OnDelete(DeleteBehavior.Restrict); // Change this to restrict

            // Comment -> Like relationship with cascade delete
            modelBuilder
                .Entity<Comment>()
                .HasMany(c => c.Likes)
                .WithOne(l => l.Comment)
                .HasForeignKey(l => l.CommentId)
                .OnDelete(DeleteBehavior.Cascade); // Keep cascade delete here

            // AppUser -> Post relationship with restricted delete
            modelBuilder
                .Entity<AppUser>()
                .HasMany(u => u.Posts)
                .WithOne(p => p.AppUser)
                .HasForeignKey(p => p.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // AppUser -> Comment relationship with restricted delete
            modelBuilder
                .Entity<AppUser>()
                .HasMany(u => u.Comments)
                .WithOne(c => c.AppUser)
                .HasForeignKey(c => c.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // AppUser -> Like relationship with restricted delete
            modelBuilder
                .Entity<AppUser>()
                .HasMany(u => u.Likes)
                .WithOne(l => l.AppUser)
                .HasForeignKey(l => l.AppUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Explanation of the relationships:
            // A Post can have multiple Comments and Likes.
            // A Comment can have multiple Likes.
            // An AppUser can create multiple Posts, Comments, and Likes.

            // The OnDelete behavior:
            // When a Post is deleted, all associated Comments and Likes are also deleted (cascade delete).
            // When a Comment is deleted, all associated Likes are also deleted (cascade delete).
            // When an AppUser is deleted, their associated Posts, Comments, and Likes are not deleted (restrict delete).

            // Seed predefined categories
            modelBuilder
                .Entity<Category>()
                .HasData(
                    new Category { Id = 1, Name = "Träd" },
                    new Category { Id = 2, Name = "Buskar" },
                    new Category { Id = 3, Name = "Blommor" },
                    new Category { Id = 4, Name = "Gräs" }
                );

            // Configure primary key for IdentityUserLogin<string>
            modelBuilder
                .Entity<IdentityUserLogin<string>>()
                .HasKey(i => new { i.LoginProvider, i.ProviderKey });
        }
    }
}
