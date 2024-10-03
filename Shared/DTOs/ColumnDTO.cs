namespace Shared.DTOs
{
    public class ColumnDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Constants.DataType Type { get; set; }
        public int DisplayIndex { get; set; }
        public bool IsNullable { get; set; }
    }
}
