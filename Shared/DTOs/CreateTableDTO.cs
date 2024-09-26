namespace Shared.DTOs
{
    public class CreateTableDTO
    {
        public string TableName { get; set; }
        public List<ColumnDTO> Columns { get; set; }
    }


}
