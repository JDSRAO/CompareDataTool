namespace CompareDataTool.Domain.Models
{
    public class EntityCountMismatch
    {
        public string SourceEntity { get; set; } = default!;

        public string DestinationEntity { get; set; } = default!;

        public int SourceCount { get; set; } = default!;

        public int DestinationCount { get; set; } = default!;
    }
}
