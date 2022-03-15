using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CosmosDB.SQL
{
    public class Setup
    {
        public static (IServiceProvider, IConfiguration) InitialSetup()
        {
            var configuration = SetupConfiguration();
            var services = new ServiceCollection();
            services.AddScoped<ICosmosOperations, CosmosOperations>();

            RegisterCosmosClient();



            return (services.BuildServiceProvider(), configuration);

            void RegisterCosmosClient()
            {
                string endpoint = configuration!["CosmosEndpoint"];
                string key = configuration["MasterKey"];

                //Inorder to allow bulk execution we need to enable it in the cosmos client
                var cosmosClient = new FamiliesClient(endpoint, key, new CosmosClientOptions { AllowBulkExecution = true });

                services!.AddSingleton(cosmosClient);
            }

            IConfiguration SetupConfiguration()
              => new ConfigurationBuilder()
                      .AddJsonFile(Path.Combine(AppContext.BaseDirectory, string.Format("..{0}..{0}..{0}", Path.DirectorySeparatorChar),
                      "settings.json"),
                      optional: true)
                      .Build();

        }
    }
}
