namespace CompareDataTool.Domain.Models
{
    public class FieldCompareResult
    {
        public bool Same { get; set; } = default!;

        public string SourceValue { get; set; } = default!;

        public string DestinationValue { get; set; } = default!;
    }
}
