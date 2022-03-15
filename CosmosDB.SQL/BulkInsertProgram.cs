using CosmosDB.SQL;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

string containerName = "Builddemo";
string databaseName = "Families";
(IServiceProvider serviceProvider, IConfiguration configuration)  = Setup.InitialSetup();

ICosmosOperations cosmosOperations = serviceProvider.GetService<ICosmosOperations>() ?? throw new Exception(nameof(ICosmosOperations));
FamiliesClient familiesClient = serviceProvider.GetService<FamiliesClient>() ?? throw new Exception(nameof(FamiliesClient));


await CreateContainer();

Console.WriteLine("UploadBulkSequentially");
//await UploadBulkSequentially(); // RUs = 133.4 (without indexing)

Console.WriteLine("Parallel");
await UploadBulkParallel(); // RUs= 86 (without indexing).






//---------------------- Methods ------------------------------

/// <summary>
/// Upload parallely, use Task.WhenAll make sure that exceptions are handled as it wont throw exceptions
/// </summary>
async Task UploadBulkParallel()
{
    double totalRus = 0;
    int errors = 0;
    var itemsToUpload = GetItems();
    var container = familiesClient.GetContainer(databaseName, containerName);
    List<Task> tasks = new List<Task>();
    foreach (var item in itemsToUpload)
    {
        tasks.Add(container.CreateItemAsync(item, new Microsoft.Azure.Cosmos.PartitionKey(item.pk))
            .ContinueWith(async t => {
                if (t.IsFaulted)
                {
                    Console.WriteLine(t.Exception.Message);
                    errors++;
                }

                if (t.Status == TaskStatus.RanToCompletion)
                {
                    var response = await t;
                    totalRus += response.RequestCharge;
                }

            }));
    }

    await Task.WhenAll(tasks);
    Console.WriteLine($"Total Ru's = {totalRus}");
    Console.ReadLine();

}




/// <summary>
/// Upload to cosmos container Sequentially.. Has no indexing
/// </summary>
async Task UploadBulkSequentially()
{
    double totalRus = 0;
    int errors = 0;
    var itemsToUpload = GetItems();
    var container =  familiesClient.GetContainer(databaseName, containerName);

    foreach (var item in itemsToUpload)
    {
        try
        {
            var result = await container.CreateItemAsync(item, new Microsoft.Azure.Cosmos.PartitionKey(item.pk));
            totalRus += result.RequestCharge;
        }
        catch (Exception ex)
        {
            errors++;
            Console.WriteLine(ex.Message);
        }
      
    }

    Console.WriteLine($"Total Ru's = {totalRus}");
    Console.ReadLine();
}

IEnumerable<BulkItem> GetItems()
{
    var items = new List<BulkItem>();
    for (int i = 0; i < 20; i++)
    {
        string id = Guid.NewGuid().ToString();
        items.Add(new BulkItem
        {
            id = id,
            pk = id,
            Date= DateTime.Now,
            Name= $"Demo-{i}-{id}"
        });
    }
    return items;
}

async Task CreateContainer()
{
    await cosmosOperations!.CreateContainerIfNotExists(databaseName, containerName, "/pk");

    Console.WriteLine("Created Container");
}

