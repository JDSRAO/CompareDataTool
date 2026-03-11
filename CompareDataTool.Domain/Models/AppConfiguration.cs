namespace CompareDataTool.Domain.Models
{
    public class AppConfiguration
    {
        public CompareSettings CompareSettings { get; set; } = default!;

        public EnvironmentSettings EnvironmentSettings { get; set; } = default!;

        public EntityMapping[] EntityMappings { get; set; } = default!;
    }
}
