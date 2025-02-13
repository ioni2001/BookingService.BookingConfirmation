using BookingService.BookingConfirmation.Models.Entities;

namespace BookingService.BookingConfirmation.Services;

public interface IMailService
{
    void SendEmail(MailData mailData);
}
