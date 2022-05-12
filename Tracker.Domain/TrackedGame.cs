namespace Tracker.Domain;

#nullable disable

public enum TrackedGameFormat { Digital, Physical }

public enum TrackedGameStatus { Completed, Playing, Paused, Planning  }

public enum TrackedGameOwnership { Owned, Loan, Wishlist, Subscription }

public class TrackedGame : Entity
{
    public string UserRemoteId { get; set; }
    
    public long GameRemoteId { get; set; }
    
    public float HoursPlayed { get; set; }
    
    public string Platform { get; set; }
    
    public TrackedGameFormat? Format { get; set; }

    public TrackedGameStatus Status { get; set; }
    
    public TrackedGameOwnership Ownership { get; set; }
}
