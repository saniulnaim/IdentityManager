using System.ComponentModel.DataAnnotations;

namespace IdentityManager.Models.ViewModels
{
    public class VerifyAuthenticatorViewModel
    {
        [Required]
        public string Code { get; set; }
        public string? ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }
}
