using CosmosDB.SQL;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Cosmos Operations");
(IServiceProvider serviceProvider, IConfiguration Configuration)  = Setup.InitialSetup();

string endpoint = Configuration["CosmosEndpoint"];
string key = Configuration["MasterKey"];
string query = "SELECT * FROM c where ARRAY_LENGTH(c.children) > 0";

var cosmosOperations = serviceProvider.GetService<ICosmosOperations>();

try
{
    //await ExecuteQuery();
    //await cosmosOperations!.GetAllDatabaseDetails();
    //await cosmosOperations!.CreateContainer("Families", "Demo", "pk");
    //await cosmosOperations!.CreateItemAsync("Families", "Family");
    //await QueryPerPage(pageSize:1);
    //await QueryPerPageContinuation(pageSize: 1);
    await ReplaceDocuments();
}
catch (CosmosException de)
{
    Exception baseException = de.GetBaseException();
    Console.WriteLine("{0} error occurred: {1}", de.StatusCode, de);
}
catch (Exception e)
{
    Console.WriteLine("Error: {0}", e);
}

 async Task ReplaceDocuments()
{
    string query = "select * from c where c.firstname='anish'";
    await foreach (var item in cosmosOperations!.ReplaceDocuments<FamilyExtended>(query))
    {
        Console.WriteLine($"{item.firstname} -- gender {item.gender}");
    }
}

async Task QueryPerPage(int pageSize=1)
{
    await foreach (var item in cosmosOperations.QueryDocumentsPerPage<Family>(query, pageSize))
        Console.WriteLine($"{item.firstname} has {item.children.Length} children");
}

async Task QueryPerPageContinuation(int pageSize = 1)
{
     foreach (var item in await cosmosOperations.QueryDocumentsPerPageContinuationToken<Family>(query, pageSize))
        Console.WriteLine($"{item.firstname} has {item.children.Length} children");
}

async Task ExecuteQuery()
{
    var page1 = await cosmosOperations!.QueryDocuments<Family>(query);

    foreach (var item in page1)
        Console.WriteLine($"{item.firstname} has {item.children.Length} children");
}



