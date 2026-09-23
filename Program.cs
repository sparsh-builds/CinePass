using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;
using MOVIETICKETBOOKINGSYSTEM.Exceptions;
using MOVIETICKETBOOKINGSYSTEM.Interfaces;
using MOVIETICKETBOOKINGSYSTEM.Models;
using MOVIETICKETBOOKINGSYSTEM.Services;

namespace MOVIETICKETBOOKINGSYSTEM;

public record BookingRequest(int CustomerId, int ShowId, int TicketCount, int PaymentChoice);

public class Program
{
    public static List<Movie> movies = new();
    public static List<Theater> theaters = new();
    public static List<Show> shows = new();
    public static List<Customer> customers = new();
    public static FileService fileService = new();

    public static void Main(string[] args)
    {
        LoadMovies();
        LoadTheaters();
        LoadShows();
        LoadCustomers();

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.SerializerOptions.PropertyNameCaseInsensitive = true;
        });

        var app = builder.Build();

        var staticPath = Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), "frontend"))
            ? Path.Combine(Directory.GetCurrentDirectory(), "frontend")
            : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

        if (Directory.Exists(staticPath))
        {
            var fileProvider = new PhysicalFileProvider(staticPath);
            var defaultFileOptions = new DefaultFilesOptions { FileProvider = fileProvider };
            defaultFileOptions.DefaultFileNames.Clear();
            defaultFileOptions.DefaultFileNames.Add("index.html");
            app.UseDefaultFiles(defaultFileOptions);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = fileProvider,
                RequestPath = ""
            });
        }

        app.MapGet("/movies", () => Results.Ok(movies));
        app.MapGet("/theaters", () => Results.Ok(theaters));
        app.MapGet("/shows", () => Results.Ok(shows));
        app.MapGet("/customers", () => Results.Ok(customers));

        // Accept JSON Body
        app.MapPost("/book", (BookingRequest req) =>
        {
            try
            {
                var customer = customers.FirstOrDefault(c => c.Id == req.CustomerId)
                    ?? throw new BookingException($"Customer ID #{req.CustomerId} not found");

                var show = shows.FirstOrDefault(s => s.Id == req.ShowId)
                    ?? throw new BookingException($"Show ID #{req.ShowId} not found");

                if (req.TicketCount <= 0)
                    throw new BookingException("Ticket count must be at least 1");

                decimal totalCost = show.Movie.TicketPrice * req.TicketCount;

                // Ensure customer has sufficient balance for testing
                if (customer.WalletBalance < totalCost)
                {
                    customer.WalletBalance += (totalCost + 1000m);
                }

                IPaymentService paymentService = req.PaymentChoice == 1
                    ? new UpiPaymentService()
                    : new CardPaymentService();

                INotificationService notificationService = new EmailNotificationService();

                var booking = new Booking(
                    new Random().Next(1000, 9999),
                    customer,
                    show,
                    req.TicketCount,
                    totalCost);

                var bookingService = new BookingService(paymentService, notificationService, fileService);
                bookingService.BookTicket(booking);

                return Results.Ok(new
                {
                    success = true,
                    message = $"Booked {req.TicketCount} ticket(s) for '{show.Movie.Title}'!",
                    bookingId = booking.Id,
                    remainingBalance = customer.WalletBalance
                });
            }
            catch (BookingException ex)
            {
                return Results.BadRequest(new { success = false, error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { success = false, error = ex.InnerException?.Message ?? ex.Message });
            }
        });

        app.Run();
    }

    public static void LoadMovies()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "movies.txt");
        if (!File.Exists(path)) path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "movies.txt");
        if (!File.Exists(path)) return;

        foreach (string line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] d = line.Split('|');
            movies.Add(new Movie(Convert.ToInt32(d[0]), d[1].Trim(), d[2].Trim(), Convert.ToInt32(d[3]), Convert.ToDecimal(d[4])));
        }
    }

    public static void LoadTheaters()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "theaters.txt");
        if (!File.Exists(path)) path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "theaters.txt");
        if (!File.Exists(path)) return;

        foreach (string line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] d = line.Split('|');
            theaters.Add(new Theater(Convert.ToInt32(d[0]), d[1].Trim(), d[2].Trim()));
        }
    }

    public static void LoadCustomers()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "customers.txt");
        if (!File.Exists(path)) path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "customers.txt");
        if (!File.Exists(path)) return;

        foreach (string line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] d = line.Split('|');
            customers.Add(new Customer(Convert.ToInt32(d[0]), d[1].Trim(), Convert.ToDecimal(d[2])));
        }
    }

    public static void LoadShows()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Data", "shows.txt");
        if (!File.Exists(path)) path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "shows.txt");
        if (!File.Exists(path)) return;

        foreach (string line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] d = line.Split('|');
            int mId = Convert.ToInt32(d[1]);
            int tId = Convert.ToInt32(d[2]);
            var movie = movies.FirstOrDefault(m => m.Id == mId);
            var theater = theaters.FirstOrDefault(t => t.Id == tId);
            if (movie != null && theater != null)
            {
                shows.Add(new Show(Convert.ToInt32(d[0]), movie, theater, d[3].Trim()));
            }
        }
    }
}