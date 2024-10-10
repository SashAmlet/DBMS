using API.Controllers;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Shared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using System.Xml.Linq;

namespace API_testing
{
    public class TableServiceTests
    {
        private readonly Mock<IDatabaseService> _mockDatabaseService;
        private readonly TableService _tableService;

        public TableServiceTests()
        {
            _mockDatabaseService = new Mock<IDatabaseService>();
            _tableService = new TableService(_mockDatabaseService.Object);
        }

        [Fact]
        public void RemoveDuplicateRows_ShouldRemoveDuplicatesAndReturnOk()
        {
            // Arrange
            var dbName = "TestDB";
            var tableId = Guid.NewGuid();

            var column0 = Guid.NewGuid();
            var column1 = Guid.NewGuid();
            var inputCells = new List<Cell>
            {
                new Cell { RowNum = 0, ColumnId = column0, Value = "Value1" }, // duplicate 0
                new Cell { RowNum = 0, ColumnId = column1, Value = "Value2" }, // duplicate 0
                new Cell { RowNum = 1, ColumnId = column0, Value = "Value3" },
                new Cell { RowNum = 1, ColumnId = column1, Value = "Value4" },
                new Cell { RowNum = 2, ColumnId = column0, Value = "Value1" }, // duplicate 0
                new Cell { RowNum = 2, ColumnId = column1, Value = "Value2" }, // duplicate 0
                new Cell { RowNum = 3, ColumnId = column0, Value = "Value5" }, // duplicate 1
                new Cell { RowNum = 3, ColumnId = column1, Value = "Value6" }, // duplicate 1
                new Cell { RowNum = 4, ColumnId = column0, Value = "Value5" }, // duplicate 1
                new Cell { RowNum = 4, ColumnId = column1, Value = "Value6" }, // duplicate 1
            };
            var outputCells = new List<Cell>
            {
                new Cell { RowNum = 0, ColumnId = column0, Value = "Value1" }, // duplicate 0
                new Cell { RowNum = 0, ColumnId = column1, Value = "Value2" }, // duplicate 0
                new Cell { RowNum = 1, ColumnId = column0, Value = "Value3" },
                new Cell { RowNum = 1, ColumnId = column1, Value = "Value4" },
                //new Cell { RowNum = 2, ColumnId = column0, Value = "Value1" }, // duplicate 0
                //new Cell { RowNum = 2, ColumnId = column1, Value = "Value2" }, // duplicate 0
                new Cell { RowNum = 3, ColumnId = column0, Value = "Value5" }, // duplicate 1
                new Cell { RowNum = 3, ColumnId = column1, Value = "Value6" }, // duplicate 1
                //new Cell { RowNum = 4, ColumnId = column0, Value = "Value5" }, // duplicate 1
                //new Cell { RowNum = 4, ColumnId = column1, Value = "Value6" }, // duplicate 1
            };

            var table = new Table
            {
                Id = tableId,
                Cells = inputCells
            };


            var database = new Database
            {
                Name = dbName,
                Tables = new List<Table> { table }
            };


            _mockDatabaseService.Setup(ds => ds.GetDatabase(dbName))
                .Returns(database);

            // Act
            var updatedTable = _tableService.RemoveDuplicateRows(dbName, tableId);

            // Assert
            Assert.NotNull(updatedTable);

            List<Cell> test_group = updatedTable.Cells.ToList();

            var control_group = outputCells
                .OrderBy(c => c.RowNum)
                .ThenBy(c => c.ColumnId)
                .ToList();

            Assert.Equal(control_group.Count(), test_group.Count());
            for (int i = 0; i < control_group.Count; i++)
            {
                Assert.Equal(control_group[i].Value, test_group[i].Value);
                Assert.Equal(control_group[i].ColumnId, test_group[i].ColumnId);
                Assert.Equal(control_group[i].RowNum, test_group[i].RowNum);
            }


            _mockDatabaseService.Verify(ds => ds.UpdateDatabase(dbName, database), Times.Once);
        }
    }
}
