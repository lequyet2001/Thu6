using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Thu6.model
{
    public class updatePasswordModel
    {
        [Required]
        [FromHeader]
        public string Token { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required")]
        public string OldPassword { get; set; } = string.Empty;
        [Required(ErrorMessage = "Password is required")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*\W).*$",
    ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
