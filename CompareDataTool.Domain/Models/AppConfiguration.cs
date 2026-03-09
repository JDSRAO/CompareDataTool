using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareDataTool.Domain.Models
{
    public class AppConfiguration
    {
        public EntityEnvironmentConfiguration Source { get; set; } = default!;

        public EntityEnvironmentConfiguration Destination { get; set; } = default!;

        public EntityMapping[] EntityMappings { get; set; } = default!;
    }
}
