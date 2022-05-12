namespace Tracker.Domain;

#nullable disable

public enum GameFormat { Digital, Physical }

public enum GameStatus { Current, Playing, Paused, Planning  }

public enum GameOwnership { Owned, Loan, Wishlist, Subscription }

public class TrackedGame : Entity
{
    public string UserRemoteId { get; set; }
    
    public long GameRemoteId { get; set; }
    
    public float HoursPlayed { get; set; }
    
    public string Platform { get; set; }
    
    public GameFormat Format { get; set; }

    public GameStatus Status { get; set; }
    
    public GameOwnership Ownership { get; set; }
}
