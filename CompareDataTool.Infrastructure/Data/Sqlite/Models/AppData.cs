namespace CompareDataTool.Infrastructure.Data.Sqlite.Models
{
    public class AppData
    {
        public int Id { get; set; } = default!;

        public string RunId { get; set; } = default!;

        public string Type { get; set; } = default!;

        public string Entity { get; set; } = default!;

        public string Data { get; set; } = default!;

        public string CreatedOn { get; set; } = default!;
    }
}
