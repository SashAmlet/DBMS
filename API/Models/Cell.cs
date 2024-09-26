namespace API.Models
{
    public class Cell
    {
        public Guid Id { get; set; }
        public object? Value { get; set; }
        public Guid ColumnId { get; set; }
        public int RowNum { get; set; }

    }
}
