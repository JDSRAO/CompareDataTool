using CompareDataTool.Domain.Interfaces;
using CompareDataTool.Domain.Models;
using System.Text;

namespace CompareDataTool.Domain.Services
{
    public class ReportingService
    {
        private readonly IAppDataRepository appDataRepository;
        private readonly AppConfiguration appConfiguration;

        public ReportingService(IAppDataRepository appDataRepository, AppConfiguration appConfiguration)
        {
            this.appConfiguration = appConfiguration;
            this.appDataRepository = appDataRepository;
        }

        public async Task GenerateReportAsync(string runId)
        {
            // get entity count mismatch

            // get entity record mismatch

            // get entity field mismatch
        }

        public string ToHtmlTable<T>(IEnumerable<T> list)
        {
            var properties = typeof(T).GetProperties();
            var table = new StringBuilder();
            table.AppendLine("<table><thead><tr>");

            // Create header row
            foreach (var prop in properties)
            {
                table.AppendFormat("<th>{0}</th>", prop.Name).AppendLine();
            }
            table.AppendLine("</tr></thead><tbody>");

            // Create data rows
            foreach (var item in list)
            {
                table.AppendLine("<tr>");
                foreach (var prop in properties)
                {
                    table.AppendFormat("<td>{0}</td>", prop.GetValue(item, null)).AppendLine();
                }
                table.AppendLine("</tr>");
            }

            table.AppendLine("</tbody></table>");
            return table.ToString();
        }
    }
}
