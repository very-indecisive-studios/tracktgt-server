namespace Domain;

#nullable disable

public enum BookTrackingFormat { Digital, Physical }

public enum BookTrackingStatus { Completed, Reading, Paused, Planning  }

public enum BookTrackingOwnership { Owned, Loan }

public class BookTracking : Entity
{
    public string UserRemoteId { get; set; }
    
    public string BookRemoteId { get; set; }
    
    public int ChaptersRead { get; set; }

    public BookTrackingFormat Format { get; set; }

    public BookTrackingStatus Status { get; set; }
    
    public BookTrackingOwnership Ownership { get; set; }
}
