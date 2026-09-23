namespace CinePass.Models;

public class Show : BaseEntity
{
    public Movie Movie { get; set; }

    public Theater Theater { get; set; }

    public string ShowTime { get; set; }

    public Show(
        int id,
        Movie movie,
        Theater theater,
        string showTime)
    {
        Id = id;
        Movie = movie;
        Theater = theater;
        ShowTime = showTime;
    }
}