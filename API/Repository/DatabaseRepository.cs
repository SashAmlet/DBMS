using API.Models;
using System.Text.Json;
using Shared;

namespace API.Repository
{
    public class DatabaseRepository : IDatabaseRepository
    {
        private readonly string _dbDirectory = Constants.BasePath;

        public DatabaseRepository()
        {
            if (!Directory.Exists(_dbDirectory))
            {
                Directory.CreateDirectory(_dbDirectory);
            }
        }

        public void SaveDatabase(Database database)
        {
            string filePath = Path.Combine(_dbDirectory, $"{database.Name}.json");

            if (File.Exists(filePath))
            {
                throw new Exception("Database already exists.");
            }

            string jsonData = JsonSerializer.Serialize(database, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonData);
        }

        public Database LoadDatabase(string databaseName)
        {
            var filePath = Path.Combine(_dbDirectory, $"{databaseName}.json");
            if (!File.Exists(filePath))
            {
                return null;
            }

            var jsonData = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Database>(jsonData);
        }

        public void UpdateDatabase(Database database)
        {
            var filePath = Path.Combine(_dbDirectory, $"{database.Name}.json");

            string jsonData = JsonSerializer.Serialize(database, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonData);
        }

        public void DeleteDatabase(string databaseName)
        {
            var filePath = Path.Combine(_dbDirectory, $"{databaseName}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
