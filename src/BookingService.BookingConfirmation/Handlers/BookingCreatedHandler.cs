using BookingService.BookingConfirmation.Models.Events;
using BookingService.BookingConfirmation.Services;
using KafkaFlow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BookingService.BookingConfirmation.Handlers;

public class BookingCreatedHandler : IMessageHandler<BookingCreatedEvent>
{
    private readonly ILogger<BookingCreatedHandler> _logger;
    private readonly IMailService _mailService;

    public BookingCreatedHandler(
        ILogger<BookingCreatedHandler> logger,
        IMailService mailService)
    {
        _logger = logger;
        _mailService = mailService;
    }

    public async Task Handle(IMessageContext context, BookingCreatedEvent message)
    {
        _logger.LogInformation("New booking created event received");
        _logger.LogDebug(
            "Message: {MessageJson}",
            JsonConvert.SerializeObject(message, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            }));

        var subject = $"🎉 Congratulations, {message.UserName}! Your Booking is Confirmed!";
        var body = $@"
            Hello {message.UserName},

            We’re excited to let you know that your booking has been successfully confirmed! Here are the details of your reservation:

            - **Room:** {message.RoomName}
            - **Date:** {message.BookingDate:MMMM dd, yyyy}
            - **Time:** {message.StartTime} - {message.EndTime}
            - **Number of Persons:** {message.NumberOfPersons}

            If you have any questions or need assistance, feel free to reach out to us.

            Thank you for choosing our service!
            ";
        _mailService.SendEmail(new Models.Entities.MailData { EmailBody = body, EmailSubject = subject, ToEmail = message.Event });
    }
}

