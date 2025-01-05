using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace ShoppingService.Service;

public class EmailSender : IEmailSender
{
    private readonly string _smtpServer = "smtp.gmail.com"; 
    private readonly string _smtpUser = "neaca.radu309@gmail.com";
    private readonly string _smtpPass = "pzts fsts zsly oqay"; 
    private readonly int _smtpPort = 587; 
    
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var mailMessage = new MailMessage
        {
            From = new MailAddress(_smtpUser),
            Subject = subject,
            Body = message,
            IsBodyHtml = true
        };

        mailMessage.To.Add(email);

        using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
        {
            smtpClient.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
            smtpClient.EnableSsl = true;

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}