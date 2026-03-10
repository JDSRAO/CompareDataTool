namespace CompareDataTool.Domain.Models
{
    public class CompareSettings
    {
        public int PageSize { get; set; } = default!;

        public int MaxDegreeOfParallelism { get; set; } = default!;

        public bool TrimAndCompare { get; set; } = default!;

        public bool ConvertToLowerAndCompare { get; set; } = default!;
    }
}
