using Shared.DTOs;
using API.Models;

namespace API.Services
{
    public interface IDatabaseService
    {
        void CreateDatabase(Database db);
        Database GetDatabase(string dbName);
        void UpdateDatabase(string dbName, Database db);
        void DeleteDatabase(string dbName);
    }
}
