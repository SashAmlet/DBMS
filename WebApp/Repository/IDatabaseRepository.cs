using WebApp.Models;

namespace WebApp.Repository
{
    public interface IDatabaseRepository
    {
        void CreateDatabase(Database database);
        public List<Database> ReadDatabases();
        Database ReadDatabase(string databaseName);
        void UpdateDatabase(Database database);
        void DeleteDatabase(string databaseName);
    }
}
