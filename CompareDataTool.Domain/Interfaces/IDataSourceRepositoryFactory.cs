namespace CompareDataTool.Domain.Interfaces
{
    public interface IDataSourceRepositoryFactory
    {
        IDataSourceRepository GetDataSourceRepositoryService(string type);
    }
}
