using Microsoft.Azure.Cosmos;

namespace CosmosDB.SQL
{
    public class CosmosOperations : ICosmosOperations
    {
        public CosmosOperations(CosmosClient cosmosClient) =>  CosmosClient = cosmosClient;

        public CosmosClient CosmosClient { get; }

        public async Task QueryDocuments(string endpoint, string key, string query)
        {
            var container = CosmosClient.GetContainer(databaseId: "Families", containerId: "Family");
            var iterator = container.GetItemQueryIterator<Family>(query);
            var page1 = await iterator.ReadNextAsync();

            foreach (var item in page1)
            {
                Console.WriteLine($"{item.firstname} has {item.children.Length} children");
            }
        }

        public async Task GetDatabaseDetails(string endpoint, string key)
        {
            var container = CosmosClient.GetDatabaseQueryIterator<DatabaseProperties>();
            var databases = await container.ReadNextAsync();

            foreach (var item in databases)
                Console.WriteLine(item.Id);
        }
    }
}
