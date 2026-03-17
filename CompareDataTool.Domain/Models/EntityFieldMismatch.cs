namespace CompareDataTool.Domain.Models
{
    public class EntityFieldMismatch
    {
        public string SourceEntity { get; set; } = default!;

        public string DestinationEntity { get; set; } = default!;

        public string RowId { get; set; } = default!;

        public string SourceField { get; set; } = default!;

        public string DestinationField { get; set; } = default!;

        public string SourceValue { get; set; } = default!;

        public string DestinationValue { get; set; } = default!;
    }
}
