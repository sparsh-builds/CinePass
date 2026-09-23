using CinePass.Exceptions;
using CinePass.Interfaces;
using CinePass.Models;

namespace CinePass.Services;

public class BookingService
{
    private readonly IPaymentService _paymentService;
    private readonly INotificationService _notificationService;
    private readonly FileService _fileService;

    public BookingService(
        IPaymentService paymentService,
        INotificationService notificationService,
        FileService fileService)
    {
        _paymentService = paymentService;
        _notificationService = notificationService;
        _fileService = fileService;
    }

    public void BookTicket(Booking booking)
    {
        if (booking.TicketCount <= 0)
        {
            throw new BookingException(
                "Invalid Ticket Count");
        }

        if (booking.Amount <= 0)
        {
            throw new BookingException(
                "Amount Must Be Greater Than Zero");
        }

        if (booking.Customer.WalletBalance <
            booking.Amount)
        {
            throw new BookingException(
                "Insufficient Balance");
        }

        _paymentService.ProcessPayment(
            booking.Amount);

        booking.Customer.WalletBalance -=
            booking.Amount;

        _notificationService.SendNotification(
            "Booking Successful");

        string bookingRecord =
$@"Customer : {booking.Customer.Name}
Movie : {booking.Show.Movie.Title}
Theater : {booking.Show.Theater.Name}
Show : {booking.Show.ShowTime}
Tickets : {booking.TicketCount}
Amount : {booking.Amount}
Remaining Balance : {booking.Customer.WalletBalance}
Date : {DateTime.Now:yyyy-MM-dd}
--------------------------------";

        _fileService.SaveBooking(
            bookingRecord);

        Console.WriteLine(
            "\nBooking Completed Successfully");
    }
}