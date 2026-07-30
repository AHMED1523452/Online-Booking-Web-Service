using Stripe.TestHelpers.Issuing;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Features.Auth.DTOs
{
    public class ChangePasswordRequestDTO
    {
        [Required(ErrorMessage = "Old password is required. ")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "New password is required")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Confrim new password is required. "),Compare("NewPassword", ErrorMessage = "Invalid confirm new password")]
        public string ConfirmNewPassword { get; set; }
    }
}
