using BookingService.BookingConfirmation.Models.Configuration;
using BookingService.BookingConfirmation.Models.Entities;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace BookingService.BookingConfirmation.Services;

public class MailService : IMailService
{
    private readonly MailSettings _mailSettings;

    public MailService(IOptions<MailSettings> mailSettingsOptions)
    {
        _mailSettings = mailSettingsOptions.Value;
    }

    public void SendEmail(MailData mailData)
    {
        string fromEmail = _mailSettings.SenderEmail;

        var mailMessage = new MailMessage(fromEmail, mailData.ToEmail, mailData.EmailSubject, mailData.EmailBody);

        var smtpClient = new SmtpClient(_mailSettings.Server, _mailSettings.Port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromEmail, _mailSettings.Password),
        };

        smtpClient.Send(mailMessage);
    }
}
