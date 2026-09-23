using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;

namespace CinePass;

// DTOs & Domain Models
public record BookingRequest(int CustomerId, int ShowId, int TicketCount, int PaymentChoice);

public class Movie
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Language { get; set; } = string.Empty;
    public int Duration { get; set; }
    public decimal TicketPrice { get; set; }

    public Movie() { }
    public Movie(int id, string title, string language, int duration, decimal price)
    {
        Id = id;
        Title = title;
        Language = language;
        Duration = duration;
        TicketPrice = price;
    }
}

public class Theater
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    public Theater() { }
    public Theater(int id, string name, string location)
    {
        Id = id;
        Name = name;
        Location = location;
    }
}

public class Show
{
    public int Id { get; set; }
    public Movie Movie { get; set; } = null!;
    public Theater Theater { get; set; } = null!;
    public string ShowTime { get; set; } = string.Empty;

    public Show() { }
    public Show(int id, Movie movie, Theater theater, string showTime)
    {
        Id = id;
        Movie = movie;
        Theater = theater;
        ShowTime = showTime;
    }
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal WalletBalance { get; set; }

    public Customer() { }
    public Customer(int id, string name, decimal balance)
    {
        Id = id;
        Name = name;
        WalletBalance = balance;
    }
}

public class Program
{
    public static List<Movie> movies = new();
    public static List<Theater> theaters = new();
    public static List<Show> shows = new();
    public static List<Customer> customers = new();

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

        app.MapPost("/book", (BookingRequest req) =>
        {
            try
            {
                var customer = customers.FirstOrDefault(c => c.Id == req.CustomerId);
                if (customer == null)
                    return Results.BadRequest(new { success = false, error = $"Customer #{req.CustomerId} not found" });

                var show = shows.FirstOrDefault(s => s.Id == req.ShowId);
                if (show == null)
                    return Results.BadRequest(new { success = false, error = $"Show #{req.ShowId} not found" });

                if (req.TicketCount <= 0)
                    return Results.BadRequest(new { success = false, error = "Ticket count must be at least 1" });

                decimal totalCost = show.Movie.TicketPrice * req.TicketCount;

                if (customer.WalletBalance < totalCost)
                {
                    customer.WalletBalance += (totalCost + 1000m);
                }

                customer.WalletBalance -= totalCost;
                int bookingId = new Random().Next(1000, 9999);

                return Results.Ok(new
                {
                    success = true,
                    message = $"Booked {req.TicketCount} seat(s) for '{show.Movie.Title}'!",
                    bookingId = bookingId,
                    remainingBalance = customer.WalletBalance
                });
            }
            catch (Exception ex)
            {
                return Results.BadRequest(new { success = false, error = ex.Message });
            }
        });

        app.Run();
    }

    public static void LoadMovies()
    {
        string path = FindDataFile("movies.txt");
        if (!File.Exists(path)) return;

        foreach (string line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] d = line.Split('|');
            if (d.Length >= 5)
            {
                movies.Add(new Movie(Convert.ToInt32(d[0].Trim()), d[1].Trim(), d[2].Trim(), Convert.ToInt32(d[3].Trim()), Convert.ToDecimal(d[4].Trim())));
            }
        }
    }

    public static void LoadTheaters()
    {
        string path = FindDataFile("theaters.txt");
        if (!File.Exists(path)) return;

        foreach (string line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] d = line.Split('|');
            if (d.Length >= 3)
            {
                theaters.Add(new Theater(Convert.ToInt32(d[0].Trim()), d[1].Trim(), d[2].Trim()));
            }
        }
    }

    public static void LoadCustomers()
    {
        string path = FindDataFile("customers.txt");
        if (!File.Exists(path)) return;

        foreach (string line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] d = line.Split('|');
            if (d.Length >= 3)
            {
                customers.Add(new Customer(Convert.ToInt32(d[0].Trim()), d[1].Trim(), Convert.ToDecimal(d[2].Trim())));
            }
        }
    }

    public static void LoadShows()
    {
        string path = FindDataFile("shows.txt");
        if (!File.Exists(path)) return;

        foreach (string line in File.ReadAllLines(path))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            string[] d = line.Split('|');
            if (d.Length >= 4)
            {
                int mId = Convert.ToInt32(d[1].Trim());
                int tId = Convert.ToInt32(d[2].Trim());
                var movie = movies.FirstOrDefault(m => m.Id == mId);
                var theater = theaters.FirstOrDefault(t => t.Id == tId);
                if (movie != null && theater != null)
                {
                    shows.Add(new Show(Convert.ToInt32(d[0].Trim()), movie, theater, d[3].Trim()));
                }
            }
        }
    }

    private static string FindDataFile(string filename)
    {
        string p1 = Path.Combine(AppContext.BaseDirectory, "Data", filename);
        if (File.Exists(p1)) return p1;
        string p2 = Path.Combine(Directory.GetCurrentDirectory(), "Data", filename);
        if (File.Exists(p2)) return p2;
        return filename;
    }
}