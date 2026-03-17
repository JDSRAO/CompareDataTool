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
            var sb = new StringBuilder();
            sb.Append("<table><thead><tr>");

            // Create header row
            foreach (var prop in properties)
            {
                sb.AppendFormat("<th>{0}</th>", prop.Name);
            }
            sb.Append("</tr></thead><tbody>");

            // Create data rows
            foreach (var item in list)
            {
                sb.Append("<tr>");
                foreach (var prop in properties)
                {
                    sb.AppendFormat("<td>{0}</td>", prop.GetValue(item, null));
                }
                sb.Append("</tr>");
            }

            sb.Append("</tbody></table>");
            return sb.ToString();
        }
    }
}
