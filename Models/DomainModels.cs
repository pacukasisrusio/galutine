using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace galutine.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsBlocked { get; set; }
        public DateTime? LastLoginAt { get; set; }

        public ICollection<Inventory> OwnedInventories { get; set; } = new List<Inventory>();
        public ICollection<InventoryAccess> InventoryAccesses { get; set; } = new List<InventoryAccess>();
        public ICollection<ItemLike> Likes { get; set; } = new List<ItemLike>();
        public ICollection<DiscussionPost> Posts { get; set; } = new List<DiscussionPost>();
    }

    public class Inventory
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsPublic { get; set; }
        public string? Tags { get; set; }

        [NotMapped]
        public string? TagsCsv { get => Tags; set => Tags = value; }

        public string OwnerId { get; set; } = string.Empty;
        public ApplicationUser? Owner { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public ICollection<InventoryField> Fields { get; set; } = new List<InventoryField>();
        public ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
        public ICollection<InventoryAccess> AccessList { get; set; } = new List<InventoryAccess>();
        public ICollection<DiscussionPost> DiscussionPosts { get; set; } = new List<DiscussionPost>();

        public string? CustomIdTemplateJson { get; set; } // store template
    }

    public enum FieldType { SingleLineText, MultiLineText, Number, Document, Image, TrueFalse }

    public class InventoryField
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public Inventory? Inventory { get; set; }

        [Required] public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public FieldType Type { get; set; } = FieldType.SingleLineText;
        public bool ShowInTable { get; set; } = true;
        public int SortOrder { get; set; } = 0;

        [NotMapped]
        public bool VisibleInTable { get => ShowInTable; set => ShowInTable = value; }
    }

    public class InventoryItem
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public Inventory? Inventory { get; set; }

        [MaxLength(100)]
        public string CustomId { get; set; } = string.Empty;

        public string CreatedById { get; set; } = string.Empty;
        public ApplicationUser? CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Timestamp] public byte[] RowVersion { get; set; } = Array.Empty<byte>();

        public ICollection<FieldValue> FieldValues { get; set; } = new List<FieldValue>();
        public ICollection<ItemLike> Likes { get; set; } = new List<ItemLike>();

        [NotMapped] public string? ValuesJson { get; set; }
    }

    public class FieldValue
    {
        public int Id { get; set; }
        public int InventoryItemId { get; set; }
        public InventoryItem? InventoryItem { get; set; }
        public int InventoryFieldId { get; set; }
        public InventoryField? InventoryField { get; set; }
        public string? Value { get; set; }
    }

    public class InventoryAccess
    {
        public int InventoryId { get; set; }
        public Inventory? Inventory { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;
    }

    public class ItemLike
    {
        public int InventoryItemId { get; set; }
        public InventoryItem? InventoryItem { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public DateTime LikedAt { get; set; } = DateTime.UtcNow;
    }

    public class DiscussionPost
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public Inventory? Inventory { get; set; }
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        public string Markdown { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
