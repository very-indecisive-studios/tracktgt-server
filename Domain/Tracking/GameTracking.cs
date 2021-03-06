namespace Domain.Tracking;

#nullable disable

public enum GameTrackingFormat { Digital, Physical }

public enum GameTrackingStatus { Completed, Playing, Paused, Planning  }

public enum GameTrackingOwnership { Owned, Loan, Subscription }

public class GameTracking : Entity
{
    public string UserRemoteId { get; set; }
    
    public long GameRemoteId { get; set; }
    
    public float HoursPlayed { get; set; }
    
    public string Platform { get; set; }
    
    public GameTrackingFormat Format { get; set; }

    public GameTrackingStatus Status { get; set; }
    
    public GameTrackingOwnership Ownership { get; set; }
}
