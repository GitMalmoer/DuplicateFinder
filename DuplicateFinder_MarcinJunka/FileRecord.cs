using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DuplicateFinder_MarcinJunka
{
    public record FileRecord
    {
        public string? Path { get; init; }
        public string Name { get; init; }
        public long Size { get; init; }

        public string Extension { get; init; }

        public DateTime DateCreated { get; init; }
        public DateTime DateModified { get; init; }
    }
}
