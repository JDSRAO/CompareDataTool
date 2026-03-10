namespace CompareDataTool.Domain.Models
{
    public class EntityMapping
    {
        public string SourceEntity { get; set; } = default!;

        public string DestinationEntity { get; set; } = default!;

        public PrimaryKeyMapping PrimaryKeyMapping { get; set; } = default!;

        public FieldMapping[] FieldMappings { get; set; } = default!;
    }
}