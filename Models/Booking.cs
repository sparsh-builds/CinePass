namespace CinePass.Models;

public class Booking : BaseEntity
{
    public Customer Customer { get; set; }

    public Show Show { get; set; }

    public int TicketCount { get; set; }

    public decimal Amount { get; set; }

    public DateTime BookingDate { get; set; }

    public Booking(
        int id,
        Customer customer,
        Show show,
        int ticketCount,
        decimal amount)
    {
        Id = id;
        Customer = customer;
        Show = show;
        TicketCount = ticketCount;
        Amount = amount;
        BookingDate = DateTime.Now;
    }
}