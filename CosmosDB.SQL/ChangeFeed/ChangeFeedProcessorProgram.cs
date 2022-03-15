using Azure.Identity;
using CosmosDB.SQL;
using Microsoft.Azure.Cosmos;
using System;

// assign "Cosmos DB Built-in Data Contributor" role to user/managed identity to make the below line work
CosmosClient cosmosClient = new CosmosClient("https://mychangefeeddemo.documents.azure.com:443/", new DefaultAzureCredential());

Database cosmosDb = cosmosClient.GetDatabase("sampledb");

Container sourceContainer = cosmosClient.GetContainer("sampledb", "sample");

Container leaseContainer = cosmosClient.GetContainer("sampledb", "productslease");


var builder = sourceContainer.GetChangeFeedProcessorBuilder<Family>("myfeed", async (collection, cancellationToken) =>
{
    foreach (var item in collection)
    {
        Console.WriteLine($"id: {item.id}, Name:{item.firstname}, LastName: {item.familyname}");
    }
});


ChangeFeedProcessor ChangeFeedProcessor = builder
                                .WithInstanceName("mychangefeedprocessor")
                                .WithErrorNotification(async (leastoken, exception) =>
                                {
                                    Console.WriteLine($"Error: {leastoken}: message: {exception.Message}");

                                })
                               .WithLeaseContainer(leaseContainer)
                               .Build();
await ChangeFeedProcessor.StartAsync();
//WithInstanceName Name of host instance
//WithStartTime	Set the pointer (in time) to start looking for changes after
//WithLeaseContainer	Configures the lease container
//WithErrorNotification	Assigns a delegate to handle errors during execution
//WithMaxItems    Quantifies the max number of items in each batch
//WithPollInterval Sets the delay when the processor will poll the change feed for new changes

Console.WriteLine("Press any key to stop");
Console.ReadKey();

await ChangeFeedProcessor.StopAsync();
Console.WriteLine("Press any key to stop");
Console.ReadLine();
