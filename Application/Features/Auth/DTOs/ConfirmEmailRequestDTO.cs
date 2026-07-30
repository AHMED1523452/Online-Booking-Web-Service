using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Features.Auth.DTOs
{
    public class ConfirmEmailRequestDTO
    {
        [Required(ErrorMessage = "Email field is require."),
              EmailAddress(ErrorMessage  = "Invalid email. "),
            MaxLength(256)]
        public string Email { get; set; }

        [Required(ErrorMessage = "Token field is required. ")]
        public string Token { get; set; }
    }
}
