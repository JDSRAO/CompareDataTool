namespace CompareDataTool.Domain.Models
{
    public class AppConfiguration
    {
        public CompareSettings CompareSettings { get; set; } = default!;

        public EntityEnvironmentConfiguration Source { get; set; } = default!;

        public EntityEnvironmentConfiguration Destination { get; set; } = default!;

        public EntityMapping[] EntityMappings { get; set; } = default!;
    }
}
