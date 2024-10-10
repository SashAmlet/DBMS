using Shared.DTOs;
using WebApp.Models;

namespace WebApp.Services
{
    public interface IDatabaseService
    {
        void CreateDatabase(Database db);
        public List<Database> GetDatabases();
        Database GetDatabase(string dbName);
        void UpdateDatabase(string dbName, Database db);
        void DeleteDatabase(string dbName);
    }
}
