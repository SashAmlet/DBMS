using Shared.DTOs;
using WebApp.Models;

namespace WebApp.Services
{
    public interface ITableService
    {
        void CreateTable(string dbName, Table table);
        public List<Table> GetTables(string dbName);
        Table GetTable(string dbName, Guid tableId);
        void UpdateTable(string dbName, Guid tableId, Table table);
        void DeleteTable(string dbName, Guid tableId);
        Table RemoveDuplicateRows(string dbName, Guid tableId);
    }
}
