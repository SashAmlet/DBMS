using API.Models;

namespace API.Repository
{
    public interface IDatabaseRepository
    {
        void SaveDatabase(Database database);
        Database LoadDatabase(string databaseName);
        void UpdateDatabase(Database database);
        void DeleteDatabase(string databaseName);
    }
}
