using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareDataTool.Domain.Models
{
    public class EntityEnvironmentConfiguration
    {
        public string Name { get; set; } = default!;

        public string Type { get; set; } = default!;

        public Dictionary<string, string> EnvironmentVariables { get; set; } = default!;
    }
}
