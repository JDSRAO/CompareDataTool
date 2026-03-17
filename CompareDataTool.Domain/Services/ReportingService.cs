using CompareDataTool.Domain.Interfaces;
using CompareDataTool.Domain.Models;
using System.Text;

namespace CompareDataTool.Domain.Services
{
    public class ReportingService
    {
        private readonly IAppDataRepository appDataRepository;
        private readonly AppConfiguration appConfiguration;
        private string reportTemplatePath = Path.Combine(Directory.GetCurrentDirectory(), "report.html");
        private string reportPath = Path.Combine(Directory.GetCurrentDirectory(), $"report-{DateTime.Now.ToString("yyyy-mm-dd")}.html");

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

            //var reportRaw = $@"
            //<!DOCTYPE html>               
            //<html>
            //<head>
            //    <meta charset=""utf-8"">
            //    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/css/bootstrap.min.css"" rel=""stylesheet"" integrity=""sha384-sRIl4kxILFvY47J16cr9ZwB07vP4J8+LH7qKQnuqkuIAvNWLzeN8tE5YBujZqJLB"" crossorigin=""anonymous"">
            //    <script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.8/dist/js/bootstrap.bundle.min.js"" integrity=""sha384-FKyoEForCGlyvwx9Hj09JcYn3nv7wiPVlz7YYwJrWVcXK/BmnVDxM+D2scQbITxI"" crossorigin=""anonymous""></script>
            //    <title>Data reconciliation report</title>
            //</head>
            //    <div clas=""container"">
            //            <div class""row"">
            //                <div class=""col"">
            //                    <section>
            //                        <p> Report Generated at : {DateTime.UtcNow.ToString("O")} UTC </p>
            //                    </section>
            //                </div>
            //            </div>

            //        <div class""row"">
            //                <div class=""col-6"">
            //                    <section>
            //                        <p> Source Environment : {this.appConfiguration.EnvironmentSettings.Source.Name}</p>
            //                    </section>
            //                </div>
            //                <div class=""col-6"">
            //                    <section>
            //                        <p> Destination Environment : {this.appConfiguration.EnvironmentSettings.Destination.Name} </p>
            //                    </section>
            //                </div>
            //            </div>
            //        </div>

            //</html>";

            var reportTemplate = await File.ReadAllTextAsync(reportTemplatePath);
            reportTemplate = reportTemplate.Replace("@reportGenerationTime", DateTime.UtcNow.ToString("O"));
            reportTemplate = reportTemplate.Replace("@sourceEnvironment", this.appConfiguration.EnvironmentSettings.Source.Name);
            reportTemplate = reportTemplate.Replace("@destinationEnvrionment", this.appConfiguration.EnvironmentSettings.Destination.Name);
            reportTemplate = reportTemplate.Replace("@reportGenerationTime", null);
            reportTemplate = reportTemplate.Replace("@reportGenerationTime", null);

            var reportContent = string.Format(reportTemplate, runId);
            await File.WriteAllTextAsync(reportPath, reportContent);
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
