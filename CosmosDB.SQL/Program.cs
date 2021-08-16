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
    await cosmosOperations.QueryDocuments(endpoint, key, query);
    await cosmosOperations.GetDatabaseDetails(endpoint, key);
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

        var cosmosClient = new CosmosClient(endpoint, key);

        services!.AddSingleton(cosmosClient);
    }
}
