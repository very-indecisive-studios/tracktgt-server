namespace Domain;

#nullable disable

public class Activity : Entity
{
    public string UserRemoteId { get; set; }
    
    public string MediaRemoteId { get; set; }

    public string Status { get; set; }
    
    public int NoOf { get; set; } 
    
    public string MediaType { get; set; }
    
    public bool NewlyAdded { get; set; }
}