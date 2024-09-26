namespace Shared.DTOs
{
    public class ColumnDTO
    {
        public Guid Id { get; set; }
        public string ColumnName { get; set; }
        public Constants.DataType ColumnType { get; set; }
        public int DisplayIndex { get; set; }
        public bool IsNullable { get; set; }
    }
}
