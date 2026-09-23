namespace CinePass.Models;

public class Movie : BaseEntity
{
    public string Title { get; set; } = "";

    public string Language { get; set; } = "";

    public int Duration { get; set; }

    public decimal TicketPrice { get; set; }

    public Movie(
        int id,
        string title,
        string language,
        int duration,
        decimal ticketPrice)
    {
        Id = id;
        Title = title;
        Language = language;
        Duration = duration;
        TicketPrice = ticketPrice;
    }
}