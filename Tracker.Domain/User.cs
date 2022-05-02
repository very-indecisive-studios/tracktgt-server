namespace Tracker.Domain;

public class User : Entity
{
    public string Email { get; set; }
    
    public string UserName { get; set; }
    
    public string DisplayName { get; set; }
}