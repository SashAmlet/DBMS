namespace WebApp.Models
{
    public class Database
    {
        public string Name { get; set; } = null!;
        public List<Table> Tables { get; set; } = new List<Table>();

    }
}
