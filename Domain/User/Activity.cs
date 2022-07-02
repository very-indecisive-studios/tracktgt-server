namespace Domain;

#nullable disable

public enum ActivityMediaType { Game, Show, Book }
public enum ActivityAction { Add, Update, Remove }

public class Activity : Entity
{
    public string UserRemoteId { get; set; }
    
    public string MediaRemoteId { get; set; }

    public string MediaStatus { get; set; }
    
    public int NoOf { get; set; } 
    
    public ActivityMediaType MediaType { get; set; }
    
    public ActivityAction Action { get; set; }
}