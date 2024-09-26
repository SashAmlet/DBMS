namespace Shared.DTOs
{
    public class TableDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public IEnumerable<ColumnDTO> Columns { get; set; } = null!;
        public IEnumerable<CellDTO> Cells { get; set; } = Enumerable.Empty<CellDTO>();
    }
}
