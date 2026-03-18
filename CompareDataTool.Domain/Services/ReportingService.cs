using CompareDataTool.Domain.Interfaces;
using CompareDataTool.Domain.Models;
using CsvHelper;
using System.Globalization;
using System.Text;

namespace CompareDataTool.Domain.Services
{
    public class ReportingService
    {
        private readonly IAppDataRepository appDataRepository;
        private readonly AppConfiguration appConfiguration;
        private string reportTemplatePath = Path.Combine(Directory.GetCurrentDirectory(), "report.html");
        private readonly string reportBasePath = Path.Combine(Directory.GetCurrentDirectory(), "reports");

        public ReportingService(IAppDataRepository appDataRepository, AppConfiguration appConfiguration)
        {
            this.appConfiguration = appConfiguration;
            this.appDataRepository = appDataRepository;
            if (!Directory.Exists(reportBasePath)) 
            {
                Directory.CreateDirectory(reportBasePath);
            }
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

            List<EntityCountMismatch> entityCountMismatches = await this.GetEntityCountDiscrepenciesAsync(runId);
            List<EntityRecordMismatch> entityRecordMismatch = await this.GetEntityRecordMismatchtDiscrepenciesAsync(runId);
            List<EntityFieldMismatch> entityFieldMismatch = await this.GetEntityFieldMismatchDiscrepenciesAsync(runId);

            var reportTemplate = await File.ReadAllTextAsync(reportTemplatePath);
            reportTemplate = reportTemplate.Replace("@reportGenerationTime", DateTime.UtcNow.ToString("O"));
            reportTemplate = reportTemplate.Replace("@sourceEnvironment", this.appConfiguration.EnvironmentSettings.Source.Name);
            reportTemplate = reportTemplate.Replace("@destinationEnvrionment", this.appConfiguration.EnvironmentSettings.Destination.Name);
            reportTemplate = reportTemplate.Replace("@entityCountMismatches", this.ToHtmlTable(entityCountMismatches, entityRecordMismatch.Count));
            reportTemplate = reportTemplate.Replace("@entityRecordMismatches", this.ToHtmlTable(entityRecordMismatch.Take(10).ToList(), entityRecordMismatch.Count));
            reportTemplate = reportTemplate.Replace("@entityFieldMismatches", this.ToHtmlTable(entityFieldMismatch.Take(10).ToList(), entityRecordMismatch.Count));
            //reportTemplate = reportTemplate.Replace("@reportGenerationTime", null);

            var reportContent = string.Format(reportTemplate, runId);
            var reportPath = Path.Combine(reportBasePath, $"report-{DateTime.Now.ToString("yyyy-MM-dd")}.html");
            await File.WriteAllTextAsync(reportPath, reportContent);
            
            await this.GenerateCsvReportAsync(entityRecordMismatch, $"entityRecordMismatch-{DateTime.Now.ToString("yyyy-MM-dd")}.csv");
            await this.GenerateCsvReportAsync(entityFieldMismatch, $"entityFieldMismatch-{DateTime.Now.ToString("yyyy-MM-dd")}.csv");
        }

        private async Task GenerateCsvReportAsync<T>(List<T> data, string fileName)
        {
            // Define the path for your CSV file
            string filePath = Path.Combine(reportBasePath, fileName);
            // Wrap IDisposable objects in 'using' blocks to ensure they are disposed of properly
            using (var writer = new StreamWriter(filePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                await csv.WriteRecordsAsync(data);
            }
        }

        private async Task<List<EntityCountMismatch>> GetEntityCountDiscrepenciesAsync(string runId)
        {
            int pageNumber = 1;
            List<EntityCountMismatch> entityCountMismatches = new List<EntityCountMismatch>();
            IEnumerable<EntityCountMismatch> currentEntityCountMismatches;
            do
            {
                currentEntityCountMismatches = await this.appDataRepository.GetCountMismatchesAsync(runId, pageNumber, this.appConfiguration.CompareSettings.PageSize);
                entityCountMismatches.AddRange(currentEntityCountMismatches);
                pageNumber++;

            }
            while (currentEntityCountMismatches.Any());
            return entityCountMismatches;
        }

        private async Task<List<EntityRecordMismatch>> GetEntityRecordMismatchtDiscrepenciesAsync(string runId)
        {
            int pageNumber = 1;
            var entityCountMismatches = new List<EntityRecordMismatch>();
            IEnumerable<EntityRecordMismatch> currentEntityCountMismatches;
            do
            {
                currentEntityCountMismatches = await this.appDataRepository.GetEntityRecordMismatchAsync(runId, pageNumber, this.appConfiguration.CompareSettings.PageSize);
                entityCountMismatches.AddRange(currentEntityCountMismatches);
                pageNumber++;

            }
            while (currentEntityCountMismatches.Any());
            return entityCountMismatches;
        }

        private async Task<List<EntityFieldMismatch>> GetEntityFieldMismatchDiscrepenciesAsync(string runId)
        {
            int pageNumber = 1;
            var entityCountMismatches = new List<EntityFieldMismatch>();
            IEnumerable<EntityFieldMismatch> currentEntityCountMismatches;
            do
            {
                currentEntityCountMismatches = await this.appDataRepository.GetEntityFieldMismatchAsync(runId, pageNumber, this.appConfiguration.CompareSettings.PageSize);
                entityCountMismatches.AddRange(currentEntityCountMismatches);
                pageNumber++;

            }
            while (currentEntityCountMismatches.Any());
            return entityCountMismatches;
        }

        public string ToHtmlTable<T>(List<T> list, int totalCount)
        {
            var properties = typeof(T).GetProperties();
            var table = new StringBuilder();
            table.AppendLine($"<table class=\"table table-responsive table-bordered table-striped table-hover\"> <caption> Showing {list.Count}/{totalCount} discrepencies </caption> <thead><tr>");

            // Create header row
            foreach (var prop in properties)
            {
                table.AppendFormat("<th scope=\"col\" class=\"text-center\">{0}</th>", prop.Name).AppendLine();
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
