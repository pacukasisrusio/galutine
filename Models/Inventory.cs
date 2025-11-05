using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using galutine.Models;
public class Inventory_Old
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public bool IsPublic { get; set; }
    public string? Tags { get; set; }
    public string? Category { get; set; }
    public string? ImageUrl { get; set; }


    public string OwnerId { get; set; } = "";
    public ApplicationUser? Owner { get; set; }

    // Add this
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    public ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
    public ICollection<InventoryField> Fields { get; set; } = new List<InventoryField>();
    public ICollection<DiscussionPost> DiscussionPosts { get; set; } = new List<DiscussionPost>();
}
