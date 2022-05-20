namespace Domain;

#nullable disable

public class Game : Entity
{
    public long RemoteId { get; set; }
    
    public string CoverImageURL { get; set; }
    
    public string Title { get; set; }
    
    public string Summary { get; set; }
    
    public double Rating { get; set; }
    
    public string PlatformsString { get; set; }
    
    public string CompaniesString { get; set; }
}