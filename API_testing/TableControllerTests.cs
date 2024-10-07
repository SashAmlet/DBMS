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

namespace API_testing
{
    public class TableControllerTests
    {
        private readonly Mock<ITableService> _mockTableService;
        private readonly TableController _controller;

        public TableControllerTests()
        {
            _mockTableService = new Mock<ITableService>();

            _controller = new TableController(_mockTableService.Object);
        }

        [Fact]
        public void RemoveDuplicateRows_ShouldRemoveDuplicatesAndReturnOk()
        {
            // Arrange
            var databaseName = "TestDB";
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

            _mockTableService.Setup(service => service.GetTable(databaseName, tableId)).Returns(table);
            _mockTableService.Setup(service => service.UpdateTable(databaseName, tableId, It.IsAny<Table>()));

            var controller = new TableController(_mockTableService.Object);

            // Act
            var result = controller.RemoveDuplicateRows(databaseName, tableId) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            var updatedTable = result.Value as Table;
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


            _mockTableService.Verify(service => service.UpdateTable(databaseName, tableId, updatedTable), Times.Once);
        }
    }
}
