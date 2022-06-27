namespace Domain.User;

#nullable disable

public class PricingUserPreference : Entity
{
    public string UserRemoteId { get; set; }
    
    public string EShopRegion { get; set; }
}
