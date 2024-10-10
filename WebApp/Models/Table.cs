namespace WebApp.Models
{
    public class Table
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public IEnumerable<Column> Columns { get; set;} = null!;
        public IEnumerable<Cell> Cells { get; set; } = Enumerable.Empty<Cell>();

    }
}
