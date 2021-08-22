using Newtonsoft.Json;

namespace CosmosDB.SQL
{
    public class BulkItem
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("pk")]
        public string pk { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }
}
