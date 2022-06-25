namespace Domain.User;

#nullable disable

public class Follow : Entity
{
    public string FollowerUserId { get; set; }
    
    public string FollowingUserId { get; set; }
}