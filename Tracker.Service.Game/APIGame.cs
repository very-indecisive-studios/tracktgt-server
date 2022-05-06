namespace Tracker.Service.Game;

public class APIGame
{
    public long Id { get; set; }
    
    public string CoverImageURL { get; set; }
    
    public string Title { get; set; }
    
    public string Summary { get; set; }
    
    public double Rating { get; set; }
    
    public List<string> Platforms { get; set; }
    
    public List<string> Companies { get; set; }
}
