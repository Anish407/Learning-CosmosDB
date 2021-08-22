using CosmosDB.SQL;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

string containerName = "Builddemoo";
string databaseName = "Families";
string partitionKey = "/customerid";
(IServiceProvider serviceProvider, IConfiguration configuration) = Setup.InitialSetup();

ICosmosOperations cosmosOperations = serviceProvider.GetService<ICosmosOperations>() ?? throw new Exception(nameof(ICosmosOperations));
FamiliesClient familiesClient = serviceProvider.GetService<FamiliesClient>() ?? throw new Exception(nameof(FamiliesClient));

string customerId = Guid.NewGuid().ToString();
//This id has been used while saving the second order and when 
// we try to save this id again it will fail. Will use this in the next transaction
string guidId = Guid.NewGuid().ToString();
await CreateContainer();
await InsertCustomerWithOrders();
await UpdateOrder();




async Task UpdateOrder()
{
    var container = familiesClient.GetContainer(databaseName, containerName);

    var result = await container.ReadItemAsync<Customer>(customerId, new PartitionKey(customerId));
    var result2 = await container.ReadItemAsync<CustomerOder[]>(customerId, new PartitionKey(customerId));

    var existingCustomer = result.Resource;

    // update order count and insert 1 more order.
    existingCustomer.ordercount++;

    var order = new CustomerOder { customerid = customerId, id = Guid.NewGuid().ToString(), Item = "Pen", Quantity = 4 };

    var batch = container.CreateTransactionalBatch(new PartitionKey(customerId))
           .ReplaceItem(customerId, existingCustomer)
           .CreateItem(order);

    await ExecuteTransaction(batch);

}

async Task InsertCustomerWithOrders()
{
    string customerid = customerId;
    Console.WriteLine("Insert with transactions");

    var customer = new Customer { customerid = customerId, id = customerId, name = "Anish", ordercount = 1 };
    var order = new CustomerOder { customerid = customerId, id = Guid.NewGuid().ToString(), Item = "Pen", Quantity = 4 };
    var order2 = new CustomerOder { customerid = customerId, id = guidId, Item = "book", Quantity = 2 };


    var container = familiesClient.GetContainer(databaseName, containerName);

    var batch = container.CreateTransactionalBatch(new PartitionKey(customer.customerid))
        .CreateItem(customer)
        .CreateItem(order)
        .CreateItem(order2);

    await ExecuteTransaction(batch);
}

async Task ExecuteTransaction(TransactionalBatch batch)
{
    var batchResult = await batch.ExecuteAsync();
    using (batchResult)
        if (batchResult.IsSuccessStatusCode)
        {
            Console.WriteLine("Successfull ");
            WriteResponse();
        }
        else
        {
            Console.WriteLine("Failed Records ");
            WriteResponse();
        }

    void WriteResponse()
    {
        for (int i = 0; i < batchResult.Count; i++)
        {
            var document = batchResult.GetOperationResultAtIndex<dynamic>(i);
            var result = document.Resource;
            Console.WriteLine(result);
        }
    }


}

async Task CreateContainer()
{

    // store a customer and his orders in the same container using the same partition key in a logical partition
    await cosmosOperations!.CreateContainerIfNotExists(databaseName, containerName,partitionKey);

    Console.WriteLine("Created Container");
}



public class Customer
{
    [JsonProperty("id")]
    public string id { get; set; }

    [JsonProperty("customerid")] // this name shud match the partitionkey name
    public string customerid { get; set; }

    [JsonProperty("name")]
    public string name { get; set; }

    [JsonProperty("orderCount")]
    public int ordercount { get; set; }
}

public class CustomerOder
{
    [JsonProperty("id")]
    public string id { get; set; }

    [JsonProperty("customerid")]
    public string customerid { get; set; }

    [JsonProperty("item")]
    public string Item { get; set; }

    [JsonProperty("quantity")]
    public int Quantity { get; set; }
}