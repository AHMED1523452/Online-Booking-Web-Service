using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.DTOs
{
    public class RevokeRefreshTokenRequestDTO
    {
        //. i sent the user id in the request body to revoke the refresh token for that specific user,
        //. and after that we will bring the last refresh token for that user and revoke it,
        //                               and also we will delete the user from the database as soft delete.

        public long UserId { get; set;  }
    }
}
