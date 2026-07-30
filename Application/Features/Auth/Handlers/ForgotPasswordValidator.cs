using Application.Features.Auth.Commands.ForgotPassword;
using FluentValidation;
using FluentValidation.Validators;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Features.Auth.Handlers
{
    public  class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordValidator()
        {
            RuleFor(x => x.requestDTO.Email)
            .NotEmpty()
            .WithMessage("Email is required.")

            .EmailAddress()
            .WithMessage("Invalid email format.")

            .MaximumLength(256);
        }
    }
}
