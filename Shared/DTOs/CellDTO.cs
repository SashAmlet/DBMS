namespace Shared.DTOs
{
    public class CellDTO
    {
        public object? Value { get; set; }
        public Guid ColumnId { get; set; }
        public int RowNum { get; set; }
    }
}
