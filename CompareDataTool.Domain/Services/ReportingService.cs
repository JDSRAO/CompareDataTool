using CompareDataTool.Domain.Interfaces;
using CompareDataTool.Domain.Models;

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

        public async Task GenerateReport(string runId)
        {
            throw new NotImplementedException();
        }
    }
}
