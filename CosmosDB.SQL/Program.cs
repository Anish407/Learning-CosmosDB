using CosmosDB.SQL;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

Console.WriteLine("Cosmos Operations");

var Configuration = SetupConfiguration();
string endpoint = Configuration["CosmosEndpoint"];
string key = Configuration["MasterKey"];
string query = "SELECT * FROM c where ARRAY_LENGTH(c.children) > 0";
var serviceProvider = InitializeDI(Configuration);
var cosmosOperations = serviceProvider.GetService<ICosmosOperations>();

try
{
    //await ExecuteQuery();
    //await cosmosOperations!.GetAllDatabaseDetails();
    //await cosmosOperations!.CreateContainer("Families", "Demo", "pk");
    //await cosmosOperations!.CreateItemAsync("Families", "Family");
    await QueryPerPage(pageSize:1);
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

async Task QueryPerPage(int pageSize=1)
{
    await foreach (var item in cosmosOperations.QueryDocumentsPerPage<Family>(query, pageSize))
        Console.WriteLine($"{item.firstname} has {item.children.Length} children");
}

async Task ExecuteQuery()
{
    var page1 = await cosmosOperations!.QueryDocuments<Family>(query);

    foreach (var item in page1)
        Console.WriteLine($"{item.firstname} has {item.children.Length} children");
}


IConfiguration SetupConfiguration()
    => new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, string.Format("..{0}..{0}..{0}", Path.DirectorySeparatorChar),
            "settings.json"),
            optional: true)
            .Build();

IServiceProvider InitializeDI(IConfiguration configuration)
{
    var services = new ServiceCollection();
    services.AddScoped<ICosmosOperations, CosmosOperations>();

    RegisterCosmosClient();



    return services.BuildServiceProvider();

    void RegisterCosmosClient()
    {
        string endpoint = Configuration!["CosmosEndpoint"];
        string key = Configuration["MasterKey"];

        var cosmosClient = new FamiliesClient(endpoint, key);

        services!.AddSingleton(cosmosClient);
    }

    
}
