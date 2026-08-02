using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Domain.Entities
{
    public class RefreshTokens
    {
        public long Id { get; set; }

        public long UserId { get; set; }
        public passenger User { get; set; } = default!;

        public string TokenHash { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpiresAt { get; set; }
        public DateTime? RevokedAt { get; set; }
        public bool? IsRevoked { get; set; } = false;

        public long ReplacedByTokenId { get; set; }
    }
}
