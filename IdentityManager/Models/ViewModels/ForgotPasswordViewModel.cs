using System.ComponentModel.DataAnnotations;

namespace IdentityManager.Models.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        public string Email { get; set; }
    }
}
