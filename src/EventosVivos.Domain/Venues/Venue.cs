namespace EventosVivos.Domain.Venues;

public class Venue
{
    public int Id { get; private set; }
    public string Name { get; private set; } = default!;
    public int Capacity { get; private set; }
    public string City { get; private set; } = default!;

    private Venue() { }

    public Venue(int id, string name, int capacity, string city)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
        City = city;
    }
}
