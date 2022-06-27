namespace Domain.Wishlist;

#nullable disable

public class GameWishlist : Entity
{
    public string UserRemoteId { get; set; }
    
    public long GameRemoteId { get; set; }

    public string Platform { get; set; }
}
