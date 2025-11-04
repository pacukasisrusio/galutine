using Microsoft.AspNetCore.Identity;

namespace galutine.Models
{
public class AppUser : IdentityUser
{
public string? FullName { get; set; }
public bool IsBlocked { get; set; } = false;
}
}