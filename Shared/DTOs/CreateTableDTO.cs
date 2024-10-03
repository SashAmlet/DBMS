namespace Shared.DTOs
{
    public class CreateTableDTO
    {
        public Guid TableId { get; set; }
        public string TableName { get; set; }
        public List<ColumnDTO> Columns { get; set; }
    }


}
