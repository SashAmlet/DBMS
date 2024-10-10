using WebApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using WebApp.Models;
using System.Xml.Linq;
using System.Globalization;
using Shared;

namespace WebApp.Controllers
{
    public class TableController : Controller
    {
        private readonly ITableService _tableService;

        public TableController(ITableService tableService)
        {
            _tableService = tableService;
        }


        public IActionResult Create(string databaseName)
        {
            ViewData["databaseName"] = databaseName;
            return View();
        }
        [HttpPost]
        public IActionResult Create(string databaseName, Table table)
        {
            try
            {
                table.Id = Guid.NewGuid();
                List<Column> columns = new List<Column>();
                foreach (var clmn in table.Columns)
                {
                    clmn.Id = Guid.NewGuid();
                    if (clmn.Name != null)
                        columns.Add(clmn);
                }
                table.Columns = columns;
                _tableService.CreateTable(databaseName, table);
                return RedirectToAction(nameof(ViewTables), new { databaseName = databaseName });
            }
            catch (Exception ex)
            {
                return Problem($"An error occurred: {ex.Message}");
            }
        }
        

        public IActionResult ViewTables(string databaseName)
        {
            try
            {
                var tables = _tableService.GetTables(databaseName);
                ViewData["databaseName"] = databaseName;

                return View(tables);
            }
            catch (Exception ex)
            {
                return Problem($"An error occurred: {ex.Message}");
            }
        }
        

        public IActionResult ViewTable(string databaseName, Guid Id)
        {
            try
            {
                var table = _tableService.GetTable(databaseName, Id);
                if (table == null)
                    return NotFound($"Table '{Id}' not found in database '{databaseName}'.");

                ViewData["databaseName"] = databaseName;
                return View(table);
            }
            catch (Exception ex)
            {
                return Problem($"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        public IActionResult ViewTable(string databaseName, Guid Id, [FromForm] List<Cell> cells)
        {
            try
            {
                var table = _tableService.GetTable(databaseName, Id);

                foreach (var cell in cells)
                {
                    string errorMessage;
                    object cellValue = cell.Value;
                    Column column = table.Columns.First(c => c.Id == cell.ColumnId);

                    bool isValid = ValidateCell(cellValue, column.Type, column.IsNullable, column.DisplayIndex, cell.RowNum, out errorMessage);

                    if (!isValid)
                    {
                        TempData["ErrorMessage"] = $"{errorMessage}";
                        return RedirectToAction(nameof(ViewTable), new { databaseName = databaseName, Id = Id });
                    }

                }

                table.Cells = cells;

                _tableService.UpdateTable(databaseName, Id, table);
                return RedirectToAction(nameof(ViewTables), new { databaseName = databaseName });
            }
            catch (Exception ex)
            {
                return Problem($"An error occurred: {ex.Message}");
            }
        }
        private static bool ValidateCell(object value, Constants.DataType dataType, bool isNullable, int column, int row, out string errorMessage)
        {
            errorMessage = string.Empty;

            // 1. Check for nullability
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                if (!isNullable)
                {
                    errorMessage = $"Cell ({column}, {row}) - Value cannot be null.";
                    return false;
                }
                return true;
            }

            // 2. Checking the data type
            switch (dataType)
            {
                case Constants.DataType.Integer:
                    if (!int.TryParse(value.ToString(), out _))
                    {
                        errorMessage = $"Cell ({column}, {row}) - Invalid integer value.";
                        return false;
                    }
                    break;

                case Constants.DataType.Real:
                    if (!double.TryParse(value.ToString(), out _))
                    {
                        errorMessage = $"Cell ({column}, {row}) - Invalid real (floating-point) value.";
                        return false;
                    }
                    break;

                case Constants.DataType.Char:
                    if (value.ToString().Length != 1)
                    {
                        errorMessage = $"Cell ({column}, {row}) - Invalid char value. It should be a single character.";
                        return false;
                    }
                    break;

                case Constants.DataType.String:
                    if (value.ToString().Length > 255)
                    {
                        errorMessage = $"Cell ({column}, {row}) - String length exceeds the maximum allowed length of 255 characters.";
                        return false;
                    }
                    break;

                case Constants.DataType.Time:
                    if (!TimeSpan.TryParseExact(value.ToString(), "hh\\:mm\\:ss", CultureInfo.InvariantCulture, out TimeSpan timeValue))
                    {
                        errorMessage = $"Cell ({column}, {row}) - Invalid time format. Expected format is HH:MM:SS.";
                        return false;
                    }
                    break;

                case Constants.DataType.TimeLvl:
                    // Split a string by the "-" character
                    var timeRangeParts = value.ToString().Split('-');
                    if (timeRangeParts.Length != 2)
                    {
                        errorMessage = $"Cell ({column}, {row}) - Invalid time range format. Expected format is HH:MM:SS - HH:MM:SS.";
                        return false;
                    }

                    // Trying to parse the start and end of a time interval
                    if (!TimeSpan.TryParseExact(timeRangeParts[0], "hh\\:mm\\:ss", CultureInfo.InvariantCulture, out TimeSpan startTime) ||
                        !TimeSpan.TryParseExact(timeRangeParts[1], "hh\\:mm\\:ss", CultureInfo.InvariantCulture, out TimeSpan endTime))
                    {
                        errorMessage = $"Cell ({column}, {row}) - Invalid time range format. Both start and end times must be in HH:MM:SS format.";
                        return false;
                    }

                    // Checking if the start is less than the end
                    if (startTime >= endTime)
                    {
                        errorMessage = $"Cell ({column}, {row}) - Invalid time range in row. Start time must be earlier than end time.";
                        return false;
                    }
                    break;

                default:
                    errorMessage = "Unsupported data type.";
                    return false;
            }

            // 3. All checks passed
            return true;
        }


        public IActionResult Delete(string databaseName, Guid Id)
        {
            if (databaseName == null || Id == Guid.Empty)
            {
                return NotFound();
            }
            var table = _tableService.GetTable(databaseName, Id);
            if (table == null)
            {
                return NotFound();
            }
            ViewData["databaseName"] = databaseName;
            return View(table);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(string databaseName, Guid Id)
        {
            try
            {
                _tableService.DeleteTable(databaseName, Id);
                return RedirectToAction(nameof(ViewTables), new { databaseName = databaseName });
            }
            catch (Exception ex)
            {
                return Problem($"An error occurred: {ex.Message}");
            }
        }


        public IActionResult RemoveDuplicateRows(string databaseName, Guid Id)
        {
            try
            {
                var table = _tableService.RemoveDuplicateRows(databaseName, Id);

                return RedirectToAction(nameof(ViewTables), new { databaseName = databaseName });
            }
            catch (Exception ex)
            {
                return Problem($"An error occurred: {ex.Message}");
            }

        }

    }
}
