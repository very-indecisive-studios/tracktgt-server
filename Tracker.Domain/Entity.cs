using System.ComponentModel.DataAnnotations;

namespace Tracker.Domain;

public abstract class Entity
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public DateTime CreatedOn { get; set; }

    public DateTime? LastModifiedOn { get; set; }
}