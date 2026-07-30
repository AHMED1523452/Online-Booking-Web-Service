using Application.Features.Auth.Commands.ChangePassword;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.Handlers
{
    public sealed class ChangePasswordValidator : AbstractValidator<ChangePassengerPasswordCommand>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.requestDTO.OldPassword)
           .NotEmpty();

            RuleFor(x => x.requestDTO.NewPassword)
                .NotEmpty()
                .MinimumLength(8)
                .Matches("[A-Z]")
                .WithMessage("Password must contain an uppercase letter.")
                .Matches("[a-z]")
                .WithMessage("Password must contain a lowercase letter.")
                .Matches("[0-9]")
                .WithMessage("Password must contain a number.")
                .Matches("[^a-zA-Z0-9]")
                .WithMessage("Password must contain a special character.");

            RuleFor(x => x.requestDTO.ConfirmNewPassword)
                .Equal(x => x.requestDTO.NewPassword)
                .WithMessage("Passwords do not match.");
        }
    }
}
