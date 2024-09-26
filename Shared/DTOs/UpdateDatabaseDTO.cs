namespace Shared.DTOs
{
    public class UpdateDatabaseDTO
    {
        public List<TableDTO> Tables { get; set; } = new List<TableDTO>();
    }
}
