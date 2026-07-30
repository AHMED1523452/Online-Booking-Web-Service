using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Features.Auth.DTOs
{
    public class ResetPasswordRequestDTO
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
