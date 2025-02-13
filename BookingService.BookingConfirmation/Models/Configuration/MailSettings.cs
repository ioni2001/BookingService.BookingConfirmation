namespace BookingService.BookingConfirmation.Models.Configuration;

public class MailSettings
{
    public required string Server{ get; set; }

    public required int Port{ get; set; }

    public required string SenderEmail{ get; set; }

    public required string Password{ get; set; }
}
