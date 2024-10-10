using Shared.DTOs;
using WebApp.Models;
using WebApp.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Controllers
{
    public class DatabaseController : Controller
    {
        private readonly IDatabaseService _databaseService;

        public DatabaseController(IDatabaseService databaseService)
        {
            _databaseService = databaseService;
        }


        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Database dto)
        {
            try
            {
                var database = new Database
                {
                    Name = dto.Name,
                    Tables = new List<Table>()
                };

                _databaseService.CreateDatabase(database);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Problem($"An error occurred: {ex.Message}");
            }
        }


        public IActionResult Index()
        {
            try
            {
                var databases = _databaseService.GetDatabases();

                return View(databases);
            }
            catch (Exception ex)
            {
                return Problem($"An error occurred: {ex.Message}");
            }
        }
        

        public IActionResult Delete(string databaseName)
        {
            if (databaseName == null)
            {
                return NotFound();
            }
            var db = _databaseService.GetDatabase(databaseName);
            if (db == null)
            {
                return NotFound();
            }

            return View(db);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(string Name)
        {
            try
            {
                _databaseService.DeleteDatabase(Name);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Problem($"An error occurred: {ex.Message}");
            }
        }

    }
}
