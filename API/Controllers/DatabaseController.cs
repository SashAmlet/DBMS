using Shared.DTOs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;

        public DatabaseController(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [HttpPost]
        public IActionResult CreateDatabase([FromBody] CreateDatabaseDTO dto)
        {
            try
            {
                var database = new Database
                {
                    Name = dto.DatabaseName,
                    Tables = new List<Table>()
                };

                _databaseService.CreateDatabase(database);
                return Ok($"Database '{dto.DatabaseName}' created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("{databaseName}")]
        public IActionResult GetDatabase(string databaseName)
        {
            try
            {
                var database = _databaseService.GetDatabase(databaseName);
                if (database == null)
                    return NotFound($"Database '{databaseName}' not found.");

                return Ok(database);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPut("{databaseName}")]
        public IActionResult UpdateDatabase(string databaseName, [FromBody] UpdateDatabaseDTO dto)
        {
            try
            {
                Database database = new Database
                {
                    Name = databaseName,
                    Tables = dto.Tables.Select(t => new Table
                    {
                        Id = t.Id,
                        Name = t.Name,
                        Columns = t.Columns.Select(c=> new Column
                        {
                            Id = c.Id,
                            Name = c.Name,
                            Type = c.Type,
                            DisplayIndex = c.DisplayIndex,
                            IsNullable = c.IsNullable,
                        }).ToList(),

                        Cells = t.Cells.Select(c => new Cell
                        {
                            Value = c.Value,
                            ColumnId = c.ColumnId,
                            RowNum = c.RowNum,
                        }).ToList(),

                    }).ToList()
                };

                _databaseService.UpdateDatabase(databaseName, database);
                return Ok($"Database '{databaseName}' updated.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("{databaseName}")]
        public IActionResult DeleteDatabase(string databaseName)
        {
            try
            {
                _databaseService.DeleteDatabase(databaseName);
                return Ok($"Database '{databaseName}' deleted.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
