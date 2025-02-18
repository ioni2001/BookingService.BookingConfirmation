namespace BookingService.BookingConfirmation.Models.Entities;

public class MailData
{
    public required string ToEmail { get; set; }

    public required string EmailSubject { get; set; }

    public required string EmailBody { get; set; }
}
