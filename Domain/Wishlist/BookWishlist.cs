namespace Domain.Wishlist;

#nullable disable

public class BookWishlist : Entity
{
    public string UserRemoteId { get; set; }
    
    public string BookRemoteId { get; set; }
}
