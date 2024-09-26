using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using API.Models;

namespace API.Controllers
{
    [ApiController]
    [Route("api/databases/{databaseName}/tables")]
    public class TableController : ControllerBase
    {
        private readonly ITableService _tableService;

        public TableController(ITableService tableService)
        {
            _tableService = tableService;
        }


        [HttpPost]
        public IActionResult CreateTable(string databaseName, [FromBody] CreateTableDTO dto)
        {
            try
            {
                var table = new Table
                {
                    Id = Guid.NewGuid(),
                    Name = dto.TableName,
                    Columns = dto.Columns.Select(c => new Column
                    {
                        Id = c.Id,
                        Name = c.ColumnName,
                        Type = c.ColumnType,
                        DisplayIndex = c.DisplayIndex,
                        IsNullable = c.IsNullable,
                    }).ToList(),
                };

                _tableService.CreateTable(databaseName, table);
                return Ok($"Table '{dto.TableName}' created in database '{databaseName}'.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpGet("{tableName}")]
        public IActionResult GetTable(string databaseName, Guid tableId)
        {
            try
            {
                var table = _tableService.GetTable(databaseName, tableId);
                if (table == null)
                    return NotFound($"Table '{tableId}' not found in database '{databaseName}'.");

                return Ok(table);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("{tableId}")]
        public IActionResult UpdateTable(string databaseName, Guid tableId, [FromBody] UpdateTableDTO dto)
        {
            try
            {
                var table = new Table
                {
                    Id = tableId,
                    Name = dto.TableName,
                    Columns = dto.Columns.Select(c => new Column
                    {
                        Id = c.Id,
                        Name = c.ColumnName,
                        Type = c.ColumnType,
                        DisplayIndex = c.DisplayIndex,
                        IsNullable = c.IsNullable,
                    }).ToList(),

                    Cells = dto.Cells.Select(c => new Cell
                    {
                        Value = c.Value,
                        ColumnId = c.ColumnId,
                        RowNum = c.RowNum,
                    }).ToList(),
                };

                _tableService.UpdateTable(databaseName, tableId, table);
                return Ok($"Table '{tableId}' updated in database '{databaseName}'.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        [HttpDelete("{tableName}")]
        public IActionResult DeleteTable(string databaseName, Guid tableId)
        {
            try
            {
                _tableService.DeleteTable(databaseName, tableId);
                return Ok($"Table '{tableId}' deleted from database '{databaseName}'.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
