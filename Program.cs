using CinePass.Exceptions;
using CinePass.Interfaces;
using CinePass.Models;
using CinePass.Services;

namespace MOVIETICKETBOOKINGSYSTEM;

public class Program
{
    public static List<Movie> movies = new();
    public static List<Theater> theaters = new();
    public static List<Show> shows = new();
    public static List<Customer> customers = new();
    public static FileService fileService = new();

    public static void Main(string[] args)
    {
        // 1. Existing file loading
        LoadMovies();
        LoadTheaters();
        LoadShows();
        LoadCustomers();

        // 2. Start lightweight Web Server for Render
        var builder = WebApplication.CreateBuilder(args);
        var app = builder.Build();

        app.MapGet("/", () => "CinePass Ticket Booking Engine API is Live on Render!");

        app.MapGet("/movies", () => movies);
        app.MapGet("/theaters", () => theaters);
        app.MapGet("/shows", () => shows);
        app.MapGet("/customers", () => customers);

        app.MapPost("/book", (int customerId, int showId, int ticketCount, int paymentChoice) =>
        {
            try
            {
                var customer = customers.FirstOrDefault(c => c.Id == customerId)
                    ?? throw new BookingException("Invalid Customer");

                var show = shows.FirstOrDefault(s => s.Id == showId)
                    ?? throw new BookingException("Invalid Show");

                if (ticketCount <= 0) throw new BookingException("Invalid Ticket Count");

                decimal amount = show.Movie.TicketPrice * ticketCount;

                IPaymentService paymentService = paymentChoice == 1 
                    ? new UpiPaymentService() 
                    : new CardPaymentService();

                INotificationService notificationService = new EmailNotificationService();

                var booking = new Booking(
                    new Random().Next(1000, 9999), 
                    customer, 
                    show, 
                    ticketCount, 
                    amount);

                var bookingService = new BookingService(paymentService, notificationService, fileService);
                bookingService.BookTicket(booking);

                return Results.Ok(new { Message = "Booking Confirmed", BookingId = booking });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { Error = ex.Message });
            }
        });

        app.Run();
    }

    public static void LoadMovies()
    {
        string path = Path.Combine("Data", "movies.txt");
        if (!File.Exists(path)) return;
        foreach (string line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] data = line.Split('|');
            movies.Add(new Movie(Convert.ToInt32(data[0]), data[1], data[2], Convert.ToInt32(data[3]), Convert.ToDecimal(data[4])));
        }
    }

    public static void LoadTheaters()
    {
        string path = Path.Combine("Data", "theaters.txt");
        if (!File.Exists(path)) return;
        foreach (string line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] data = line.Split('|');
            theaters.Add(new Theater(Convert.ToInt32(data[0]), data[1], data[2]));
        }
    }

    public static void LoadCustomers()
    {
        string path = Path.Combine("Data", "customers.txt");
        if (!File.Exists(path)) return;
        foreach (string line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] data = line.Split('|');
            customers.Add(new Customer(Convert.ToInt32(data[0]), data[1], Convert.ToDecimal(data[2])));
        }
    }

    public static void LoadShows()
    {
        string path = Path.Combine("Data", "shows.txt");
        if (!File.Exists(path)) return;
        foreach (string line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] data = line.Split('|');
            int movieId = Convert.ToInt32(data[1]);
            int theaterId = Convert.ToInt32(data[2]);
            Movie? movie = movies.FirstOrDefault(m => m.Id == movieId);
            Theater? theater = theaters.FirstOrDefault(t => t.Id == theaterId);
            if (movie != null && theater != null)
            {
                shows.Add(new Show(Convert.ToInt32(data[0]), movie, theater, data[3]));
            }
        }
    }
}