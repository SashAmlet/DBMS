using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTOs
{
    public class DatabaseDTO
    {
        public string Name { get; set; } = null!;
        public IEnumerable<TableDTO>? Tables { get; set; }

    }
}
