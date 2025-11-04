using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using galutine.Models;

namespace galutine.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<InventoryField> InventoryFields { get; set; }
        public DbSet<InventoryItem> InventoryItems { get; set; }
        public DbSet<FieldValue> FieldValues { get; set; }
        public DbSet<InventoryAccess> InventoryAccesses { get; set; }
        public DbSet<ItemLike> ItemLikes { get; set; }
        public DbSet<DiscussionPost> DiscussionPosts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Inventory>()
                .HasOne(i => i.Owner)
                .WithMany(u => u.OwnedInventories)
                .HasForeignKey(i => i.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<InventoryItem>()
                .HasIndex(i => new { i.InventoryId, i.CustomId })
                .IsUnique();

            builder.Entity<FieldValue>()
                .HasOne(fv => fv.InventoryItem)
                .WithMany(it => it.FieldValues)
                .HasForeignKey(fv => fv.InventoryItemId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<FieldValue>()
                .HasOne(fv => fv.InventoryField)
                .WithMany()
                .HasForeignKey(fv => fv.InventoryFieldId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<InventoryAccess>()
                .HasKey(a => new { a.InventoryId, a.UserId });

            builder.Entity<InventoryAccess>()
                .HasOne(a => a.Inventory)
                .WithMany(i => i.AccessList)
                .HasForeignKey(a => a.InventoryId);

            builder.Entity<InventoryAccess>()
                .HasOne(a => a.User)
                .WithMany(u => u.InventoryAccesses)
                .HasForeignKey(a => a.UserId);

            builder.Entity<ItemLike>()
                .HasKey(l => new { l.InventoryItemId, l.UserId });

            builder.Entity<ItemLike>()
                .HasOne(l => l.InventoryItem)
                .WithMany(i => i.Likes)
                .HasForeignKey(l => l.InventoryItemId);

            builder.Entity<DiscussionPost>()
                .HasOne(p => p.Inventory)
                .WithMany(i => i.DiscussionPosts)
                .HasForeignKey(p => p.InventoryId);
        }
    }
}
