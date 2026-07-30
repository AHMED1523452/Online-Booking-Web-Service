using Application.Common.Interfaces;
using FluentEmail.Core;
using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IFluentEmail fluentEmail;

        public EmailService(IFluentEmail fluentEmail)
        {
            this.fluentEmail = fluentEmail;
        }

        public async Task SendEmail(string toEmail, string Subject, string Body)
        {
            var fluent = await fluentEmail.
                To(toEmail)
                .Subject(Subject)
                .Body(Body, isHtml: true)
                .SendAsync();
        }
    }
}
