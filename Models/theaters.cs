namespace CinePass.Models;

public class Theater : BaseEntity
{
    public string Name { get; set; } = "";

    public string Location { get; set; } = "";

    public Theater(
        int id,
        string name,
        string location)
    {
        Id = id;
        Name = name;
        Location = location;
    }
}