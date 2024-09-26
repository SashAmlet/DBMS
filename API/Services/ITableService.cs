using Shared.DTOs;
using API.Models;

namespace API.Services
{
    public interface ITableService
    {
        void CreateTable(string dbName, Table table);
        Table GetTable(string dbName, Guid tableId);
        void UpdateTable(string dbName, Guid tableId, Table table);
        void DeleteTable(string dbName, Guid tableId);
    }
}
