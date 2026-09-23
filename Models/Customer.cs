namespace CinePass.Models;

public class Customer : BaseEntity
{
    public string Name { get; set; } = "";

    public decimal WalletBalance { get; set; }

    public Customer(
        int id,
        string name,
        decimal walletBalance)
    {
        Id = id;
        Name = name;
        WalletBalance = walletBalance;
    }
}