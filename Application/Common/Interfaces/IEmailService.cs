using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Common.Interfaces
{
    public interface IEmailService
    {
        Task SendEmail(string toEmail, string Subject, string Body);
    }
}
