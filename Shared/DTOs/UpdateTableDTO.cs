namespace Shared.DTOs
{
    public class UpdateTableDTO
    {
        public string TableName { get; set; }
        public List<ColumnDTO> Columns { get; set; }
        public List<CellDTO> Cells { get; set; }
    }
}
