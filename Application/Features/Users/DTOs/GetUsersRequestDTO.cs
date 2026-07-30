using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Application.Features.Users.DTOs
{
    public sealed class GetUsersRequestDTO
    {
        [Range(1, 100),
            Required(ErrorMessage = "Page number field is required. "), 
                         ]
        public int PageNumber { get; set; }
        [Range(1, 100), Required(ErrorMessage = "Page size is required.")]
        public int PageSize { get; set; }
        public string? Status { get; set; }
        public int? RoleId { get; set; }
        public bool? EmailVerified { get; set; }
        public bool? IsRevoked { get; set; }
    }
}
