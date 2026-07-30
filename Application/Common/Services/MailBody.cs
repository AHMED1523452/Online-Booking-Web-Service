using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Services
{
    public static class MailBody

    {
        public static async Task<string> mailBody(passenger passenger, string token, CancellationToken canellationToken)
        {
            var resetLink = $"https://frontend.com/reset-password?email={passenger.email}&token={Uri.EscapeDataString(token)}";

            string htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; padding:20px; border:1px solid #ddd; border-radius:10px;'>
                        <h2>Hello {passenger.name},</h2>
                        <p>You requested a password reset.</p>
                        <p><strong>Your token is:</strong> {token}</p>
                        <p>Or you can click the link below to reset your password directly:</p>
                        <a href='{resetLink}' style='display:inline-block; padding:10px 20px; background-color:#007bff; color:#fff; text-decoration:none; border-radius:5px;'>Reset Password</a>
                        <p style='margin-top:15px;'>If you did not request this, please ignore this email.</p>
                    </div>
                      ";
            return htmlBody;
        }

        public static async Task<string> ConfirmEamilMailBody(passenger passenger, string token, CancellationToken cancellationToken)
        {
            var confirmationLink = $"https://frontend.com/confirm-email-link?email={passenger.email}&token={Uri.EscapeDataString(token)}";
            string htmlBody = $@"
             <div style='font-family:Arial,Helvetica,sans-serif;
                                                                max-width:650px;
                                                                margin:auto;
                                                                padding:30px;
                                                                border:1px solid #e5e5e5;
                                                                border-radius:10px;
                                                                background-color:#ffffff;'>

                <h2 style='color:#2c3e50;'>
                    Welcome, {passenger.name}! 👋
                </h2>

                <p style='font-size:16px;color:#555;'>
                    Thank you for registering with <strong>TravelBooking</strong>.
                </p>

                <p style='font-size:16px;color:#555;'>
                    To activate your account, please use the confirmation token below.
                </p>

                <div style='margin:30px 0;
                            background:#f4f4f4;
                            border:2px dashed #007bff;
                            border-radius:8px;
                            padding:20px;
                            text-align:center;'>

                    <p style='margin:0;font-size:14px;color:#666;'>
                        Email Confirmation Token
                    </p>

                    <h2 style='letter-spacing:3px;
                               color:#007bff;
                               margin:10px 0;'>
                        {token}
                    </h2>

                </div>

                <p style='font-size:15px;color:#555;'>
                    Copy this token and use it in the
                    <strong>Confirm Email</strong> endpoint.
                </p>

                <div style='background:#eef6ff;
                            border-left:4px solid #007bff;
                            padding:15px;
                            margin-top:20px;'>

                    <strong>Note:</strong><br/>
                    This token will expire in <strong>24 hours</strong>.
                </div>

                <hr style='margin:30px 0;'>

                <p style='font-size:14px;color:#888;'>
                    If you didn't create this account, you can safely ignore this email.
                </p>

                <p style='margin-top:30px;'>
                    Best Regards,<br/>
                    <strong>TravelBooking Team</strong>
                </p>

            </div>";
            return htmlBody;
        }

        public static async Task<string> ChangeEmailHtmlBody(passenger passenger, string Token, CancellationToken cancellationToken)
        {
            var confirmationLink = $"https://frontend.com/confirm-email-link?email={passenger.email}&token={Uri.EscapeDataString(Token)}";

            string htmlBody = $@"

             <div style='font-family:Arial,Helvetica,sans-serif;
                            max-width:650px;
                            margin:auto;
                            padding:30px;
                            border:1px solid #e5e5e5;
                            border-radius:10px;
                            background-color:#ffffff;'>

                        <h2 style='color:#2c3e50;'>
                            Hello, {passenger.name}
                        </h2>

                        <p style='font-size:16px;color:#555;'>
                            We received a request to change the email address for your
                            <strong>TravelBooking</strong> account.
                        </p>

                        <p style='font-size:16px;color:#555;'>
                            To complete the email change, use the confirmation token below.
                        </p>

                        <div style='margin:30px 0;
                                    background:#f8f9fa;
                                    border:2px dashed #28a745;
                                    border-radius:8px;
                                    padding:20px;
                                    text-align:center;'>

                            <p style='margin:0;font-size:14px;color:#666;'>
                                Email Change Token
                            </p>

                            <h2 style='letter-spacing:3px;
                                       color:#28a745;
                                       margin:10px 0;'>
                                {Token}
                            </h2>

                        </div>

                        <p style='font-size:15px;color:#555;'>
                            Copy this token and submit it to the
                            <strong>Confirm Email Change</strong> endpoint.
                        </p>

                        <div style='background:#fff8e6;
                                    border-left:4px solid #f39c12;
                                    padding:15px;
                                    margin-top:20px;'>

                            <strong>Security Notice:</strong><br/>
                            This token expires in <strong>24 hours</strong>.
                        </div>

                        <hr style='margin:30px 0;'>

                        <p style='font-size:14px;color:#888;'>
                            If you didn't request this email change, simply ignore this email.
                            Your current email address will remain unchanged.
                        </p>

                        <p style='margin-top:30px;'>
                            Best Regards,<br/>
                            <strong>TravelBooking Team</strong>
                        </p>

             </div>";
            return htmlBody;
        }
    }
}
