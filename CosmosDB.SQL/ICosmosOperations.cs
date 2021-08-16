namespace CosmosDB.SQL
{
    public interface ICosmosOperations
    {
        Task QueryDocuments(string endpoint, string key, string query);
        Task GetDatabaseDetails(string endpoint, string key);
    }
}