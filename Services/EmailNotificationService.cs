using CinePass.Interfaces;

namespace CinePass.Services;

public class EmailNotificationService : INotificationService
{
    public void SendNotification(string message)
    {
        Console.WriteLine($"Email Sent : {message}");
    }
}