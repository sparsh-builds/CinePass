using CinePass.Interfaces;

namespace CinePass.Services;

public class CardPaymentService : IPaymentService
{
    public void ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Card Payment Successful : ₹{amount}");
    }
}