namespace Domain;

#nullable disable

public class User : Entity
{
    public string RemoteId { get; set; }
    
    public string Email { get; set; }
    
    public string UserName { get; set; }
}