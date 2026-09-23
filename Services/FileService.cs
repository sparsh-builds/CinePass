using System.IO;

namespace CinePass.Services;

public class FileService
{
    private readonly string _filePath;

    public FileService()
    {
        Directory.CreateDirectory("Bookings");

        _filePath = Path.Combine("Bookings", "bookings.txt");
    }

    public void SaveBooking(string bookingRecord)
    {
        File.AppendAllText(
            _filePath,
            bookingRecord + Environment.NewLine);
    }

    public void DisplayBookingHistory()
    {
        if (File.Exists(_filePath))
        {
            Console.WriteLine("\nBooking History\n");

            string content =
                File.ReadAllText(_filePath);

            Console.WriteLine(content);
        }
        else
        {
            Console.WriteLine(
                "No Booking History Found");
        }
    }
}