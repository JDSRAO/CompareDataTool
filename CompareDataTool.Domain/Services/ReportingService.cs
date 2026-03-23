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

        public async Task<string> GenerateReportAsync(string runId)
        {
            var reportTasks = new List<Task>(this.appConfiguration.EntityMappings.Length);
            foreach (var entityMapping in this.appConfiguration.EntityMappings)
            {
                reportTasks.Add(GenerateRrpotsAsync(runId, entityMapping));
            }

            await Task.WhenAll(reportTasks);
            return reportBasePath;
        }

        private async Task GenerateRrpotsAsync(string runId, EntityMapping entityMapping)
        {
            List<EntityCountMismatch> entityCountMismatches = await this.GetEntityCountDiscrepenciesAsync(runId);
            List<EntityRecordMismatch> entityRecordMismatch = await this.GetEntityRecordMismatchtDiscrepenciesAsync(runId);
            List<EntityFieldMismatch> entityFieldMismatch = await this.GetEntityFieldMismatchDiscrepenciesAsync(runId);

            await GenerateHtmlSummaryAsync(runId, entityCountMismatches, entityRecordMismatch, entityFieldMismatch);

            await this.GenerateCsvReportAsync(entityRecordMismatch, $"entityRecordMismatch-{entityMapping.SourceEntity}-{DateTime.Now.ToString("yyyy-MM-dd")}.csv");
            await this.GenerateCsvReportAsync(entityRecordMismatch, $"entityRecordMismatch-{entityMapping.DestinationEntity}-{DateTime.Now.ToString("yyyy-MM-dd")}.csv");

            await this.GenerateCsvReportAsync(entityFieldMismatch, $"entityFieldMismatch-{entityMapping.SourceEntity}-{DateTime.Now.ToString("yyyy-MM-dd")}.csv");
        }

        private async Task GenerateHtmlSummaryAsync(string runId, List<EntityCountMismatch> entityCountMismatches, List<EntityRecordMismatch> entityRecordMismatch, List<EntityFieldMismatch> entityFieldMismatch)
        {
            var reportTemplate = await File.ReadAllTextAsync(reportTemplatePath);
            reportTemplate = reportTemplate.Replace("@reportGenerationTime", DateTime.UtcNow.ToString("O"));
            reportTemplate = reportTemplate.Replace("@sourceEnvironment", this.appConfiguration.EnvironmentSettings.Source.Name);
            reportTemplate = reportTemplate.Replace("@destinationEnvrionment", this.appConfiguration.EnvironmentSettings.Destination.Name);
            reportTemplate = reportTemplate.Replace("@entityCountMismatches", this.ToHtmlTable(entityCountMismatches, entityCountMismatches.Count));
            reportTemplate = reportTemplate.Replace("@entityRecordMismatches", this.ToHtmlTable(entityRecordMismatch.Take(10).ToList(), entityRecordMismatch.Count));
            reportTemplate = reportTemplate.Replace("@entityFieldMismatches", this.ToHtmlTable(entityFieldMismatch.Take(10).ToList(), entityFieldMismatch.Count));
            //reportTemplate = reportTemplate.Replace("@reportGenerationTime", null);

            var reportContent = string.Format(reportTemplate, runId);
            var reportPath = Path.Combine(reportBasePath, $"report-{DateTime.Now.ToString("yyyy-MM-dd")}.html");
            await File.WriteAllTextAsync(reportPath, reportContent);
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
