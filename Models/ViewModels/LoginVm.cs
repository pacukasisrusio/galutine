using System.ComponentModel.DataAnnotations;
namespace galutine.Models.ViewModels
{
    public class LoginVm
    {
        [Required][EmailAddress] public string Email { get; set; } = string.Empty;
        [Required] public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
