

namespace Domain.Entities;

[CollectionName("users")]
public class ApplicationUser : MongoIdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    public List<string> UserRoles { get; set; } = new();
}
