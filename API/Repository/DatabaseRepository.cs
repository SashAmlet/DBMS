﻿using API.Models;
using System.Text.Json;
using Shared;

namespace API.Repository
{
    public class DatabaseRepository : IDatabaseRepository
    {
        public void CreateDatabase(Database database)
        {
            string filePath = Path.Combine(Constants.BasePath, $"{database.Name}.json");

            if (File.Exists(filePath))
            {
                throw new Exception("Database already exists.");
            }

            string jsonData = JsonSerializer.Serialize(database, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonData);
        }

        public Database ReadDatabase(string databaseName)
        {
            var filePath = Path.Combine(Constants.BasePath, $"{databaseName}.json");
            if (!File.Exists(filePath))
            {
                return null;
            }

            var jsonData = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Database>(jsonData);
        }

        public void UpdateDatabase(Database database)
        {
            var filePath = Path.Combine(Constants.BasePath, $"{database.Name}.json");

            string jsonData = JsonSerializer.Serialize(database, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, jsonData);
        }

        public void DeleteDatabase(string databaseName)
        {
            var filePath = Path.Combine(Constants.BasePath, $"{databaseName}.json");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
