using CompareDataTool.Domain.Interfaces;
using CompareDataTool.Domain.Models;
using CompareDataTool.Infrastructure.DataSources.Dataverse;
using CompareDataTool.Infrastructure.DataSources.SQL;

namespace CompareDataTool.Infrastructure.DataSources
{
    public class DataSourceFactory : IDataSourceRepositoryFactory
    {
        private readonly IServiceProvider serviceProvider;

        public DataSourceFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IDataSourceRepository GetDataSourceRepositoryService(string type)
        {
            return type.ToLower() switch
            {
                DataSourceTypes.Source => (IDataSourceRepository)this.serviceProvider.GetService(typeof(DataverseDataSource)),
                DataSourceTypes.Destination => (IDataSourceRepository)this.serviceProvider.GetService(typeof(SqlDataSource)),
                _ => throw new ArgumentException("Invalid type")
            };
        }
    }
}
