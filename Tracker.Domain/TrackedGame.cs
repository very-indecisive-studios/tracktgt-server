namespace Tracker.Domain;

public enum GameFormat { Digital, Physical, Subscription }

public enum GameStatus { Current, Playing, Paused, Planning  }

public enum GameOwnership { Owned, Loan, Wishlist }

public class TrackedGame : Entity
{
    public Guid UserId { get; set; }
    
    public long RemoteId { get; set; }
    
    public float HoursPlayed { get; set; }
    
    public string Platform { get; set; }
    
    public GameFormat Format { get; set; }

    public GameStatus Status { get; set; }
    
    public GameOwnership Ownership { get; set; }
}
