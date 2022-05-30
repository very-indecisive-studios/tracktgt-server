namespace Domain;

#nullable disable

public enum BookTrackingFormat { Digital, Physical, None }

public enum BookTrackingStatus { Completed, Reading, Paused, Planning  }

public enum BookTrackingOwnership { Owned, Loan, Wishlist }

public class BookTracking : Entity
{
    public string UserRemoteId { get; set; }
    
    public string BookRemoteId { get; set; }
    
    public float ChaptersRead { get; set; }

    public BookTrackingFormat Format { get; set; }

    public BookTrackingStatus Status { get; set; }
    
    public BookTrackingOwnership Ownership { get; set; }
}
