namespace CompareDataTool.Domain.Models
{
    public class EntityRecordMismatch
    {
        public string Entity { get; set; } = default!;

        public string RowId { get; set; } = default!;

        public bool ExistsInSource { get; set; } = default!;

        public bool ExistsInDestination { get; set; } = default!;
    }
}
