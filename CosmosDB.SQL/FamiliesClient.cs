using Azure.Core;
using Microsoft.Azure.Cosmos;

namespace CosmosDB.SQL
{
    public class FamiliesClient : CosmosClient
    {
        public FamiliesClient(string connectionString, CosmosClientOptions clientOptions = null) : base(connectionString, clientOptions)
        {
        }

        public FamiliesClient(string accountEndpoint, string authKeyOrResourceToken, CosmosClientOptions clientOptions = null) : base(accountEndpoint, authKeyOrResourceToken, clientOptions)
        {
        }

        public FamiliesClient(string accountEndpoint, TokenCredential tokenCredential, CosmosClientOptions clientOptions = null) : base(accountEndpoint, tokenCredential, clientOptions)
        {
        }

        protected FamiliesClient()
        {
        }
    }
}
