namespace Domain.User;

#nullable disable

public class User : Entity
{
    public string RemoteId { get; set; }
    
    public string Email { get; set; }
    
    public string UserName { get; set; }
    
    public string ProfilePictureURL { get; set; }
    
    public string Bio { get; set; }
}