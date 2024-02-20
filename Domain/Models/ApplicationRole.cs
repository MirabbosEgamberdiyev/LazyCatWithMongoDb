using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace Domain.Models;

[CollectionName("roles")]
public class ApplicationRole : MongoIdentityRole<Guid>
{
    public ApplicationRole() : base()
    {
        
    }
    public ApplicationRole(string roleName) : base(roleName)
    {
        
    }
}
