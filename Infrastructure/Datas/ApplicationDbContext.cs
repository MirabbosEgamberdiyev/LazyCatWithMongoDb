using Domain.Entities;
using Domain.Models;
using MongoDB.Driver;

namespace Infrastructure.Data;

public class ApplicationDbContext
{
    private readonly IMongoDatabase _database;

    public ApplicationDbContext(string connectionString, string databaseName)
    {
        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(databaseName);
    }

    public IMongoCollection<ApplicationUser> Users => _database.GetCollection<ApplicationUser>("users");
    public IMongoCollection<ApplicationRole> Roles => _database.GetCollection<ApplicationRole>("roles");
}
