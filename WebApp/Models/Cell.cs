using System.ComponentModel.DataAnnotations.Schema;

namespace WebApp.Models
{
    public class Cell
    {
        public Guid Id { get; set; }
        public string? Value { get; set; }
        public Guid ColumnId { get; set; }
        public int RowNum { get; set; }

    }
}
