using Shared.DTOs;
using WebApp.Models;

namespace WebApp.Services
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

        public List<Table> GetTables(string dbName)
        {
            var database = _databaseService.GetDatabase(dbName);
            return database?.Tables;
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
    
        public Table RemoveDuplicateRows(string dbName, Guid tableId)
        {
            var table = GetTable(dbName, tableId);
            if (table == null)
                throw new Exception($"Table '{tableId}' not found in database '{dbName}'.");

            // Group the elements by RowNum and select only the first element from each group
            var uniqueCells = table.Cells
                    .GroupBy(cell => cell.RowNum)
                    .Select(group => group.OrderBy(cell => cell.ColumnId).ToList())
                    .GroupBy(row => string.Join(";", row.Select(cell => cell.Value)))
                    .Select(group => group.First())
                    .SelectMany(row => row)
                    .ToList();

            table.Cells = uniqueCells;

            UpdateTable(dbName, tableId, table);

            return table;
        }
    }
}
