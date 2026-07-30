using Application.Features.Auth.Commands.RevokeTokenPassenger;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.Handlers
{
    public class RevokePassengerValidator : AbstractValidator<RevokeRefreshTokenCommand>
    {
        public RevokePassengerValidator()
        {
            RuleFor(x => x.requestDTO.RefreshToken)
           .NotEmpty()
           .WithMessage("Refresh token is required.");
        }
    }
}
