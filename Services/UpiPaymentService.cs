using CinePass.Interfaces;

namespace CinePass.Services;

public class UpiPaymentService : IPaymentService
{
    public void ProcessPayment(decimal amount)
    {
        Console.WriteLine($"UPI Payment Successful : ₹{amount}");
    }
}