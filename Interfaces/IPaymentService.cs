namespace CinePass.Interfaces;

public interface IPaymentService
{
    void ProcessPayment(decimal amount);
}