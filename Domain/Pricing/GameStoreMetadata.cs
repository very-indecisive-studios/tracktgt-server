namespace Domain.Pricing;

#nullable disable

public class GameStoreMetadata : Entity
{
    public long GameRemoteId { get; set; }
 
    public GameStoreType GameStoreType { get; set; }
    
    public string Region { get; set; }
    
    public string GameStoreId { get; set; }
}
