using Microsoft.Azure.Cosmos;

namespace CosmosDB.SQL
{
    public interface ICosmosOperations
    {
        Task<FeedResponse<T>> QueryDocuments<T>(string query);
        Task GetAllDatabaseDetails();
        Task CreateDatabase(string databaseName);
        Task DeleteDatabase(string databaseName);
        Task CreateContainerIfNotExists(string databaseName, string containerName, string partitionKey);
        Task CreateItemAsync(string databaseName, string containerName);
        IAsyncEnumerable<T> QueryDocumentsPerPage<T>(string query, int pageSize = 1);
        Task<IList<T>> QueryDocumentsPerPageContinuationToken<T>(string query, int pageSize = 1);
        IAsyncEnumerable<T> ReplaceDocuments<T>(string query);
    }
}