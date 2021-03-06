using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace CosmosDB.SQL
{
    public class CosmosOperations : ICosmosOperations
    {
        public CosmosOperations(FamiliesClient cosmosClient) => CosmosClient = cosmosClient;

        public FamiliesClient CosmosClient { get; }

        public async Task<FeedResponse<T>> QueryDocuments<T>(string query)
        {
            var container = CosmosClient.GetContainer(databaseId: "Families", containerId: "Family");
            var iterator = container.GetItemQueryIterator<T>(query);
            return await iterator.ReadNextAsync();
        }

        public async IAsyncEnumerable<T> QueryDocumentsPerPage<T>(string query, int pageSize = 1)
        {
            var container = CosmosClient.GetContainer(databaseId: "Families", containerId: "Family");
            var iterator = container.GetItemQueryIterator<T>(query, requestOptions: new QueryRequestOptions { MaxItemCount = pageSize });

            while (iterator.HasMoreResults)
            {
                foreach (var item in await iterator.ReadNextAsync())
                    yield return item;
            }
        }

        public async Task<IList<T>> QueryDocumentsPerPageContinuationToken<T>(string query, int pageSize = 1)
        {
            var container = CosmosClient.GetContainer(databaseId: "Families", containerId: "Family");
            var iterator = container.GetItemQueryIterator<T>(query, requestOptions: new QueryRequestOptions { MaxItemCount = pageSize });
            var page = await iterator.ReadNextAsync();
            IList<T> result = new List<T>();
            do
            {
                foreach (var item in page)
                    result.Add(item);

                iterator = container.GetItemQueryIterator<T>(query, page.ContinuationToken, requestOptions: new QueryRequestOptions { MaxItemCount = pageSize });
                page = await iterator.ReadNextAsync();

            } while (page.ContinuationToken != null);
            return result;
        }

        public async IAsyncEnumerable<T> ReplaceDocuments<T>(string query)
        {
            var container = CosmosClient.GetContainer(databaseId: "Families", containerId: "Family");
            var page = container.GetItemQueryIterator<dynamic>(query);

            foreach (var item in await page.ReadNextAsync())
            {
                item.gender = "male";
                var result = await container.ReplaceItemAsync<dynamic>(item, (string)item.id);
                var updated = JsonSerializer.Deserialize<T>(result.Resource.ToString());
                yield return updated;
            }
        }

        public IEnumerable<string> Sample()
        {
            foreach (var item in new string[] { })
                yield return item;
        }

        public async IAsyncEnumerable<string> SampleAsync()
        {
            foreach (var item in await Task.FromResult(new string[] { }))
                yield return item;
        }


        public async Task GetAllDatabaseDetails()
        {
            var container = CosmosClient.GetDatabaseQueryIterator<DatabaseProperties>();
            var databases = await container.ReadNextAsync();

            foreach (var item in databases)
                Console.WriteLine(item.Id);
        }

        public async Task CreateDatabase(string databaseName)
        {
            Console.WriteLine("Create Database");
            var database = await CosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);
            Console.WriteLine($"Created {databaseName} with Database Id: {database.Resource.Id}");
        }

        public async Task DeleteDatabase(string databaseName)
        {
            Console.WriteLine($"Deleting database {databaseName}");
            await CosmosClient.GetDatabase(databaseName).DeleteAsync();
        }
        public async Task CreateContainerIfNotExists(string databaseName, string containerName, string partitionKey)
        {
            Console.WriteLine($"Creating Container {containerName} in database: {databaseName}");
            try
            {
                var database = CosmosClient.GetDatabase(databaseName);
                await database.CreateContainerIfNotExistsAsync(containerName, partitionKey,throughput: 400);
            }
            catch (Exception)
            {

                throw;
            }
          
        }

        public async Task CreateItemAsync(string databaseName, string containerName)
        {
            string familyName = "aravind";
            dynamic item = new
            {
                id = Guid.NewGuid(),
                firstname = $"demo{new Random().Next(1, 999)}",
                familyname = familyName,
                children = new object[]
                {
                   new  {
                     name="Jiya",
                     age=3
                   }
                }
            };

            var container = CosmosClient.GetContainer(databaseName, containerName);
            await container.CreateItemAsync(item, new PartitionKey(familyName));
            Console.WriteLine("Created Item");
        }
    }
}
