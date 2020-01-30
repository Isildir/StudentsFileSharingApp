using Microsoft.EntityFrameworkCore;
using StudentsFileSharingApp.Entities.Models;
using StudentsFileSharingApp.Entities.Models.ManyToMany;

namespace StudentsFileSharingApp.Entities
{
    public class BasicContext : DbContext
    {
        public BasicContext(DbContextOptions<BasicContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        public DbSet<File> Files { get; set; }

        public DbSet<Group> Groups { get; set; }

        public DbSet<Post> Posts { get; set; }

        public DbSet<PostComment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserGroup>().HasKey(a => new { a.UserId, a.GroupId });

            modelBuilder.Entity<PostComment>().HasOne(a => a.Author).WithMany(a => a.Comments).OnDelete(DeleteBehavior.NoAction);
            modelBuilder.Entity<PostComment>().HasOne(a => a.Post).WithMany(a => a.Comments).OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Post>().HasIndex(b => b.GroupId);
            modelBuilder.Entity<File>().HasIndex(b => b.GroupId);

            modelBuilder.Entity<User>().HasIndex(b => b.Name).IsUnique();
            modelBuilder.Entity<Group>().HasIndex(b => b.Name).IsUnique();
            modelBuilder.Entity<Post>().HasIndex(b => new { b.GroupId, b.Title }).IsUnique();
            modelBuilder.Entity<File>().HasIndex(b => new { b.GroupId, b.Name }).IsUnique();
        }
    }
}