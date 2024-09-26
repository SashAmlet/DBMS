using Shared;
namespace API.Models
{
    public class Column
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public Constants.DataType Type { get; set; }
        public int DisplayIndex { get; set; }
        public bool IsNullable { get; set; }

    }

    
}
