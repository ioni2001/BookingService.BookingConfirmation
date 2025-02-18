namespace BookingService.BookingConfirmation.Models.Events;

public class BookingCreatedEvent
{
    public required string RoomName { get; set; }

    public required DateOnly BookingDate { get; set; }

    public required string StartTime { get; set; }

    public required string EndTime { get; set; }

    public required int NumberOfPersons { get; set; }

    public required string UserName { get; set; }

    public required string Event { get; set; }
}
