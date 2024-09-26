using Shared.DTOs;
using API.Models;

namespace API.Services
{
    public class TableService : ITableService
    {
        private readonly IDatabaseService _databaseService;

        public TableService(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public void CreateTable(string dbName, Table table)
        {
            var database = _databaseService.GetDatabase(dbName) as Database;
            if (database == null)
                throw new Exception($"Database '{dbName}' not found.");
            
            if (database.Tables.Any(t => t.Name == table.Name))
                throw new Exception($"Table '{table.Name}' already exists in database '{dbName}'.");

            database.Tables.Add(table);
            _databaseService.UpdateDatabase(dbName, database);
        }

        public Table GetTable(string dbName, Guid tableId)
        {
            var database = _databaseService.GetDatabase(dbName);
            return database?.Tables.FirstOrDefault(t => t.Id == tableId);
        }

        public void UpdateTable(string dbName, Guid tableId, Table table)
        {
            var database = _databaseService.GetDatabase(dbName);
            if (database == null)
                throw new Exception($"Database '{dbName}' not found.");

            var _table = database.Tables.FirstOrDefault(t => t.Id == tableId);
            if (table == null)
                throw new Exception($"Table '{tableId}' not found in database '{dbName}'.");

            _table.Name = table.Name;
            _table.Cells = table.Cells;
            _table.Columns = table.Columns;

            _databaseService.UpdateDatabase(dbName, database);
        }

        public void DeleteTable(string dbName, Guid tableId)
        {
            var database = _databaseService.GetDatabase(dbName);
            if (database == null)
                throw new Exception($"Database '{dbName}' not found.");

            var table = database.Tables.FirstOrDefault(t => t.Id == tableId);
            if (table == null)
                throw new Exception($"Table '{tableId}' not found in database '{dbName}'.");

            database.Tables.Remove(table);
            _databaseService.UpdateDatabase(dbName, database);
        }
    }
}
