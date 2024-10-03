using API.Models;

namespace API.Repository
{
    public interface IDatabaseRepository
    {
        void CreateDatabase(Database database);
        Database ReadDatabase(string databaseName);
        void UpdateDatabase(Database database);
        void DeleteDatabase(string databaseName);
    }
}
