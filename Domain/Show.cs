namespace Domain;

#nullable disable

public enum ShowType { Movie, Series }

public class Show : Entity
{
    public long RemoteId { get; set; }
    
    public string CoverImageURL { get; set; }
    
    public string Title { get; set; }
    
    public string Summary { get; set; }

    public double Rating { get; set; }
}